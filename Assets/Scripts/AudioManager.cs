using UnityEngine;

public enum SfxType
{
    AlienExplosion,
    AsteroidExplosion,
    CollectCroissant,
    CollectPowerUp,
    CollectStar,
    Death,
    LevelComplete,
    Magnet,
    PlayerExplosion,
    Snow,
    UiClick,
    UiPlay,
    UiReturn,
    UiSaved,
    Win
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;
    public bool IsMuted => isMuted;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip musicLoop;

    [Header("SFX")]
    [SerializeField] private AudioClip alienExplosion;
    [SerializeField] private AudioClip asteroidExplosion;
    [SerializeField] private AudioClip collectCroissant;
    [SerializeField] private AudioClip collectPowerUp;
    [SerializeField] private AudioClip collectStar;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip levelComplete;
    [SerializeField] private AudioClip magnet;
    [SerializeField] private AudioClip playerExplosion;
    [SerializeField] private AudioClip snow;
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip uiPlay;
    [SerializeField] private AudioClip uiReturn;
    [SerializeField] private AudioClip uiSaved;
    [SerializeField] private AudioClip win;

    private bool musicInitialized;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    private bool isMuted;

    private const string MusicVolumeKey = "Audio.MusicVolume";
    private const string SfxVolumeKey = "Audio.SfxVolume";
    private const string MutedKey = "Audio.Muted";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureAudioSources();
        LoadSettings();
        ApplyVolumes();
    }

    private void Start()
    {
        InitializeMusic();
    }

    private void InitializeMusic()
    {
        if (musicInitialized)
        {
            return;
        }

        if (musicSource == null || musicLoop == null)
        {
            return;
        }

        musicSource.loop = true;
        if (musicSource.clip != musicLoop)
        {
            musicSource.clip = musicLoop;
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }

        musicInitialized = true;
    }

    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        ApplyVolumes();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        ApplyVolumes();
    }

    public void SetMuted(bool muted)
    {
        isMuted = muted;
        PlayerPrefs.SetInt(MutedKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    private void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        isMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = isMuted ? 0f : musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = isMuted ? 0f : sfxVolume;
        }
    }

    public void PlaySFX(SfxType type)
    {
        if (sfxSource == null)
        {
            return;
        }

        AudioClip clip = GetClip(type);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    private AudioClip GetClip(SfxType type)
    {
        switch (type)
        {
            case SfxType.AlienExplosion:
                return alienExplosion;
            case SfxType.AsteroidExplosion:
                return asteroidExplosion;
            case SfxType.CollectCroissant:
                return collectCroissant;
            case SfxType.CollectPowerUp:
                return collectPowerUp;
            case SfxType.CollectStar:
                return collectStar;
            case SfxType.Death:
                return death;
            case SfxType.LevelComplete:
                return levelComplete;
            case SfxType.Magnet:
                return magnet;
            case SfxType.PlayerExplosion:
                return playerExplosion;
            case SfxType.Snow:
                return snow;
            case SfxType.UiClick:
                return uiClick;
            case SfxType.UiPlay:
                return uiPlay;
            case SfxType.UiReturn:
                return uiReturn;
            case SfxType.UiSaved:
                return uiSaved;
            case SfxType.Win:
                return win;
            default:
                return null;
        }
    }
}
