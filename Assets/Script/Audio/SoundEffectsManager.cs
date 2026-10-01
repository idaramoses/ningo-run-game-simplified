using UnityEngine;

public class SoundEffectsManager : MonoBehaviour
{
    public static SoundEffectsManager Instance { get; private set; }
    
    [Header("Sound Effects")]
    public AudioClip buttonClickSound;
    public AudioClip coinCollectSound;
    public AudioClip letterCollectSound;
    public AudioClip obstacleHitSound;
    public AudioClip jumpSound;
    
    [Header("Visual Effects")]
    public GameObject coinCollectEffectPrefab;
    [Tooltip("Burst effect spawned where a word letter is picked up.")]
    public GameObject letterCollectEffectPrefab;
    
    [Header("Settings")]
    public float sfxVolume = 1f;
    
    private AudioSource sfxAudioSource;
    private const string SOUND_ENABLED_KEY = "SoundEnabled";
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Create AudioSource for sound effects
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.loop = false;
    }
    
    private void Start()
    {
        // Load sound settings
        bool soundEnabled = PlayerPrefs.GetInt(SOUND_ENABLED_KEY, 1) == 1;
        sfxAudioSource.mute = !soundEnabled;
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }
    
    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }
    
    public void PlayCoinCollect()
    {
        PlaySound(coinCollectSound);
    }
    
    public void PlayLetterCollect()
    {
        PlaySound(letterCollectSound);
    }
    
    public void PlayObstacleHit()
    {
        PlaySound(obstacleHitSound);
    }
    
    public void PlayJump()
    {
        PlaySound(jumpSound);
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip, sfxVolume);
        }
    }
    
    public void SetSoundEnabled(bool enabled)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.mute = !enabled;
            Debug.Log($"[AudioManager] Sound effects {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    public bool IsSoundEnabled()
    {
        return sfxAudioSource != null && !sfxAudioSource.mute;
    }

    public void SpawnCoinCollectEffect(Vector3 position)
    {
        SpawnBurstEffect(coinCollectEffectPrefab, position);
    }

    public void SpawnLetterCollectEffect(Vector3 position)
    {
        SpawnBurstEffect(letterCollectEffectPrefab, position);
    }

    private void SpawnBurstEffect(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        GameObject effect = null;
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.AddPool(prefab.name, prefab, 10);
            effect = ObjectPoolManager.Instance.SpawnFromPool(prefab.name, position, Quaternion.identity);
        }
        else
        {
            effect = Instantiate(prefab, position, Quaternion.identity);
        }

        if (effect != null)
        {
            // Set layer to the same layer as other temporary/UI particles or default
            effect.layer = LayerMask.NameToLayer("Default");

            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps == null) ps = effect.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                var main = ps.main;
                main.loop = false; // single burst explosion

                // Get maximum lifetime to destroy/return cleanly
                float maxLifetime = main.startLifetime.mode == ParticleSystemCurveMode.Constant
                    ? main.startLifetime.constant
                    : main.startLifetime.constantMax;

                StartCoroutine(DeactivateEffectAfter(effect, main.duration + maxLifetime));
            }
            else
            {
                StartCoroutine(DeactivateEffectAfter(effect, 1.5f));
            }
        }
    }

    private System.Collections.IEnumerator DeactivateEffectAfter(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect != null)
        {
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.ReturnToPool(effect);
            }
            else
            {
                Destroy(effect);
            }
        }
    }
}
