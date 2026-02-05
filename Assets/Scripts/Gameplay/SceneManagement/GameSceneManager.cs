using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages scene transitions with loading screen support.
/// Singleton that persists across scenes.
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreenPrefab;
    [SerializeField] private float minimumLoadingTime = 1.5f;

    [Header("Scene Configuration")]
    [SerializeField] private string[] levelSceneNames;

    public event Action OnLoadingStarted;
    public event Action<float> OnLoadingProgress;
    public event Action OnLoadingComplete;

    private LoadingScreenUI currentLoadingScreen;
    private int currentLevelIndex = 0;
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Load the next level in sequence.
    /// </summary>
    public void LoadNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex < levelSceneNames.Length)
        {
            LoadScene(levelSceneNames[currentLevelIndex]);
        }
        else
        {
            Debug.Log("All levels completed!");
            OnAllLevelsComplete();
        }
    }

    /// <summary>
    /// Load a specific scene by name.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    /// <summary>
    /// Load a specific scene by build index.
    /// </summary>
    public void LoadScene(int buildIndex)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneAsyncByIndex(buildIndex));
    }

    /// <summary>
    /// Reload the current scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Load a level by its index in the levelSceneNames array.
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelSceneNames.Length)
        {
            currentLevelIndex = levelIndex;
            LoadScene(levelSceneNames[levelIndex]);
        }
        else
        {
            Debug.LogWarning($"Level index {levelIndex} is out of range.");
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;
        OnLoadingStarted?.Invoke();

        ShowLoadingScreen();

        float startTime = Time.unscaledTime;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            OnLoadingProgress?.Invoke(progress);

            if (currentLoadingScreen != null)
            {
                currentLoadingScreen.SetProgress(progress);
            }

            if (asyncLoad.progress >= 0.9f)
            {
                float elapsedTime = Time.unscaledTime - startTime;
                if (elapsedTime >= minimumLoadingTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        OnLoadingComplete?.Invoke();
        HideLoadingScreen();
        isLoading = false;
    }

    private IEnumerator LoadSceneAsyncByIndex(int buildIndex)
    {
        isLoading = true;
        OnLoadingStarted?.Invoke();

        ShowLoadingScreen();

        float startTime = Time.unscaledTime;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            OnLoadingProgress?.Invoke(progress);

            if (currentLoadingScreen != null)
            {
                currentLoadingScreen.SetProgress(progress);
            }

            if (asyncLoad.progress >= 0.9f)
            {
                float elapsedTime = Time.unscaledTime - startTime;
                if (elapsedTime >= minimumLoadingTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        OnLoadingComplete?.Invoke();
        HideLoadingScreen();
        isLoading = false;
    }

    private void ShowLoadingScreen()
    {
        if (loadingScreenPrefab != null)
        {
            GameObject loadingScreenObj = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(loadingScreenObj);
            currentLoadingScreen = loadingScreenObj.GetComponent<LoadingScreenUI>();

            if (currentLoadingScreen != null)
            {
                currentLoadingScreen.Show();
            }
        }
    }

    private void HideLoadingScreen()
    {
        if (currentLoadingScreen != null)
        {
            currentLoadingScreen.Hide(() =>
            {
                Destroy(currentLoadingScreen.gameObject);
                currentLoadingScreen = null;
            });
        }
    }

    private void OnAllLevelsComplete()
    {
        // Override this or subscribe to an event for game completion logic
        Debug.Log("Game Complete! Return to main menu or show credits.");
    }

    public int CurrentLevelIndex => currentLevelIndex;
    public bool IsLoading => isLoading;
}
