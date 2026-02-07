using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Audio
{
    /// <summary>
    /// Central audio manager for the game.
    ///
    /// ============================================
    /// SETUP INSTRUCTIONS
    /// ============================================
    ///
    /// 1. Create an empty GameObject named "AudioManager"
    /// 2. Add this AudioManager component
    /// 3. The script will auto-create child AudioSources on Awake:
    ///    - MusicSource (for background music, looping)
    ///    - AmbientSource (for environment loops like lava, wind)
    ///    - SFXSource (for one-shot sound effects)
    ///
    /// 4. Assign your AudioClips in the inspector:
    ///    - Music section: main theme
    ///    - Character section: jump, death, steps, wall scratch
    ///    - Props section: crystal, mask sounds
    ///    - Stage section: breaking sounds
    ///    - Environment section: ambient loops
    ///
    /// 5. Call from other scripts:
    ///    AudioManager.Instance.PlayJump();
    ///    AudioManager.Instance.PlayDeath();
    ///    AudioManager.Instance.PlayMaskPickup();
    ///    etc.
    ///
    /// ============================================
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        // ==========================================
        // AUDIO SOURCES (auto-created on Awake)
        // ==========================================
        private AudioSource musicSource;
        private AudioSource ambientSource;
        private AudioSource sfxSource;

        // ==========================================
        // VOLUME SETTINGS
        // ==========================================
        [Header("Volume Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        [Range(0f, 1f)] public float ambientVolume = 0.7f;

        private float duckMultiplier = 1f;

        // ==========================================
        // MUSIC CLIPS
        // ==========================================
        [Header("Music")]
        [Tooltip("Main game theme - loops continuously")]
        public AudioClip mainTheme;

        // ==========================================
        // CHARACTER SOUND CLIPS
        // ==========================================
        [Header("Character - Movement")]
        [Tooltip("Played when player jumps")]
        public AudioClip jumpClip;

        [Tooltip("Played when player dies")]
        public AudioClip deathClip;

        [Tooltip("Played when player scratches/clings to wall")]
        public AudioClip wallScratchClip;

        [Header("Character - Footsteps")]
        [Tooltip("Footstep sounds on amethyst/crystal surfaces")]
        public AudioClip[] stepsAmethyst;

        [Tooltip("Footstep sounds on burning rock surfaces")]
        public AudioClip[] stepsBurningRock;

        // ==========================================
        // PROPS SOUND CLIPS
        // ==========================================
        [Header("Props")]
        [Tooltip("Crystal pickup/interaction sound")]
        public AudioClip crystalClip;

        [Tooltip("Mask pickup/equip sounds (randomly selected)")]
        public AudioClip[] maskClips;

        // ==========================================
        // STAGE SOUND CLIPS
        // ==========================================
        [Header("Stage")]
        [Tooltip("Rock breaking sound")]
        public AudioClip breakingRockClip;

        [Tooltip("Crystal breaking sound")]
        public AudioClip crystalBreakingClip;

        [Tooltip("Levitating mask ambient sound")]
        public AudioClip levitatingMaskClip;

        // ==========================================
        // ENVIRONMENT / AMBIENT CLIPS
        // ==========================================
        [Header("Environment - Ambient Loops")]
        [Tooltip("Wind ambient loop")]
        public AudioClip windClip;

        [Tooltip("Wind + fire ambient loop")]
        public AudioClip windFireClip;

        [Tooltip("Lava + wind + fire ambient loop (recommended)")]
        public AudioClip lavaWindFireClip;

        [Tooltip("Lava only ambient loop")]
        public AudioClip lavaAloneClip;

        [Tooltip("Glass/crystal environment ambient")]
        public AudioClip glassEnvironmentClip;

        // ==========================================
        // INITIALIZATION
        // ==========================================
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio sources
            CreateAudioSources();
        }

        private void CreateAudioSources()
        {
            // Music source - loops, quieter
            musicSource = CreateChildAudioSource("MusicSource");
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            // Ambient source - loops
            ambientSource = CreateChildAudioSource("AmbientSource");
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;

            // SFX source - one-shots
            sfxSource = CreateChildAudioSource("SFXSource");
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        private AudioSource CreateChildAudioSource(string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform);
            return child.AddComponent<AudioSource>();
        }

        private void Start()
        {
            // Auto-play music on start (optional - remove if you want manual control)
            // PlayMusic();
        }

        // ==========================================
        // MUSIC CONTROLS
        // ==========================================

        /// <summary>
        /// Play the main theme music
        /// </summary>
        public void PlayMusic()
        {
            if (mainTheme == null) return;
            musicSource.clip = mainTheme;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void PauseMusic()
        {
            musicSource.Pause();
        }

        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        // ==========================================
        // AMBIENT CONTROLS
        // ==========================================

        /// <summary>
        /// Play an ambient loop (stops previous ambient)
        /// </summary>
        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null) return;
            ambientSource.clip = clip;
            ambientSource.volume = ambientVolume * masterVolume;
            ambientSource.Play();
        }

        public void PlayLavaAmbient() => PlayAmbient(lavaWindFireClip ?? lavaAloneClip);
        public void PlayWindAmbient() => PlayAmbient(windClip);
        public void PlayGlassAmbient() => PlayAmbient(glassEnvironmentClip);

        public void StopAmbient()
        {
            ambientSource.Stop();
        }

        // ==========================================
        // SFX - CHARACTER
        // ==========================================

        public void PlayJump()
        {
            PlaySFX(jumpClip);
        }

        public void PlayDeath()
        {
            PlaySFX(deathClip);
        }

        public void PlayWallScratch()
        {
            PlaySFX(wallScratchClip);
        }

        public void PlayFootstepAmethyst()
        {
            PlayRandomSFX(stepsAmethyst);
        }

        public void PlayFootstepBurningRock()
        {
            PlayRandomSFX(stepsBurningRock);
        }

        // ==========================================
        // SFX - PROPS
        // ==========================================

        public void PlayCrystal()
        {
            PlaySFX(crystalClip);
        }

        public void PlayMaskPickup()
        {
            PlayRandomSFX(maskClips);
        }

        // ==========================================
        // SFX - STAGE
        // ==========================================

        public void PlayBreakingRock()
        {
            PlaySFX(breakingRockClip);
        }

        public void PlayCrystalBreaking()
        {
            PlaySFX(crystalBreakingClip);
        }

        public void PlayLevitatingMask()
        {
            PlaySFX(levitatingMaskClip);
        }

        // ==========================================
        // CORE SFX METHODS
        // ==========================================

        private void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        private void PlayRandomSFX(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            PlaySFX(clip);
        }

        // ==========================================
        // VOLUME UPDATES
        // ==========================================

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume * masterVolume * duckMultiplier;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            ambientSource.volume = ambientVolume * masterVolume;
        }

        private void UpdateAllVolumes()
        {
            musicSource.volume = musicVolume * masterVolume * duckMultiplier;
            ambientSource.volume = ambientVolume * masterVolume;
        }

        // ==========================================
        // MUSIC DUCKING (lower volume temporarily)
        // ==========================================

        /// <summary>
        /// Lower music volume to a percentage (0.2 = 20%)
        /// </summary>
        public void DuckMusic(float multiplier = 0.2f)
        {
            duckMultiplier = Mathf.Clamp01(multiplier);
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume * duckMultiplier;
        }

        /// <summary>
        /// Restore music to normal volume
        /// </summary>
        public void RestoreMusicVolume()
        {
            duckMultiplier = 1f;
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
        }
    }
}
