using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance { get; private set; }

    [Header("Music Tracks")]
    [Tooltip("Music for home screen/menu")]
    public AudioClip homeMusicTrack;
    [Tooltip("Music for gameplay")]
    public AudioClip gameplayMusicTrack;
    [Tooltip("Music for game over/failure")]
    public AudioClip failMusicTrack;
    [Tooltip("Music for high score celebration")]
    public AudioClip highScoreMusicTrack;
    
    [Header("Music Settings")]
    public float volume = 0.5f;
    public bool loop = true;
    public bool playHomeOnStart = true;
    
    [Header("Crossfade Settings")]
    public float crossfadeDuration = 1.5f;
    
    [Header("Radio Ducking")]
    [Range(0f, 1f)] [SerializeField] private float _radioMusicMultiplier = 0.2f;
    [SerializeField] private float _radioDuckFadeSeconds = 0.2f;
    [SerializeField] private float _radioRestoreFadeSeconds = 0.7f;
    private bool _radioSpeaking;
    private float _radioGain = 1f;
    private float _currentMusicGain;
    private float _nextMusicGain;

    private AudioSource audioSource1;
    private AudioSource audioSource2;
    private AudioSource currentAudioSource;
    private AudioSource nextAudioSource;
    
    private bool isMuted = false;
    private MusicTrack currentTrack = MusicTrack.None;
    private Coroutine crossfadeCoroutine;

    public enum MusicTrack
    {
        None,
        Home,
        Gameplay,
        Fail,
        HighScore
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource1.loop = loop;
        audioSource1.playOnAwake = false;
        audioSource1.volume = 0f;
        
        audioSource2 = gameObject.AddComponent<AudioSource>();
        audioSource2.loop = loop;
        audioSource2.playOnAwake = false;
        audioSource2.volume = 0f;
        
        currentAudioSource = audioSource1;
        nextAudioSource = audioSource2;
        
        LoadMusicSettings();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (playHomeOnStart && homeMusicTrack != null)
        {
            PlayHomeMusic();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto-switch music based on scene name
        if (scene.name == SceneLoader.GAME_SCENE)
        {
            PlayGameplayMusic();
        }
        else if (scene.name == SceneLoader.HOME_SCENE)
        {
            PlayHomeMusic();
        }
    }
    
    private void LoadMusicSettings()
    {
        isMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", volume);
        volume = savedVolume;
        
        if (isMuted)
        {
            audioSource1.mute = true;
            audioSource2.mute = true;
        }
    }
    
    public void PlayHomeMusic()
    {
        if (currentTrack == MusicTrack.Home && currentAudioSource.isPlaying)
        {
            Debug.Log("[BackgroundMusicManager] Home music already playing");
            return;
        }
        
        SwitchToTrack(homeMusicTrack, MusicTrack.Home);
    }
    
    public void PlayGameplayMusic()
    {
        if (currentTrack == MusicTrack.Gameplay && currentAudioSource.isPlaying)
        {
            Debug.Log("[BackgroundMusicManager] Gameplay music already playing");
            return;
        }
        
        SwitchToTrack(gameplayMusicTrack, MusicTrack.Gameplay);
    }

    public void PlayFailMusic()
    {
        if (currentTrack == MusicTrack.Fail && currentAudioSource.isPlaying)
        {
            Debug.Log("[BackgroundMusicManager] Fail music already playing");
            return;
        }
        
        SwitchToTrack(failMusicTrack, MusicTrack.Fail);
    }
    
    public void PlayHighScoreMusic()
    {
        if (currentTrack == MusicTrack.HighScore && currentAudioSource.isPlaying)
        {
            Debug.Log("[BackgroundMusicManager] HighScore music already playing");
            return;
        }
        
        SwitchToTrack(highScoreMusicTrack, MusicTrack.HighScore);
    }
    
    private void SwitchToTrack(AudioClip newClip, MusicTrack trackType)
    {
        if (newClip == null)
        {
            Debug.LogWarning($"[BackgroundMusicManager] No music clip assigned for {trackType}!");
            return;
        }
        
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
        }
        
        crossfadeCoroutine = StartCoroutine(CrossfadeToTrack(newClip, trackType));
    }
    
    private IEnumerator CrossfadeToTrack(AudioClip newClip, MusicTrack trackType)
    {
        nextAudioSource.clip = newClip;
        _nextMusicGain = 0f;
        nextAudioSource.volume = 0f;
        nextAudioSource.Play();
        
        float elapsed = 0f;
        float startVolume = _currentMusicGain;
        
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / crossfadeDuration;
            
            _currentMusicGain = Mathf.Lerp(startVolume, 0f, t);
            _nextMusicGain = Mathf.Lerp(0f, volume, t);
            ApplyMusicGains();
            
            yield return null;
        }
        
        _currentMusicGain = 0f;
        currentAudioSource.volume = 0f;
        currentAudioSource.Stop();
        _nextMusicGain = volume;
        nextAudioSource.volume = volume * _radioGain;
        
        AudioSource temp = currentAudioSource;
        currentAudioSource = nextAudioSource;
        nextAudioSource = temp;
        _currentMusicGain = volume;
        _nextMusicGain = 0f;
        
        currentTrack = trackType;
        
        Debug.Log($"[BackgroundMusicManager] Switched to {trackType} music");
        
        crossfadeCoroutine = null;
    }
    
    public void StopMusic()
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }
        
        audioSource1.Stop();
        audioSource2.Stop();
        currentTrack = MusicTrack.None;
        
        Debug.Log("[BackgroundMusicManager] Music stopped");
    }
    
    public void PauseMusic()
    {
        audioSource1.Pause();
        audioSource2.Pause();
        Debug.Log("[BackgroundMusicManager] Music paused");
    }
    
    public void ResumeMusic()
    {
        audioSource1.UnPause();
        audioSource2.UnPause();
        Debug.Log("[BackgroundMusicManager] Music resumed");
    }
    
    public void SetRadioSpeaking(bool speaking)
    {
        _radioSpeaking = speaking;
    }

    private void LateUpdate()
    {
        float target = _radioSpeaking ? Mathf.Clamp01(_radioMusicMultiplier) : 1f;
        float seconds = _radioSpeaking ? _radioDuckFadeSeconds : _radioRestoreFadeSeconds;
        _radioGain = Mathf.MoveTowards(_radioGain, target, Time.unscaledDeltaTime / Mathf.Max(0.01f, seconds));
        ApplyMusicGains();
    }

    private void ApplyMusicGains()
    {
        if (currentAudioSource != null) currentAudioSource.volume = _currentMusicGain * _radioGain;
        if (nextAudioSource != null) nextAudioSource.volume = _nextMusicGain * _radioGain;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        
        if (currentAudioSource.isPlaying)
        {
            _currentMusicGain = volume;
            ApplyMusicGains();
        }
        
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        
        Debug.Log($"[BackgroundMusicManager] Volume set to {volume}");
    }
    
    private bool AudioSourcesReady()
    {
        return audioSource1 != null && audioSource2 != null;
    }

    public void Mute()
    {
        isMuted = true;
        PlayerPrefs.SetInt("MusicMuted", 1);
        PlayerPrefs.Save();

        if (AudioSourcesReady())
        {
            audioSource1.mute = true;
            audioSource2.mute = true;
            Debug.Log("[BackgroundMusicManager] Music muted");
        }
        else
        {
            Debug.LogWarning("[BackgroundMusicManager] Mute called before audio sources initialized");
        }
    }
    
    public void Unmute()
    {
        isMuted = false;
        PlayerPrefs.SetInt("MusicMuted", 0);
        PlayerPrefs.Save();

        if (AudioSourcesReady())
        {
            audioSource1.mute = false;
            audioSource2.mute = false;
            Debug.Log("[BackgroundMusicManager] Music unmuted");
        }
        else
        {
            Debug.LogWarning("[BackgroundMusicManager] Unmute called before audio sources initialized");
        }
    }
    
    public void ToggleMute()
    {
        if (isMuted)
        {
            Unmute();
        }
        else
        {
            Mute();
        }
    }
    
    public bool IsMuted()
    {
        return isMuted;
    }
    
    public MusicTrack GetCurrentTrack()
    {
        return currentTrack;
    }
    
    public float GetVolume()
    {
        return volume;
    }
}
