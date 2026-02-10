using System;
using UnityEngine;

namespace Gameplay.Audio
{
    /// <summary>
    /// Centralized volume system. Static class that stores volume settings
    /// in PlayerPrefs and notifies all audio managers when values change.
    ///
    /// Usage from any script:
    ///   VolumeManager.MasterVolume = 0.8f;
    ///   VolumeManager.MusicVolume = 0.5f;
    ///   VolumeManager.SFXVolume = 1f;
    ///   float finalMusic = VolumeManager.GetMusicVolume(); // 0.4f
    ///   float finalSFX = VolumeManager.GetSFXVolume();     // 0.8f
    /// </summary>
    public static class VolumeManager
    {
        private const string MasterKey = "MasterVolume";
        private const string MusicKey = "MusicVolume";
        private const string SFXKey = "SFXVolume";

        private static float masterVolume = -1f;
        private static float musicVolume = -1f;
        private static float sfxVolume = -1f;
        private static bool loaded = false;

        /// <summary>
        /// Fired whenever any volume value changes.
        /// AudioManager, EnvironmentAudioManager etc. subscribe to this.
        /// </summary>
        public static event Action OnVolumeChanged;

        public static float MasterVolume
        {
            get
            {
                EnsureLoaded();
                return masterVolume;
            }
            set
            {
                EnsureLoaded();
                masterVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(MasterKey, masterVolume);
                OnVolumeChanged?.Invoke();
            }
        }

        public static float MusicVolume
        {
            get
            {
                EnsureLoaded();
                return musicVolume;
            }
            set
            {
                EnsureLoaded();
                musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(MusicKey, musicVolume);
                OnVolumeChanged?.Invoke();
            }
        }

        public static float SFXVolume
        {
            get
            {
                EnsureLoaded();
                return sfxVolume;
            }
            set
            {
                EnsureLoaded();
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(SFXKey, sfxVolume);
                OnVolumeChanged?.Invoke();
            }
        }

        /// <summary>
        /// Final music volume (MusicVolume * MasterVolume).
        /// Use this when setting AudioSource volume for music.
        /// </summary>
        public static float GetMusicVolume() => MusicVolume * MasterVolume;

        /// <summary>
        /// Final SFX volume (SFXVolume * MasterVolume).
        /// Use this for PlayOneShot, ambient sources, etc.
        /// </summary>
        public static float GetSFXVolume() => SFXVolume * MasterVolume;

        /// <summary>
        /// Save to disk. Call after finishing slider adjustments.
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        private static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;
            masterVolume = PlayerPrefs.GetFloat(MasterKey, 1f);
            musicVolume = PlayerPrefs.GetFloat(MusicKey, 0.5f);
            sfxVolume = PlayerPrefs.GetFloat(SFXKey, 1f);
        }
    }
}
