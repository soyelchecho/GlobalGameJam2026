using UnityEngine;

namespace Gameplay.Parallax
{
    /// <summary>
    /// Individual parallax layer that moves relative to camera.
    /// Attach to each background layer sprite.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Tooltip("0 = fixed, 0.5 = half speed, 1 = follows camera exactly")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxFactorY = 0.8f;

        [Tooltip("Horizontal parallax (usually 0 for vertical runners)")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxFactorX = 0f;

        [Header("Infinite Scrolling")]
        [SerializeField] private bool infiniteScrollY = false;

        [Tooltip("Height of one tile. Auto-detects if 0.")]
        [SerializeField] private float tileHeight = 0f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private Vector3 startPosition;
        private Vector3 cameraStartPosition;
        private float calculatedHeight;
        private Camera cam;

        private void Awake()
        {
            // Auto-detect tile height
            if (tileHeight <= 0)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    calculatedHeight = sr.bounds.size.y;
                }
                else
                {
                    calculatedHeight = 20f; // fallback
                }
            }
            else
            {
                calculatedHeight = tileHeight;
            }
        }

        private void Start()
        {
            if (cameraTransform == null)
            {
                cam = Camera.main;
                cameraTransform = cam?.transform;
            }
            else
            {
                cam = cameraTransform.GetComponent<Camera>();
            }

            startPosition = transform.position;

            if (cameraTransform != null)
            {
                cameraStartPosition = cameraTransform.position;
            }
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            // Calculate how much the camera has moved from its start
            Vector3 cameraDelta = cameraTransform.position - cameraStartPosition;

            // Apply parallax
            float newX = startPosition.x + (cameraDelta.x * parallaxFactorX);
            float newY = startPosition.y + (cameraDelta.y * parallaxFactorY);

            transform.position = new Vector3(newX, newY, transform.position.z);

            // Infinite scrolling
            if (infiniteScrollY && calculatedHeight > 0)
            {
                HandleInfiniteScroll();
            }
        }

        private void HandleInfiniteScroll()
        {
            float cameraY = cameraTransform.position.y;
            float viewHeight = cam != null ? cam.orthographicSize * 2f : 40f;

            // Position relative to camera
            float relativeY = transform.position.y - cameraY;

            // If this tile is too far below the view, wrap it to above
            if (relativeY < -(viewHeight / 2f + calculatedHeight))
            {
                float wrapDistance = calculatedHeight * Mathf.Ceil(viewHeight / calculatedHeight + 1);
                transform.position += Vector3.up * wrapDistance;
                startPosition += Vector3.up * wrapDistance;
            }
            // If this tile is too far above the view, wrap it to below
            else if (relativeY > (viewHeight / 2f + calculatedHeight))
            {
                float wrapDistance = calculatedHeight * Mathf.Ceil(viewHeight / calculatedHeight + 1);
                transform.position -= Vector3.up * wrapDistance;
                startPosition -= Vector3.up * wrapDistance;
            }
        }

        [ContextMenu("Auto-detect Tile Height")]
        private void AutoDetectHeight()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                tileHeight = sr.bounds.size.y;
                Debug.Log($"[ParallaxLayer] Detected height: {tileHeight}");
            }
        }
    }
}
