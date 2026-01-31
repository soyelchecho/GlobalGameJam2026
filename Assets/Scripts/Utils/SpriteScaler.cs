using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Automatically scales a sprite to fit desired world dimensions.
    /// Attach to a GameObject with a SpriteRenderer.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteScaler : MonoBehaviour
    {
        [Header("Target Size (World Units)")]
        [SerializeField] private float targetWidth = 1f;
        [SerializeField] private float targetHeight = 2f;

        [Header("Options")]
        [Tooltip("Keep aspect ratio (uses the smaller scale)")]
        [SerializeField] private bool keepAspectRatio = false;

        [Tooltip("Auto-update when values change in editor")]
        [SerializeField] private bool autoUpdate = true;

        private SpriteRenderer spriteRenderer;
        private bool hasApplied = false;

        private void Awake()
        {
            // Apply once at startup
            ApplyScale();
            hasApplied = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Only in editor when values change
            if (autoUpdate)
            {
                ApplyScale();
            }
        }
#endif

        [ContextMenu("Apply Scale")]
        public void ApplyScale()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null || spriteRenderer.sprite == null)
                return;

            // Get sprite size in world units (at scale 1)
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

            if (spriteSize.x <= 0 || spriteSize.y <= 0)
                return;

            // Calculate required scale
            float scaleX = targetWidth / spriteSize.x;
            float scaleY = targetHeight / spriteSize.y;

            if (keepAspectRatio)
            {
                // Use uniform scale (smaller of the two to fit within bounds)
                float uniformScale = Mathf.Min(scaleX, scaleY);
                transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
            }
            else
            {
                // Stretch to exact dimensions
                transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }

        /// <summary>
        /// Set target size and apply scale
        /// </summary>
        public void SetTargetSize(float width, float height)
        {
            targetWidth = width;
            targetHeight = height;
            ApplyScale();
        }
    }
}
