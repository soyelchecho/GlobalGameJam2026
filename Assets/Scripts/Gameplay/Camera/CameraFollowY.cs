using UnityEngine;

namespace Gameplay.CameraSystem
{
    /// <summary>
    /// Camera follow with vertical dead zone.
    /// Camera only moves when player exits the dead zone area.
    /// </summary>
    public class CameraFollowY : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Dead Zone")]
        [Tooltip("Distance above camera center where player can move freely")]
        [SerializeField] private float deadZoneTop = 5f;

        [Tooltip("Distance below camera center where player can move freely")]
        [SerializeField] private float deadZoneBottom = 8f;

        [Header("Smoothing")]
        [Tooltip("How fast the camera catches up (higher = faster)")]
        [SerializeField] private float smoothSpeed = 5f;

        [Tooltip("Use SmoothDamp instead of Lerp for more natural movement")]
        [SerializeField] private bool useSmoothDamp = true;

        [Header("Bounds")]
        [Tooltip("Use starting Y position as minimum (camera can't go lower than where it started)")]
        [SerializeField] private bool useStartingYAsMin = true;

        [Tooltip("Use custom bounds instead of/in addition to starting position")]
        [SerializeField] private bool useCustomBounds = false;
        [SerializeField] private float minY = -10f;
        [SerializeField] private float maxY = 100f;

        private float startingY;

        [Header("Debug")]
        [SerializeField] private bool showDeadZone = true;
        [SerializeField] private Color deadZoneColor = new Color(0f, 1f, 0f, 0.3f);

        private float currentVelocity; // For SmoothDamp
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            startingY = transform.position.y;
        }

        private void Start()
        {
            // Try to find player if not assigned
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float targetY = target.position.y;
            float cameraY = transform.position.y;

            // Calculate dead zone bounds in world space
            float deadZoneTopWorld = cameraY + deadZoneTop;
            float deadZoneBottomWorld = cameraY - deadZoneBottom;

            float desiredY = cameraY;

            // Check if player is outside dead zone
            if (targetY > deadZoneTopWorld)
            {
                // Player is above dead zone - move camera up
                desiredY = targetY - deadZoneTop;
            }
            else if (targetY < deadZoneBottomWorld)
            {
                // Player is below dead zone - move camera down
                desiredY = targetY + deadZoneBottom;
            }

            // Apply bounds
            float effectiveMinY = useStartingYAsMin ? startingY : float.MinValue;
            float effectiveMaxY = float.MaxValue;

            if (useCustomBounds)
            {
                effectiveMinY = Mathf.Max(effectiveMinY, minY);
                effectiveMaxY = maxY;
            }

            desiredY = Mathf.Clamp(desiredY, effectiveMinY, effectiveMaxY);

            // Smooth movement
            float newY;
            if (useSmoothDamp)
            {
                newY = Mathf.SmoothDamp(cameraY, desiredY, ref currentVelocity, 1f / smoothSpeed);
            }
            else
            {
                newY = Mathf.Lerp(cameraY, desiredY, smoothSpeed * Time.deltaTime);
            }

            // Apply new position (only Y changes)
            Vector3 newPos = transform.position;
            newPos.y = newY;
            transform.position = newPos;
        }

        /// <summary>
        /// Instantly snap camera to target (useful for respawns, teleports)
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;

            Vector3 newPos = transform.position;
            newPos.y = target.position.y;
            transform.position = newPos;
            currentVelocity = 0f;
        }

        /// <summary>
        /// Set a new target to follow
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void OnDrawGizmos()
        {
            if (!showDeadZone) return;

            Camera camera = cam != null ? cam : GetComponent<Camera>();
            if (camera == null) return;

            float height = camera.orthographicSize * 2f;
            float width = height * camera.aspect;

            Vector3 center = transform.position;

            // Draw dead zone
            Gizmos.color = deadZoneColor;

            // Top of dead zone
            Vector3 topCenter = center + Vector3.up * deadZoneTop;
            Gizmos.DrawLine(topCenter + Vector3.left * width / 2f, topCenter + Vector3.right * width / 2f);

            // Bottom of dead zone
            Vector3 bottomCenter = center - Vector3.up * deadZoneBottom;
            Gizmos.DrawLine(bottomCenter + Vector3.left * width / 2f, bottomCenter + Vector3.right * width / 2f);

            // Vertical lines
            Gizmos.DrawLine(
                new Vector3(center.x - width / 4f, center.y - deadZoneBottom, center.z),
                new Vector3(center.x - width / 4f, center.y + deadZoneTop, center.z)
            );
            Gizmos.DrawLine(
                new Vector3(center.x + width / 4f, center.y - deadZoneBottom, center.z),
                new Vector3(center.x + width / 4f, center.y + deadZoneTop, center.z)
            );

            // Draw camera bounds
            if (useStartingYAsMin || useCustomBounds)
            {
                Gizmos.color = Color.red;

                // Min Y line
                float drawMinY = useStartingYAsMin ? startingY : minY;
                if (useCustomBounds && !useStartingYAsMin)
                {
                    drawMinY = minY;
                }

                Gizmos.DrawLine(
                    new Vector3(center.x - width / 2f, drawMinY, center.z),
                    new Vector3(center.x + width / 2f, drawMinY, center.z)
                );

                // Max Y line (only if custom bounds)
                if (useCustomBounds)
                {
                    Gizmos.DrawLine(
                        new Vector3(center.x - width / 2f, maxY, center.z),
                        new Vector3(center.x + width / 2f, maxY, center.z)
                    );
                }
            }
        }
    }
}
