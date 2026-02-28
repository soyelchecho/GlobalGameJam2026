using System.Collections;
using Gameplay.Hazards;
using UnityEngine;

namespace Gameplay.Audio
{
    /// <summary>
    /// Plays a one-shot intro clip then seamlessly loops the core clip.
    /// Hooks into VolumeManager so the music volume slider applies.
    /// On Start, stops the global AudioManager to take over music.
    /// </summary>
    public class FinalLevelMusicManager : MonoBehaviour
    {
        [Header("Clips")]
        [SerializeField] private AudioClip introClip;
        [SerializeField] private AudioClip coreClip;

        private AudioSource introSource;
        private AudioSource coreSource;
        private bool introStarted = false;
        private bool coreStarted = false;

        private void Awake()
        {
            introSource = gameObject.AddComponent<AudioSource>();
            introSource.loop = false;
            introSource.playOnAwake = false;
            introSource.spatialBlend = 0f;

            coreSource = gameObject.AddComponent<AudioSource>();
            coreSource.loop = true;
            coreSource.playOnAwake = false;
            coreSource.spatialBlend = 0f;

            // Suppress AudioManager immediately so it can't play anything in this level,
            // regardless of what other scripts try to call on it.
            if (AudioManager.Instance != null)
                AudioManager.Instance.Suppress();

            VolumeManager.OnVolumeChanged += UpdateVolume;
        }

        private IEnumerator Start()
        {
            // Wait one frame so all other Start() calls (e.g. AudioManager.Start playing
            // calm theme, EnvironmentAudioManager.Start playing ambient) have finished.
            yield return null;

            // Suppress again in case AudioManager was freshly created in this scene
            // and started playing its calm theme inside its own Start().
            if (AudioManager.Instance != null)
                AudioManager.Instance.Suppress();

            var risingLava = FindObjectOfType<RisingLava>();
            if (risingLava != null)
                risingLava.PlayIntenseThemeOnRise = false;

            if (EnvironmentAudioManager.Instance != null)
            {
                EnvironmentAudioManager.Instance.StopAmbient();
                EnvironmentAudioManager.Instance.StopPeriodicCrystal();
            }

            float volume = VolumeManager.GetMusicVolume();
            introSource.volume = volume;
            coreSource.volume = volume;

            if (introClip != null)
            {
                introSource.clip = introClip;
                introSource.Play();
                introStarted = true;
            }
            else
            {
                introStarted = true;
                StartCore();
            }
        }

        private void Update()
        {
            if (!introStarted || coreStarted) return;
            if (!introSource.isPlaying)
                StartCore();
        }

        private void StartCore()
        {
            coreStarted = true;
            if (coreClip != null)
            {
                coreSource.clip = coreClip;
                coreSource.Play();
            }
        }

        private void UpdateVolume()
        {
            float volume = VolumeManager.GetMusicVolume();
            if (introSource != null) introSource.volume = volume;
            if (coreSource != null) coreSource.volume = volume;
        }

        private void OnDestroy()
        {
            VolumeManager.OnVolumeChanged -= UpdateVolume;
            if (AudioManager.Instance != null)
                AudioManager.Instance.Unsuppress();
        }
    }
}
