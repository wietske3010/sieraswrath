using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Sources (for crossfading)")]
    [SerializeField] private AudioSource musicSource1;
    [SerializeField] private AudioSource musicSource2;
    [SerializeField] private float crossfadeDuration = 1.5f;

    [Header("SFX Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip catacombMusic;
    [SerializeField] private AudioClip gardenMusic;
    [SerializeField] private AudioClip passageMusic;
    [SerializeField] private AudioClip pauseMenuMusic;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField] private AudioClip conclusionMusic;
    [SerializeField] private AudioClip prologueMusic;
    [SerializeField] private AudioClip cutScene1;
    [SerializeField] private AudioClip cutScene2;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip cluePickupSFX;
    [SerializeField] private AudioClip clueOpenSFX;
    [SerializeField] private AudioClip lifeLostSFX;
    [SerializeField] private AudioClip buttonClickSFX;

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private AudioSource activeMusicSource;
    private Coroutine crossfadeCoroutine;

    void Awake()
    {
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

        activeMusicSource = musicSource1;
        musicSource1.loop = true;
        musicSource2.loop = true;
        musicSource2.volume = 0f;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnLivesChanged += OnLivesChanged;
            GameStateManager.Instance.OnClueCollected += OnClueCollected;
        }
    }

    // ─── Scene → Music Mapping ────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu": PlayMusic(mainMenuMusic); break;
            case "Prologue": PlayMusic(prologueMusic); break;
            case "Level1_Catacombs": PlayMusic(catacombMusic); break;
            case "Cutscene1_AfterCatacombs": PlayMusic(cutScene1); break;
            case "Level2_Gardens": PlayMusic(gardenMusic); break;
            case "Cutscene2_AfterGardens": PlayMusic(cutScene2); break;
            case "Level3_Corridors": PlayMusic(passageMusic); break;
            case "GameOver": PlayMusic(gameOverMusic); break;
            case "Conclusion": PlayMusic(conclusionMusic); break;
            case "ConclusionCutscene": PlayMusic(conclusionMusic); break;
        }
    }

    // ─── Music ────────────────────────────────────────────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (activeMusicSource.clip == clip && activeMusicSource.isPlaying) return;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(CrossfadeTo(clip));
    }

    private IEnumerator CrossfadeTo(AudioClip clip)
    {
        AudioSource incoming = (activeMusicSource == musicSource1) ? musicSource2 : musicSource1;

        incoming.clip = clip;
        incoming.volume = 0f;
        incoming.Play();

        float elapsed = 0f;
        float startVolume = activeMusicSource.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / crossfadeDuration;
            activeMusicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            incoming.volume = Mathf.Lerp(0f, musicVolume, t);
            yield return null;
        }

        activeMusicSource.Stop();
        activeMusicSource.clip = null;
        activeMusicSource = incoming;
    }

    public void PauseMusic() => activeMusicSource.Pause();
    public void ResumeMusic() => activeMusicSource.UnPause();

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        activeMusicSource.volume = musicVolume;
    }

    // ─── SFX ─────────────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayUIClick() => PlaySFX(buttonClickSFX);

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void ClueOpened() => PlaySFX(clueOpenSFX);


    // ─── Game Event Hooks ─────────────────────────────────────────────────────

    private void OnLivesChanged(int newLives)
    {
        // Only play when lives are lost, not when reset to full
        if (newLives < 5)
            PlaySFX(lifeLostSFX);
    }

    private void OnClueCollected(string comboID)
    {
        PlaySFX(cluePickupSFX);
    }


}