using System.Collections;
using UnityEngine;

namespace Gameplay.Audio
{
    /// <summary>
    /// Main audio manager - handles music with crossfade transitions.
    /// Volume is controlled via VolumeManager (static, PlayerPrefs-backed).
    ///
    /// SETUP:
    /// 1. Create empty GameObject "AudioManager"
    /// 2. Add this component
    /// 3. Assign calm and intense theme clips
    /// 4. DontDestroyOnLoad is automatic
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [Tooltip("Calm theme - plays at start")]
        public AudioClip calmTheme;

        [Tooltip("Intense theme - plays when lava starts rising")]
        public AudioClip intenseTheme;

        [Header("Crossfade")]
        [Tooltip("Duration of crossfade transition in seconds")]
        [SerializeField] private float crossfadeDuration = 2f;

        private AudioSource musicSourceA;
        private AudioSource musicSourceB;
        private AudioSource activeSource;
        private Coroutine crossfadeCoroutine;
        private float duckMultiplier = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateMusicSources();

            VolumeManager.OnVolumeChanged += UpdateMusicVolume;
        }

        private void OnDestroy()
        {
            VolumeManager.OnVolumeChanged -= UpdateMusicVolume;
        }

        private void CreateMusicSources()
        {
            // Two sources for crossfading
            GameObject musicObjA = new GameObject("MusicSourceA");
            musicObjA.transform.SetParent(transform);
            musicSourceA = musicObjA.AddComponent<AudioSource>();
            musicSourceA.loop = true;
            musicSourceA.playOnAwake = false;

            GameObject musicObjB = new GameObject("MusicSourceB");
            musicObjB.transform.SetParent(transform);
            musicSourceB = musicObjB.AddComponent<AudioSource>();
            musicSourceB.loop = true;
            musicSourceB.playOnAwake = false;

            activeSource = musicSourceA;
        }

        private void Start()
        {
            PlayCalmTheme();
        }

        // ==========================================
        // MUSIC CONTROLS
        // ==========================================

        public void PlayCalmTheme()
        {
            CrossfadeTo(calmTheme);
        }

        public void PlayIntenseTheme()
        {
            CrossfadeTo(intenseTheme);
        }

        private void CrossfadeTo(AudioClip clip)
        {
            if (clip == null) return;

            if (crossfadeCoroutine != null)
                StopCoroutine(crossfadeCoroutine);

            crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(clip));
        }

        private IEnumerator CrossfadeCoroutine(AudioClip newClip)
        {
            AudioSource fadeOutSource = activeSource;
            AudioSource fadeInSource = (activeSource == musicSourceA) ? musicSourceB : musicSourceA;

            // Start new clip
            fadeInSource.clip = newClip;
            fadeInSource.volume = 0f;
            fadeInSource.Play();

            float targetVolume = VolumeManager.GetMusicVolume() * duckMultiplier;
            float elapsed = 0f;

            // Crossfade
            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / crossfadeDuration;

                fadeOutSource.volume = Mathf.Lerp(targetVolume, 0f, t);
                fadeInSource.volume = Mathf.Lerp(0f, targetVolume, t);

                yield return null;
            }

            // Finish
            fadeOutSource.Stop();
            fadeOutSource.volume = 0f;
            fadeInSource.volume = targetVolume;

            activeSource = fadeInSource;
            crossfadeCoroutine = null;
        }

        public void StopMusic()
        {
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
                crossfadeCoroutine = null;
            }
            musicSourceA.Stop();
            musicSourceB.Stop();
        }

        public void PauseMusic()
        {
            musicSourceA.Pause();
            musicSourceB.Pause();
        }

        public void ResumeMusic()
        {
            musicSourceA.UnPause();
            musicSourceB.UnPause();
        }

        // ==========================================
        // VOLUME
        // ==========================================

        private void UpdateMusicVolume()
        {
            float targetVolume = VolumeManager.GetMusicVolume() * duckMultiplier;
            // Update both sources (one may be fading during crossfade)
            if (musicSourceA != null && musicSourceA.isPlaying)
                musicSourceA.volume = targetVolume;
            if (musicSourceB != null && musicSourceB.isPlaying)
                musicSourceB.volume = targetVolume;
        }

        // ==========================================
        // MUSIC DUCKING
        // ==========================================

        public void DuckMusic(float multiplier = 0.2f)
        {
            duckMultiplier = Mathf.Clamp01(multiplier);
            UpdateMusicVolume();
        }

        public void RestoreMusicVolume()
        {
            duckMultiplier = 1f;
            UpdateMusicVolume();
        }
    }
}
