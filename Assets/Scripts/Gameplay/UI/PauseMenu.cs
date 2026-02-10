using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// In-game pause menu. Pauses the game (TimeScale=0) and shows
    /// volume sliders + navigation buttons.
    ///
    /// Setup:
    /// 1. Use Tools > UI > Create Pause Menu
    /// 2. Assign gear button sprite and panel sprites in inspector
    /// 3. Configure scene names for level select / main menu
    /// 4. Add to each level scene (or make it a prefab)
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("The gear/settings button (always visible)")]
        [SerializeField] private GameObject gearButton;
        [Tooltip("The pause menu panel (hidden by default)")]
        [SerializeField] private GameObject pausePanel;
        [Tooltip("Dark overlay behind the panel")]
        [SerializeField] private GameObject overlay;

        [Header("Buttons")]
        [SerializeField] private Button gearBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button levelSelectBtn;
        [SerializeField] private Button mainMenuBtn;
        [SerializeField] private Button resumeBtn;

        [Header("Volume Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Scene Navigation")]
        [Tooltip("Scene name for level selection")]
        [SerializeField] private string levelSelectSceneName = "LevelSelect";
        [Tooltip("Scene name for main menu")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [Tooltip("Use loading screen for transitions")]
        [SerializeField] private bool useLoadingScreen = true;

        [Header("Events")]
        public UnityEvent OnPaused;
        public UnityEvent OnResumed;

        private bool isPaused;

        private void Start()
        {
            // Hide panel at start
            if (pausePanel != null) pausePanel.SetActive(false);
            if (overlay != null) overlay.SetActive(false);

            // Wire buttons
            if (gearBtn != null) gearBtn.onClick.AddListener(Pause);
            if (closeBtn != null) closeBtn.onClick.AddListener(Resume);
            if (resumeBtn != null) resumeBtn.onClick.AddListener(Resume);
            if (levelSelectBtn != null) levelSelectBtn.onClick.AddListener(GoToLevelSelect);
            if (mainMenuBtn != null) mainMenuBtn.onClick.AddListener(GoToMainMenu);

            // Wire sliders
            SetupSliders();
        }

        private void SetupSliders()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 1f;
                masterVolumeSlider.value = Audio.VolumeManager.MasterVolume;
                masterVolumeSlider.onValueChanged.AddListener(v => Audio.VolumeManager.MasterVolume = v);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.minValue = 0f;
                musicVolumeSlider.maxValue = 1f;
                musicVolumeSlider.value = Audio.VolumeManager.MusicVolume;
                musicVolumeSlider.onValueChanged.AddListener(v => Audio.VolumeManager.MusicVolume = v);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 1f;
                sfxVolumeSlider.value = Audio.VolumeManager.SFXVolume;
                sfxVolumeSlider.onValueChanged.AddListener(v => Audio.VolumeManager.SFXVolume = v);
            }
        }

        public void Pause()
        {
            if (isPaused) return;
            isPaused = true;

            Time.timeScale = 0f;

            if (pausePanel != null) pausePanel.SetActive(true);
            if (overlay != null) overlay.SetActive(true);

            // Refresh slider values
            if (masterVolumeSlider != null) masterVolumeSlider.value = Audio.VolumeManager.MasterVolume;
            if (musicVolumeSlider != null) musicVolumeSlider.value = Audio.VolumeManager.MusicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = Audio.VolumeManager.SFXVolume;

            OnPaused?.Invoke();
        }

        public void Resume()
        {
            if (!isPaused) return;
            isPaused = false;

            Audio.VolumeManager.Save();
            Time.timeScale = 1f;

            if (pausePanel != null) pausePanel.SetActive(false);
            if (overlay != null) overlay.SetActive(false);

            OnResumed?.Invoke();
        }

        private void GoToLevelSelect()
        {
            Time.timeScale = 1f;
            Audio.VolumeManager.Save();

            if (useLoadingScreen && LoadingScreen.Instance != null)
                LoadingScreen.Instance.LoadScene(levelSelectSceneName);
            else
                SceneManager.LoadScene(levelSelectSceneName);
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            Audio.VolumeManager.Save();

            if (useLoadingScreen && LoadingScreen.Instance != null)
                LoadingScreen.Instance.LoadScene(mainMenuSceneName);
            else
                SceneManager.LoadScene(mainMenuSceneName);
        }

        private void OnDestroy()
        {
            // Safety: restore time scale if destroyed while paused
            if (isPaused)
                Time.timeScale = 1f;
        }
    }
}
