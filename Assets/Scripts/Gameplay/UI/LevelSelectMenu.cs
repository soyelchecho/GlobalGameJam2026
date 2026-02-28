using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Gameplay.Audio;
using Gameplay.SceneManagement;

namespace Gameplay.UI
{
    /// <summary>
    /// Level selection menu. Shows a background image (map) with invisible
    /// hotspot buttons placed over level locations (e.g. volcanoes).
    ///
    /// Setup:
    /// 1. Use Tools > UI > Create Level Select Menu
    /// 2. Assign your map sprite as background
    /// 3. Position the hotspot buttons over each volcano in the Scene view
    /// 4. Configure scene names for each level
    /// </summary>
    public class LevelSelectMenu : MonoBehaviour
    {
        [Header("Background")]
        [Tooltip("Full-screen background image (your volcano map)")]
        [SerializeField] private Image backgroundImage;

        [Header("Level Buttons")]
        [SerializeField] private LevelHotspot[] levels;

        [Header("Navigation")]
        [Tooltip("Back button to return to main menu")]
        [SerializeField] private Button backButton;
        [Tooltip("Scene name for main menu")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [Tooltip("Use loading screen for transitions")]
        [SerializeField] private bool useLoadingScreen = true;

        [Header("Events")]
        public UnityEvent<int> OnLevelSelected;
        public UnityEvent OnBackPressed;

        private void Start()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.DuckMusic(0.6f);

            if (backButton != null)
                backButton.onClick.AddListener(GoToMainMenu);

            for (int i = 0; i < levels.Length; i++)
            {
                int levelIndex = i;
                if (levels[i].button != null)
                    levels[i].button.onClick.AddListener(() => SelectLevel(levelIndex));
            }

            RefreshLockStates();
        }

        private void RefreshLockStates()
        {
            for (int i = 0; i < levels.Length; i++)
                SetLevelLocked(i, !LevelProgressManager.IsLevelUnlocked(i));
        }

        private void SelectLevel(int index)
        {
            if (index < 0 || index >= levels.Length) return;

            var level = levels[index];
            if (level.locked) return;

            OnLevelSelected?.Invoke(index);

            if (AudioManager.Instance != null)
                AudioManager.Instance.RestoreMusicVolume();

            if (useLoadingScreen && LoadingScreen.Instance != null)
            {
                if (level.useSceneIndex)
                    LoadingScreen.Instance.LoadScene(level.sceneIndex);
                else
                    LoadingScreen.Instance.LoadScene(level.sceneName);
            }
            else
            {
                if (level.useSceneIndex)
                    SceneManager.LoadScene(level.sceneIndex);
                else
                    SceneManager.LoadScene(level.sceneName);
            }
        }

        private void GoToMainMenu()
        {
            OnBackPressed?.Invoke();

            if (AudioManager.Instance != null)
                AudioManager.Instance.RestoreMusicVolume();

            if (useLoadingScreen && LoadingScreen.Instance != null)
                LoadingScreen.Instance.LoadScene(mainMenuSceneName);
            else
                SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Lock or unlock a level at runtime.
        /// </summary>
        public void SetLevelLocked(int index, bool locked)
        {
            if (index < 0 || index >= levels.Length) return;
            levels[index].locked = locked;

            if (levels[index].lockIcon != null)
                levels[index].lockIcon.SetActive(locked);

            if (levels[index].button != null)
                levels[index].button.interactable = !locked;
        }
    }

    [System.Serializable]
    public class LevelHotspot
    {
        [Tooltip("Button placed over the level location")]
        public Button button;
        [Tooltip("Scene name to load")]
        public string sceneName;
        [Tooltip("Use scene index instead of name")]
        public bool useSceneIndex;
        [Tooltip("Scene build index")]
        public int sceneIndex;
        [Tooltip("Is this level locked?")]
        public bool locked;
        [Tooltip("Optional lock icon to show/hide")]
        public GameObject lockIcon;
        [Tooltip("Optional label for level name")]
        public Text levelLabel;
    }
}
