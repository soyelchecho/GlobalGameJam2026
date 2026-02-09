using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// Animated loading screen using sprite sequence.
    /// Persists across scenes (DontDestroyOnLoad).
    ///
    /// Setup:
    /// 1. Create a Canvas (sort order high, e.g. 100)
    /// 2. Add a full-screen Image child for the animation
    /// 3. Add this component to the Canvas
    /// 4. Assign the loading sprites array (PANTALLA_CARGA_0001 to 0012)
    /// 5. Call LoadingScreen.Instance.LoadScene("SceneName") from anywhere
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance { get; private set; }

        [Header("Animation")]
        [Tooltip("Sprites for the loading animation (in order)")]
        [SerializeField] private Sprite[] loadingSprites;
        [Tooltip("Image component that displays the animation")]
        [SerializeField] private Image animationImage;
        [Tooltip("Frames per second for sprite animation")]
        [SerializeField] private float framesPerSecond = 12f;
        [Tooltip("Loop the animation")]
        [SerializeField] private bool loopAnimation = true;

        [Header("Canvas")]
        [Tooltip("The canvas group for fading (optional)")]
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Root GameObject of the loading screen visuals")]
        [SerializeField] private GameObject loadingRoot;

        [Header("Timing")]
        [Tooltip("Minimum time to show the loading screen")]
        [SerializeField] private float minimumDisplayTime = 1.5f;
        [Tooltip("Fade in duration")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [Tooltip("Fade out duration")]
        [SerializeField] private float fadeOutDuration = 0.3f;

        [Header("Progress (Optional)")]
        [Tooltip("Optional progress bar")]
        [SerializeField] private Slider progressBar;

        [Header("Events")]
        public UnityEvent OnLoadingStarted;
        public UnityEvent OnLoadingFinished;
        public UnityEvent OnFadeInComplete;
        public UnityEvent OnFadeOutComplete;

        private bool isLoading;
        private int currentFrame;
        private float frameTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Hide();
        }

        private void Update()
        {
            if (!isLoading || loadingSprites == null || loadingSprites.Length == 0) return;

            frameTimer += Time.unscaledDeltaTime;
            float frameDuration = 1f / framesPerSecond;

            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                currentFrame++;

                if (currentFrame >= loadingSprites.Length)
                {
                    currentFrame = loopAnimation ? 0 : loadingSprites.Length - 1;
                }

                if (animationImage != null && loadingSprites[currentFrame] != null)
                {
                    animationImage.sprite = loadingSprites[currentFrame];
                }
            }
        }

        /// <summary>
        /// Load a scene by name with animated loading screen.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneRoutine(sceneName, -1));
        }

        /// <summary>
        /// Load a scene by build index with animated loading screen.
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            if (isLoading) return;
            StartCoroutine(LoadSceneRoutine(null, sceneIndex));
        }

        private IEnumerator LoadSceneRoutine(string sceneName, int sceneIndex)
        {
            isLoading = true;
            currentFrame = 0;
            frameTimer = 0f;
            OnLoadingStarted?.Invoke();

            // Show and fade in
            Show();
            yield return StartCoroutine(FadeIn());
            OnFadeInComplete?.Invoke();

            // Start async loading
            AsyncOperation asyncOp;
            if (sceneName != null)
                asyncOp = SceneManager.LoadSceneAsync(sceneName);
            else
                asyncOp = SceneManager.LoadSceneAsync(sceneIndex);

            asyncOp.allowSceneActivation = false;

            float elapsed = 0f;

            // Wait for loading + minimum display time
            while (asyncOp.progress < 0.9f || elapsed < minimumDisplayTime)
            {
                elapsed += Time.unscaledDeltaTime;

                // Update progress bar
                if (progressBar != null)
                {
                    float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                    progressBar.value = progress;
                }

                yield return null;
            }

            if (progressBar != null)
                progressBar.value = 1f;

            // Activate the scene
            asyncOp.allowSceneActivation = true;

            // Wait for scene to fully load
            while (!asyncOp.isDone)
                yield return null;

            // Fade out
            yield return StartCoroutine(FadeOut());
            OnFadeOutComplete?.Invoke();

            Hide();
            isLoading = false;
            OnLoadingFinished?.Invoke();
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            canvasGroup.alpha = 1f;
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        private void Show()
        {
            if (loadingRoot != null)
                loadingRoot.SetActive(true);

            if (animationImage != null && loadingSprites != null && loadingSprites.Length > 0)
                animationImage.sprite = loadingSprites[0];
        }

        private void Hide()
        {
            if (loadingRoot != null)
                loadingRoot.SetActive(false);
        }
    }
}
