using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CityRadioController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private GameObject _hud;
    [SerializeField] private CityManager _cityManager;
    [SerializeField] private CityRadioBank[] _banks;
    [Header("Timing")]
    [SerializeField] private Vector2 _initialDelay = new Vector2(2f, 4f);
    [SerializeField] private Vector2 _quietGap = new Vector2(15f, 25f);
    [Header("Voice")]
    [Range(0f, 1f)] [SerializeField] private float _voiceVolume = 0.85f;
    [SerializeField] private float _obstacleFadeDuration = 0.5f;
    private bool _fadingAfterHit;
    private float _fadeElapsed;
    private float _fadeStartVolume;

    private AudioSource _source;
    private BackgroundMusicManager _duckedMusic;
    private CityRadioBank _bank;
    private int[] _order = new int[0];
    private int _cursor;
    private int _lastIndex = -1;
    private float _remaining;
    private bool _wasEligible;
    private bool _wasSpeaking;
    private bool _focused = true;
    private bool _applicationPaused;

    public bool IsRunning { get; private set; }
    public bool IsSpeaking => _source != null && _source.isPlaying;
    public string StationLabel => _bank != null ? _bank.StationLabel : "CITY RADIO";
    public string CityKey => _bank != null ? _bank.CityKey : "";
    public event System.Action StatusChanged;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
        _source.spatialBlend = 0f;
        _source.ignoreListenerPause = false;
    }

    private void OnEnable()
    {
        SimplePlayerController.ObstacleHit += OnObstacleHit;
        if (_cityManager != null) _cityManager.CityChanged += OnCityChanged;
        SelectBank();
    }

    private void OnDisable()
    {
        SimplePlayerController.ObstacleHit -= OnObstacleHit;
        if (_cityManager != null) _cityManager.CityChanged -= OnCityChanged;
        Silence();
    }

    private void OnApplicationFocus(bool focused)
    {
        _focused = focused;
        if (!focused) Silence();
    }

    private void OnApplicationPause(bool paused)
    {
        _applicationPaused = paused;
        if (paused) Silence();
    }

    private void OnObstacleHit()
    {
        if (_fadingAfterHit) return;
        if (!IsSpeaking || _obstacleFadeDuration <= 0f) { Silence(); return; }
        _fadingAfterHit = true;
        _fadeElapsed = 0f;
        _fadeStartVolume = _source.volume;
        IsRunning = false;
        _wasEligible = false;
        StatusChanged?.Invoke();
    }

    private void OnCityChanged(int index) => SelectBank();

    private void SelectBank()
    {
        Silence();
        _bank = null;
        string city = _cityManager != null ? _cityManager.CurrentCityName : "";
        if (_banks != null)
            for (int i = 0; i < _banks.Length; i++)
                if (_banks[i] != null && _banks[i].CityKey == city) { _bank = _banks[i]; break; }
        int count = 0;
        if (_bank != null && _bank.Entries != null)
            for (int i = 0; i < _bank.Entries.Length; i++)
                if (_bank.Entries[i] != null && _bank.Entries[i].clip != null) count++;
        _order = new int[count];
        if (count > 0)
        {
            int n = 0;
            for (int i = 0; i < _bank.Entries.Length; i++)
                if (_bank.Entries[i] != null && _bank.Entries[i].clip != null) _order[n++] = i;
        }
        _lastIndex = -1;
        _cursor = count;
        StatusChanged?.Invoke();
    }

    private bool Eligible()
    {
        var runner = SimplePlayerController.Instance;
        var game = GameStateController.Instance;
        var ui = UImanager.uimanager;
        var objectives = LevelObjectiveTracker.Instance;
        var music = BackgroundMusicManager.Instance;
        return music != null && music.isActiveAndEnabled && !music.IsMuted() &&
            _focused && !_applicationPaused && Time.timeScale > 0f &&
            _hud != null && _hud.activeInHierarchy && runner != null &&
            runner.isActiveAndEnabled && runner.IsMoving && !runner.IsInRoadsideIdlePose &&
            game != null && game.IsPlaying() && ui != null && !ui.IsCountingDown &&
            (objectives == null || !objectives.IsLevelCompleted());
    }

    private bool SoundEnabled()
    {
        return PlayerPrefs.GetInt("SoundEnabled", 1) == 1 &&
            (SoundEffectsManager.Instance == null || SoundEffectsManager.Instance.IsSoundEnabled());
    }

    private void Update()
    {
        // CityManager.Awake may execute after this service's OnEnable.
        if (_cityManager != null && (_bank == null || _bank.CityKey != _cityManager.CurrentCityName))
        {
            if (_bank != null || _order.Length != 0) SelectBank();
            else if (_banks != null)
                for (int i = 0; i < _banks.Length; i++)
                    if (_banks[i] != null && _banks[i].CityKey == _cityManager.CurrentCityName) { SelectBank(); break; }
        }
        if (_fadingAfterHit)
        {
            // Fail/countdown can freeze game time; the impact tail must still finish.
            if (!_focused || _applicationPaused || !SoundEnabled()) { Silence(); return; }
            var music = BackgroundMusicManager.Instance;
            if (music == null || !music.isActiveAndEnabled || music.IsMuted()) { Silence(); return; }
            _fadeElapsed += Time.unscaledDeltaTime;
            _source.volume = Mathf.Lerp(_fadeStartVolume, 0f,
                Mathf.Clamp01(_fadeElapsed / _obstacleFadeDuration));
            if (_fadeElapsed >= _obstacleFadeDuration || !IsSpeaking) Silence();
            return;
        }
        bool eligible = Eligible();
        if (!eligible) { if (IsRunning || _wasEligible || IsSpeaking) Silence(); return; }
        if (!IsRunning) { IsRunning = true; StatusChanged?.Invoke(); }
        if (!SoundEnabled())
        {
            if (IsSpeaking) _source.Stop();
            _wasEligible = false;
            NotifySpeaking();
            return;
        }
        _source.volume = _voiceVolume * (SoundEffectsManager.Instance != null ?
            SoundEffectsManager.Instance.sfxVolume : PlayerPrefs.GetFloat("SFXVolume", 0.5f));
        if (!_wasEligible)
        {
            _wasEligible = true;
            _remaining = Random.Range(_initialDelay.x, _initialDelay.y);
        }
        if (_wasSpeaking && !IsSpeaking) _remaining = Random.Range(_quietGap.x, _quietGap.y);
        if (!IsSpeaking && _order.Length > 0)
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) PlayNext();
        }
        NotifySpeaking();
    }

    private void PlayNext()
    {
        if (_cursor >= _order.Length)
        {
            for (int i = _order.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = _order[i]; _order[i] = _order[j]; _order[j] = temp;
            }
            if (_order.Length > 1 && _order[0] == _lastIndex)
            {
                int j = Random.Range(1, _order.Length);
                int temp = _order[0]; _order[0] = _order[j]; _order[j] = temp;
            }
            _cursor = 0;
        }
        _lastIndex = _order[_cursor++];
        _source.clip = _bank.Entries[_lastIndex].clip;
        _source.Play();
    }

    private void UpdateMusicDucking(bool speaking)
    {
        var music = BackgroundMusicManager.Instance;
        if (_duckedMusic != music)
        {
            if (_duckedMusic != null) _duckedMusic.SetRadioSpeaking(false);
            _duckedMusic = music;
        }
        if (_duckedMusic != null) _duckedMusic.SetRadioSpeaking(speaking);
    }

    private void NotifySpeaking()
    {
        bool speaking = IsSpeaking;
        UpdateMusicDucking(speaking);
        if (_wasSpeaking == speaking) return;
        _wasSpeaking = speaking;
        StatusChanged?.Invoke();
    }

    private void Silence()
    {
        _fadingAfterHit = false;
        _fadeElapsed = 0f;
        if (_source != null) _source.Stop();
        UpdateMusicDucking(false);
        bool changed = IsRunning || _wasSpeaking;
        IsRunning = false;
        _wasEligible = false;
        _wasSpeaking = false;
        _remaining = 0f;
        if (changed) StatusChanged?.Invoke();
    }
}
