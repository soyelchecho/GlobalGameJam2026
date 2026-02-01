using UnityEngine;

namespace Gameplay.Parallax
{
    /// <summary>
    /// Keeps an element at a fixed offset from the camera.
    /// Use for ceiling rocks, floor decorations, or any element that should
    /// stay at a fixed position relative to the camera view.
    /// </summary>
    public class CameraAttachedElement : MonoBehaviour
    {
        public enum AttachPosition
        {
            Top,
            Bottom,
            Left,
            Right,
            Custom
        }

        [Header("Attachment")]
        [SerializeField] private AttachPosition attachTo = AttachPosition.Top;

        [Tooltip("Offset from the edge (positive = towards center)")]
        [SerializeField] private float edgeOffset = 0f;

        [Tooltip("Custom offset from camera center (only used if AttachPosition is Custom)")]
        [SerializeField] private Vector2 customOffset = Vector2.zero;

        [Header("Parallax (Optional)")]
        [Tooltip("Add slight parallax movement for depth effect")]
        [SerializeField] private bool useParallax = false;

        [Range(0f, 0.3f)]
        [SerializeField] private float parallaxAmount = 0.1f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        private Camera cam;
        private Vector3 cameraStartPosition;
        private Vector3 startOffset;

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

            if (cameraTransform != null)
            {
                cameraStartPosition = cameraTransform.position;
                startOffset = transform.position - cameraTransform.position;
            }
        }

        private void LateUpdate()
        {
            if (cameraTransform == null || cam == null) return;

            Vector3 targetPosition = CalculateTargetPosition();

            // Apply optional parallax offset
            if (useParallax)
            {
                Vector3 cameraDelta = cameraTransform.position - cameraStartPosition;
                // Subtract small amount to create "lag" effect
                targetPosition.y -= cameraDelta.y * parallaxAmount;
            }

            transform.position = targetPosition;
        }

        private Vector3 CalculateTargetPosition()
        {
            float cameraHalfHeight = cam.orthographicSize;
            float cameraHalfWidth = cameraHalfHeight * cam.aspect;

            Vector3 cameraPos = cameraTransform.position;
            Vector3 targetPos = transform.position;

            switch (attachTo)
            {
                case AttachPosition.Top:
                    targetPos.x = cameraPos.x + startOffset.x;
                    targetPos.y = cameraPos.y + cameraHalfHeight - edgeOffset;
                    break;

                case AttachPosition.Bottom:
                    targetPos.x = cameraPos.x + startOffset.x;
                    targetPos.y = cameraPos.y - cameraHalfHeight + edgeOffset;
                    break;

                case AttachPosition.Left:
                    targetPos.x = cameraPos.x - cameraHalfWidth + edgeOffset;
                    targetPos.y = cameraPos.y + startOffset.y;
                    break;

                case AttachPosition.Right:
                    targetPos.x = cameraPos.x + cameraHalfWidth - edgeOffset;
                    targetPos.y = cameraPos.y + startOffset.y;
                    break;

                case AttachPosition.Custom:
                    targetPos.x = cameraPos.x + customOffset.x;
                    targetPos.y = cameraPos.y + customOffset.y;
                    break;
            }

            return targetPos;
        }

        private void OnDrawGizmosSelected()
        {
            // Show attachment point in editor
            Camera sceneCam = Camera.main;
            if (sceneCam == null) return;

            float halfHeight = sceneCam.orthographicSize;
            float halfWidth = halfHeight * sceneCam.aspect;
            Vector3 camPos = sceneCam.transform.position;

            Gizmos.color = Color.cyan;

            switch (attachTo)
            {
                case AttachPosition.Top:
                    Gizmos.DrawLine(
                        new Vector3(camPos.x - halfWidth, camPos.y + halfHeight - edgeOffset, 0),
                        new Vector3(camPos.x + halfWidth, camPos.y + halfHeight - edgeOffset, 0)
                    );
                    break;
                case AttachPosition.Bottom:
                    Gizmos.DrawLine(
                        new Vector3(camPos.x - halfWidth, camPos.y - halfHeight + edgeOffset, 0),
                        new Vector3(camPos.x + halfWidth, camPos.y - halfHeight + edgeOffset, 0)
                    );
                    break;
            }
        }
    }
}
