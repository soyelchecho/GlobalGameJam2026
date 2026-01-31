using UnityEngine;

namespace Gameplay.CameraSystem
{
    /// <summary>
    /// Adjusts camera orthographic size to maintain a fixed world width
    /// across different screen aspect ratios.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraAspectAdjuster : MonoBehaviour
    {
        [Header("Target Dimensions")]
        [Tooltip("Fixed world width in units (e.g., 20 for 20 units wide)")]
        [SerializeField] private float targetWidth = 20f;

        [Tooltip("Reference aspect ratio (width/height). 9:16 portrait = 0.5625")]
        [SerializeField] private float referenceAspect = 0.5625f; // 9:16 portrait

        [Header("Bounds (Optional)")]
        [Tooltip("Minimum orthographic size (limits how zoomed in we can get)")]
        [SerializeField] private float minOrthoSize = 10f;

        [Tooltip("Maximum orthographic size (limits how zoomed out we can get)")]
        [SerializeField] private float maxOrthoSize = 25f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private Camera cam;
        private float lastAspect;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            AdjustCamera();
        }

        private void Update()
        {
            // Only recalculate if aspect ratio changed (window resize, orientation change)
            if (Mathf.Abs(cam.aspect - lastAspect) > 0.001f)
            {
                AdjustCamera();
            }
        }

        private void AdjustCamera()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (!cam.orthographic) return;

            lastAspect = cam.aspect;

            // Calculate required orthographic size to maintain target width
            // orthoSize = height/2, and width = height * aspect
            // So: targetWidth = orthoSize * 2 * aspect
            // Therefore: orthoSize = targetWidth / (2 * aspect)
            float requiredOrthoSize = targetWidth / (2f * cam.aspect);

            // Clamp to bounds
            requiredOrthoSize = Mathf.Clamp(requiredOrthoSize, minOrthoSize, maxOrthoSize);

            cam.orthographicSize = requiredOrthoSize;
        }

        /// <summary>
        /// Get the actual visible width in world units
        /// </summary>
        public float GetVisibleWidth()
        {
            if (cam == null) return targetWidth;
            return cam.orthographicSize * 2f * cam.aspect;
        }

        /// <summary>
        /// Get the actual visible height in world units
        /// </summary>
        public float GetVisibleHeight()
        {
            if (cam == null) return targetWidth / referenceAspect;
            return cam.orthographicSize * 2f;
        }

        private void OnValidate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            AdjustCamera();
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            float width = GetVisibleWidth();
            float height = GetVisibleHeight();

            GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 210, 120));
            GUILayout.Label($"<b>Camera Info</b>", new GUIStyle { richText = true, normal = { textColor = Color.white } });
            GUILayout.Label($"Screen: {Screen.width}x{Screen.height}");
            GUILayout.Label($"Aspect: {cam.aspect:F3}");
            GUILayout.Label($"Ortho Size: {cam.orthographicSize:F2}");
            GUILayout.Label($"View: {width:F1}x{height:F1} units");
            GUILayout.EndArea();
        }
#endif
    }
}
