using System.Collections;
using UnityEngine;

namespace Gameplay.Audio
{
    /// <summary>
    /// Handles environment audio: ambient loops with intervals, stage sounds.
    /// Volume is controlled via VolumeManager.GetSFXVolume().
    ///
    /// SETUP:
    /// 1. Create empty GameObject "EnvironmentAudioManager"
    /// 2. Add this component
    /// 3. Assign audio clips in inspector
    /// 4. Set auto-play clip if desired
    /// </summary>
    public class EnvironmentAudioManager : MonoBehaviour
    {
        public static EnvironmentAudioManager Instance { get; private set; }

        [Header("Loop Settings")]
        [Tooltip("Start next loop this many seconds before current ends (overlap)")]
        [SerializeField] private float loopOverlap = 1f;

        [Header("Auto-Play on Start")]
        [SerializeField] private bool autoPlay = true;
        [SerializeField] private AmbientType autoPlayType = AmbientType.LavaWindFire;

        [Header("Ambient Clips")]
        [Tooltip("Wind ambient")]
        public AudioClip windClip;

        [Tooltip("Wind + fire ambient")]
        public AudioClip windFireClip;

        [Tooltip("Lava + wind + fire ambient")]
        public AudioClip lavaWindFireClip;

        [Tooltip("Lava only ambient")]
        public AudioClip lavaAloneClip;

        [Tooltip("Glass/crystal environment ambient")]
        public AudioClip glassEnvironmentClip;

        [Header("Stage SFX")]
        [Tooltip("Rock breaking sound")]
        public AudioClip breakingRockClip;

        [Tooltip("Crystal breaking sound")]
        public AudioClip crystalBreakingClip;

        [Tooltip("Levitating mask ambient sound")]
        public AudioClip levitatingMaskClip;

        [Header("Props SFX")]
        [Tooltip("Crystal pickup/interaction sound")]
        public AudioClip crystalClip;

        [Header("Periodic Crystal Sound")]
        [Tooltip("Enable periodic crystal ambient sound")]
        [SerializeField] private bool enablePeriodicCrystal;
        [Tooltip("Clip to play periodically")]
        [SerializeField] private AudioClip periodicCrystalClip;
        [Tooltip("Minimum interval between plays")]
        [SerializeField] private float periodicMinInterval = 2f;
        [Tooltip("Maximum interval between plays")]
        [SerializeField] private float periodicMaxInterval = 3f;

        private AudioSource ambientSource;
        private AudioSource sfxSource;
        private Coroutine loopCoroutine;
        private Coroutine periodicCrystalCoroutine;
        private AudioClip currentAmbientClip;

        public enum AmbientType
        {
            Wind,
            WindFire,
            LavaWindFire,
            LavaAlone,
            Glass
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

            CreateAudioSources();

            VolumeManager.OnVolumeChanged += OnVolumeChanged;
        }

        private void OnDestroy()
        {
            VolumeManager.OnVolumeChanged -= OnVolumeChanged;
        }

        private void Start()
        {
            if (autoPlay)
            {
                PlayAmbientByType(autoPlayType);
            }

            if (enablePeriodicCrystal && periodicCrystalClip != null)
            {
                periodicCrystalCoroutine = StartCoroutine(PeriodicCrystalLoop());
            }
        }

        private void CreateAudioSources()
        {
            // Ambient source - manual looping with intervals
            GameObject ambientObj = new GameObject("AmbientSource");
            ambientObj.transform.SetParent(transform);
            ambientSource = ambientObj.AddComponent<AudioSource>();
            ambientSource.loop = false;
            ambientSource.playOnAwake = false;

            // SFX source - one-shots
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        private void OnVolumeChanged()
        {
            if (ambientSource != null && ambientSource.isPlaying)
            {
                ambientSource.volume = VolumeManager.GetSFXVolume();
            }
        }

        // ==========================================
        // AMBIENT CONTROLS
        // ==========================================

        public void PlayAmbientByType(AmbientType type)
        {
            switch (type)
            {
                case AmbientType.Wind:
                    PlayAmbient(windClip);
                    break;
                case AmbientType.WindFire:
                    PlayAmbient(windFireClip);
                    break;
                case AmbientType.LavaWindFire:
                    PlayAmbient(lavaWindFireClip ?? lavaAloneClip);
                    break;
                case AmbientType.LavaAlone:
                    PlayAmbient(lavaAloneClip);
                    break;
                case AmbientType.Glass:
                    PlayAmbient(glassEnvironmentClip);
                    break;
            }
        }

        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null) return;

            StopAmbient();
            currentAmbientClip = clip;
            loopCoroutine = StartCoroutine(AmbientLoopCoroutine());
        }

        private IEnumerator AmbientLoopCoroutine()
        {
            while (true)
            {
                ambientSource.clip = currentAmbientClip;
                ambientSource.volume = VolumeManager.GetSFXVolume();
                ambientSource.Play();

                // Wait for clip to almost finish, then restart (overlap)
                float waitTime = currentAmbientClip.length - loopOverlap;
                if (waitTime < 0.1f) waitTime = currentAmbientClip.length; // Safety: don't overlap more than clip length
                yield return new WaitForSeconds(waitTime);
            }
        }

        public void PlayLavaAmbient() => PlayAmbient(lavaWindFireClip ?? lavaAloneClip);
        public void PlayWindAmbient() => PlayAmbient(windClip);
        public void PlayWindFireAmbient() => PlayAmbient(windFireClip);
        public void PlayGlassAmbient() => PlayAmbient(glassEnvironmentClip);

        public void StopAmbient()
        {
            if (loopCoroutine != null)
            {
                StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }
            ambientSource.Stop();
            currentAmbientClip = null;
        }

        public void SetLoopOverlap(float overlap)
        {
            loopOverlap = Mathf.Max(0, overlap);
        }

        // ==========================================
        // STAGE SFX
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
        // PROPS SFX
        // ==========================================

        public void PlayCrystal()
        {
            PlaySFX(crystalClip);
        }

        // ==========================================
        // PERIODIC CRYSTAL
        // ==========================================

        public void StopPeriodicCrystal()
        {
            if (periodicCrystalCoroutine != null)
            {
                StopCoroutine(periodicCrystalCoroutine);
                periodicCrystalCoroutine = null;
            }
        }

        private IEnumerator PeriodicCrystalLoop()
        {
            while (true)
            {
                PlaySFX(periodicCrystalClip);
                float clipLength = periodicCrystalClip.length;
                float pause = Random.Range(periodicMinInterval, periodicMaxInterval);
                yield return new WaitForSeconds(clipLength + pause);
            }
        }

        // ==========================================
        // CORE AUDIO METHODS
        // ==========================================

        private void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, VolumeManager.GetSFXVolume());
        }
    }
}
