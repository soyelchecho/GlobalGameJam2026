using UnityEngine;

/// <summary>
/// Ajusta la cámara ortográfica para mantener un ancho o alto fijo en unidades
/// Para juegos 2D mobile portrait - attach al GameObject de la cámara
/// </summary>
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class CameraScaler : MonoBehaviour
{
    public enum ScaleMode
    {
        FixedWidth,   // Mantiene ancho fijo, altura se adapta (RECOMENDADO para mobile portrait)
        FixedHeight,  // Mantiene altura fija, ancho se adapta
    }

    [Header("Scale Mode")]
    [SerializeField] private ScaleMode scaleMode = ScaleMode.FixedWidth;

    [Header("Fixed Width Settings")]
    [Tooltip("Cuántas unidades quieres que quepan de lado a lado")]
    [SerializeField] private float targetWorldWidth = 12f;

    [Header("Fixed Height Settings")]
    [Tooltip("Cuántas unidades quieres que quepan de arriba a abajo")]
    [SerializeField] private float targetWorldHeight = 21.33f;

    [Header("Debug Info (Read Only)")]
    [SerializeField] private float currentWidth;
    [SerializeField] private float currentHeight;
    [SerializeField] private float currentSize;

    private Camera cam;
    private float lastAspect;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCamera();
    }

    private void Update()
    {
        if (!Mathf.Approximately(lastAspect, cam.aspect))
        {
            UpdateCamera();
        }
    }

    private void UpdateCamera()
    {
        if (cam == null) cam = GetComponent<Camera>();

        lastAspect = cam.aspect;

        switch (scaleMode)
        {
            case ScaleMode.FixedWidth:
                // Size = Ancho / (2 × Aspect)
                cam.orthographicSize = targetWorldWidth / (2f * cam.aspect);
                break;

            case ScaleMode.FixedHeight:
                // Size = Altura / 2
                cam.orthographicSize = targetWorldHeight / 2f;
                break;
        }

        // Actualizar debug info
        currentSize = cam.orthographicSize;
        currentHeight = cam.orthographicSize * 2f;
        currentWidth = currentHeight * cam.aspect;
    }

    private void OnValidate()
    {
        UpdateCamera();
    }

    /// <summary>
    /// Obtiene el ancho visible actual en unidades
    /// </summary>
    public float GetWorldWidth() => cam.orthographicSize * 2f * cam.aspect;

    /// <summary>
    /// Obtiene la altura visible actual en unidades
    /// </summary>
    public float GetWorldHeight() => cam.orthographicSize * 2f;
}
