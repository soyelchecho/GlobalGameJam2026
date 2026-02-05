using UnityEngine;

/// <summary>
/// Configuración inicial del juego para mobile portrait
/// Attach to a GameObject in your first scene
/// </summary>
public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance { get; private set; }

    [Header("Target Frame Rate")]
    [SerializeField] private int targetFrameRate = 60;

    [Header("Screen Settings")]
    [SerializeField] private bool preventScreenSleep = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Frame rate objetivo
        Application.targetFrameRate = targetFrameRate;

        // Prevenir que la pantalla se apague
        if (preventScreenSleep)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        // Forzar orientación portrait
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
    }
}
