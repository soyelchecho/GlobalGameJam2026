using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// Main menu controller. Handles navigation between menu panels
    /// and options (volume sliders for mobile).
    /// Volume is managed through VolumeManager (static, PlayerPrefs-backed).
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("Background")]
        [Tooltip("Background image for the menu")]
        [SerializeField] private Image backgroundImage;
        [Tooltip("Sprite for the background")]
        [SerializeField] private Sprite backgroundSprite;

        [Header("Panels")]
        [Tooltip("Main panel with the 4 buttons")]
        [SerializeField] private GameObject mainPanel;
        [Tooltip("Options panel with sliders")]
        [SerializeField] private GameObject optionsPanel;
        [Tooltip("Credits panel")]
        [SerializeField] private GameObject creditsPanel;
        [Tooltip("Image component inside credits panel (assign your credits sprite here)")]
        [SerializeField] private Image creditsImage;

        [Header("Buttons (SpriteButton)")]
        [SerializeField] private SpriteButton startButton;
        [SerializeField] private SpriteButton optionsButton;
        [SerializeField] private SpriteButton creditsButton;
        [SerializeField] private SpriteButton exitButton;
        [SerializeField] private SpriteButton optionsBackButton;
        [SerializeField] private SpriteButton creditsBackButton;

        [Header("Options - Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Scene Loading")]
        [Tooltip("Scene name or index to load when pressing Start")]
        [SerializeField] private string startSceneName;
        [Tooltip("Use scene index instead of name")]
        [SerializeField] private bool useSceneIndex;
        [Tooltip("Scene index to load (if useSceneIndex is true)")]
        [SerializeField] private int startSceneIndex = 1;
        [Tooltip("Use loading screen when transitioning")]
        [SerializeField] private bool useLoadingScreen = true;

        [Header("Events")]
        public UnityEvent OnStartPressed;
        public UnityEvent OnOptionsOpened;
        public UnityEvent OnOptionsClosed;
        public UnityEvent OnCreditsOpened;
        public UnityEvent OnCreditsClosed;
        public UnityEvent OnExitPressed;

        private void Start()
        {
            // Setup background
            if (backgroundImage != null && backgroundSprite != null)
                backgroundImage.sprite = backgroundSprite;

            // Show main panel, hide others
            ShowMainPanel();

            // Wire buttons
            if (startButton != null) startButton.OnClick.AddListener(OnStart);
            if (optionsButton != null) optionsButton.OnClick.AddListener(ShowOptions);
            if (creditsButton != null) creditsButton.OnClick.AddListener(ShowCredits);
            if (exitButton != null) exitButton.OnClick.AddListener(OnExit);
            if (optionsBackButton != null) optionsBackButton.OnClick.AddListener(HideOptions);
            if (creditsBackButton != null) creditsBackButton.OnClick.AddListener(HideCredits);

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
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.minValue = 0f;
                musicVolumeSlider.maxValue = 1f;
                musicVolumeSlider.value = Audio.VolumeManager.MusicVolume;
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 1f;
                sfxVolumeSlider.value = Audio.VolumeManager.SFXVolume;
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            }
        }

        #region Panel Navigation

        public void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        public void ShowOptions()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            OnOptionsOpened?.Invoke();
        }

        public void HideOptions()
        {
            Audio.VolumeManager.Save();
            ShowMainPanel();
            OnOptionsClosed?.Invoke();
        }

        public void ShowCredits()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(true);
            OnCreditsOpened?.Invoke();
        }

        public void HideCredits()
        {
            ShowMainPanel();
            OnCreditsClosed?.Invoke();
        }

        #endregion

        #region Actions

        private void OnStart()
        {
            OnStartPressed?.Invoke();

            if (useLoadingScreen && LoadingScreen.Instance != null)
            {
                if (useSceneIndex)
                    LoadingScreen.Instance.LoadScene(startSceneIndex);
                else
                    LoadingScreen.Instance.LoadScene(startSceneName);
            }
            else
            {
                if (useSceneIndex)
                    SceneManager.LoadScene(startSceneIndex);
                else
                    SceneManager.LoadScene(startSceneName);
            }
        }

        private void OnExit()
        {
            OnExitPressed?.Invoke();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Volume

        private void SetMasterVolume(float value)
        {
            Audio.VolumeManager.MasterVolume = value;
        }

        private void SetMusicVolume(float value)
        {
            Audio.VolumeManager.MusicVolume = value;
        }

        private void SetSFXVolume(float value)
        {
            Audio.VolumeManager.SFXVolume = value;
        }

        #endregion
    }
}
