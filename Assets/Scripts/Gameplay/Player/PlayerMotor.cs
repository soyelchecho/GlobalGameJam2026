using System.Collections;
using UnityEngine;

namespace Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform wallCheck;
        [Tooltip("Chest-height check point for detecting frontal obstacles")]
        [SerializeField] private Transform frontCheck;

        [Header("Drop Through")]
        [SerializeField] private float dropThroughDuration = 0.25f;

        private Rigidbody2D rb;
        private Collider2D playerCollider;
        private PlayerData data;
        private float originalGravityScale;
        private Coroutine dropThroughCoroutine;

        // Pre-allocated array to avoid GC allocation
        private readonly ContactPoint2D[] contactsBuffer = new ContactPoint2D[10];

        public Rigidbody2D Rigidbody => rb;
        public Vector2 Velocity => rb.velocity;
        public bool IsDropping { get; private set; }

        public void Initialize(PlayerData playerData)
        {
            rb = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            data = playerData;

            rb.gravityScale = data.gravityScale;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            originalGravityScale = data.gravityScale;
        }

        public void Move(int direction)
        {
            rb.velocity = new Vector2(direction * data.moveSpeed, rb.velocity.y);
        }

        public void Move(int direction, float speed)
        {
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        }

        public void Jump(float force)
        {
            rb.velocity = new Vector2(rb.velocity.x, force);
        }

        public void WallJump(int direction)
        {
            rb.velocity = new Vector2(direction * data.wallJumpForce.x, data.wallJumpForce.y);
        }

        public void ApplyWallSlide()
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -data.wallSlideSpeed);
            }
        }

        public void StartWallCling()
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        public void EndWallCling()
        {
            rb.gravityScale = originalGravityScale;
        }

        public void SetGravityScale(float scale)
        {
            rb.gravityScale = scale;
        }

        public void ResetGravity()
        {
            rb.gravityScale = originalGravityScale;
        }

        public void ApplyBetterJumpPhysics(bool isHoldingJump)
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (data.fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.velocity.y > 0 && !isHoldingJump)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (data.lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        public bool CheckGrounded()
        {
            if (groundCheck == null) return false;

            float checkWidth = data.groundCheckSize * 2f;
            float checkDistance = data.groundCheckSize;
            Vector2 origin = groundCheck.position;

            // Use multiple raycasts to detect diagonal surfaces and edges
            // Center, left, and right raycasts
            RaycastHit2D hitCenter = Physics2D.Raycast(origin, Vector2.down, checkDistance, data.AllGroundLayers);
            RaycastHit2D hitLeft = Physics2D.Raycast(origin + Vector2.left * (checkWidth * 0.5f), Vector2.down, checkDistance, data.AllGroundLayers);
            RaycastHit2D hitRight = Physics2D.Raycast(origin + Vector2.right * (checkWidth * 0.5f), Vector2.down, checkDistance, data.AllGroundLayers);

            // Also cast diagonally to catch slopes
            RaycastHit2D hitDiagLeft = Physics2D.Raycast(origin, new Vector2(-0.3f, -1f).normalized, checkDistance * 1.2f, data.AllGroundLayers);
            RaycastHit2D hitDiagRight = Physics2D.Raycast(origin, new Vector2(0.3f, -1f).normalized, checkDistance * 1.2f, data.AllGroundLayers);

            // Check if any ray hit something
            RaycastHit2D[] hits = { hitCenter, hitLeft, hitRight, hitDiagLeft, hitDiagRight };

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                // For diagonal surfaces, accept normals with y > 0.4 (more permissive)
                // This allows slopes up to ~66 degrees
                if (hit.normal.y <= 0.4f) continue;

                // For one-way platforms: special checks
                if (((1 << hit.collider.gameObject.layer) & data.oneWayPlatformLayer) != 0)
                {
                    // If moving upward fast, ignore one-way platforms (jumping through)
                    if (rb.velocity.y > 0.5f) continue;

                    // Make sure player is FULLY above, not passing through
                    float playerBottom = playerCollider.bounds.min.y;
                    float platformTop = hit.collider.bounds.max.y;

                    float tolerance = Mathf.Min(0.2f, hit.collider.bounds.size.y * 0.5f);
                    if (playerBottom < platformTop - tolerance)
                    {
                        continue; // Still passing through, try next hit
                    }
                }

                return true; // Found valid ground
            }

            return false;
        }

        public bool IsOnOneWayPlatform()
        {
            if (groundCheck == null) return false;

            // Use BoxCast to match CheckGrounded behavior
            float boxWidth = data.groundCheckSize * 2f;
            float boxHeight = 0.05f;
            Vector2 boxSize = new Vector2(boxWidth, boxHeight);

            RaycastHit2D hit = Physics2D.BoxCast(
                groundCheck.position,
                boxSize,
                0f,
                Vector2.down,
                data.groundCheckSize,
                data.oneWayPlatformLayer
            );

            if (hit.collider == null) return false;

            return hit.normal.y > 0.7f;
        }

        public bool CheckWall(int direction)
        {
            if (wallCheck == null) return false;
            Vector2 wallCheckDir = new Vector2(direction, 0);
            RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, wallCheckDir, data.wallCheckDistance, data.AllWallLayers);
            return hit.collider != null;
        }

        public int GetWallDirection()
        {
            if (CheckWall(1)) return 1;
            if (CheckWall(-1)) return -1;
            return 0;
        }

        /// <summary>
        /// Checks if there's an obstacle at chest height in the given direction.
        /// Useful for detecting when player is stuck or needs to react to frontal obstacles.
        /// </summary>
        public bool CheckFront(int direction)
        {
            if (frontCheck == null) return false;

            Vector2 boxSize = new Vector2(0.05f, data.frontCheckHeight);
            Vector2 castDir = new Vector2(direction, 0);

            RaycastHit2D hit = Physics2D.BoxCast(
                frontCheck.position,
                boxSize,
                0f,
                castDir,
                data.frontCheckDistance,
                data.AllWallLayers | data.AllGroundLayers
            );

            return hit.collider != null;
        }

        /// <summary>
        /// Checks front in the current movement direction (based on rigidbody velocity or sprite facing).
        /// Returns the collider hit, or null if nothing.
        /// </summary>
        public Collider2D GetFrontObstacle(int direction)
        {
            if (frontCheck == null) return null;

            Vector2 boxSize = new Vector2(0.05f, data.frontCheckHeight);
            Vector2 castDir = new Vector2(direction, 0);

            RaycastHit2D hit = Physics2D.BoxCast(
                frontCheck.position,
                boxSize,
                0f,
                castDir,
                data.frontCheckDistance,
                data.AllWallLayers | data.AllGroundLayers
            );

            return hit.collider;
        }

        /// <summary>
        /// Checks if player is blocked horizontally by ANY collider (walls, platforms, etc.)
        /// Uses actual collision contacts from the rigidbody.
        /// </summary>
        public bool IsBlockedHorizontally(int direction)
        {
            int contactCount = rb.GetContacts(contactsBuffer);

            for (int i = 0; i < contactCount; i++)
            {
                // Check if contact normal is horizontal (hitting a side)
                if (Mathf.Abs(contactsBuffer[i].normal.x) > 0.5f)
                {
                    // normal.x > 0 means wall is to the LEFT (pushing us right)
                    // normal.x < 0 means wall is to the RIGHT (pushing us left)
                    int blockDirection = contactsBuffer[i].normal.x > 0 ? -1 : 1;
                    if (blockDirection == direction)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetVelocity(Vector2 velocity)
        {
            rb.velocity = velocity;
        }

        public void SetVelocityX(float x)
        {
            rb.velocity = new Vector2(x, rb.velocity.y);
        }

        public void SetVelocityY(float y)
        {
            rb.velocity = new Vector2(rb.velocity.x, y);
        }

        /// <summary>
        /// Drop through one-way platforms. Call this when player wants to drop down.
        /// </summary>
        public void DropThroughPlatform()
        {
            if (!IsOnOneWayPlatform() || IsDropping) return;

            if (dropThroughCoroutine != null)
            {
                StopCoroutine(dropThroughCoroutine);
            }
            dropThroughCoroutine = StartCoroutine(DropThroughCoroutine());
        }

        private IEnumerator DropThroughCoroutine()
        {
            IsDropping = true;

            // Get all one-way platform colliders currently touching
            Collider2D[] platforms = new Collider2D[5];
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(data.oneWayPlatformLayer);
            filter.useLayerMask = true;

            int count = Physics2D.OverlapCollider(playerCollider, filter, platforms);

            // Temporarily ignore collision with these platforms
            for (int i = 0; i < count; i++)
            {
                if (platforms[i] != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, platforms[i], true);
                }
            }

            yield return new WaitForSeconds(dropThroughDuration);

            // Re-enable collision
            for (int i = 0; i < count; i++)
            {
                if (platforms[i] != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, platforms[i], false);
                }
            }

            IsDropping = false;
            dropThroughCoroutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (data == null) return;

            // Ground check (multi-raycast visualization)
            if (groundCheck != null)
            {
                float checkWidth = data.groundCheckSize * 2f;
                float checkDistance = data.groundCheckSize;
                Vector3 origin = groundCheck.position;

                Gizmos.color = Color.green;

                // Center raycast
                Gizmos.DrawLine(origin, origin + Vector3.down * checkDistance);
                Gizmos.DrawWireSphere(origin + Vector3.down * checkDistance, 0.03f);

                // Left raycast
                Vector3 leftOrigin = origin + Vector3.left * (checkWidth * 0.5f);
                Gizmos.DrawLine(leftOrigin, leftOrigin + Vector3.down * checkDistance);
                Gizmos.DrawWireSphere(leftOrigin + Vector3.down * checkDistance, 0.03f);

                // Right raycast
                Vector3 rightOrigin = origin + Vector3.right * (checkWidth * 0.5f);
                Gizmos.DrawLine(rightOrigin, rightOrigin + Vector3.down * checkDistance);
                Gizmos.DrawWireSphere(rightOrigin + Vector3.down * checkDistance, 0.03f);

                // Diagonal raycasts (for slopes)
                Gizmos.color = Color.cyan;
                Vector3 diagLeft = new Vector3(-0.3f, -1f, 0).normalized * checkDistance * 1.2f;
                Vector3 diagRight = new Vector3(0.3f, -1f, 0).normalized * checkDistance * 1.2f;
                Gizmos.DrawLine(origin, origin + diagLeft);
                Gizmos.DrawLine(origin, origin + diagRight);
            }

            // Front check (chest height)
            if (frontCheck != null)
            {
                float boxWidth = 0.05f;
                float boxHeight = data.frontCheckHeight;

                Gizmos.color = Color.yellow;

                Vector3 startPos = frontCheck.position;
                Gizmos.DrawWireCube(startPos, new Vector3(boxWidth, boxHeight, 0));

                // Draw cast to the right
                Vector3 endPosRight = startPos + Vector3.right * data.frontCheckDistance;
                Gizmos.DrawWireCube(endPosRight, new Vector3(boxWidth, boxHeight, 0));

                // Draw cast to the left
                Vector3 endPosLeft = startPos + Vector3.left * data.frontCheckDistance;
                Gizmos.DrawWireCube(endPosLeft, new Vector3(boxWidth, boxHeight, 0));

                // Lines connecting
                float halfHeight = boxHeight / 2f;
                Gizmos.DrawLine(startPos + new Vector3(0, halfHeight, 0), endPosRight + new Vector3(0, halfHeight, 0));
                Gizmos.DrawLine(startPos + new Vector3(0, -halfHeight, 0), endPosRight + new Vector3(0, -halfHeight, 0));
                Gizmos.DrawLine(startPos + new Vector3(0, halfHeight, 0), endPosLeft + new Vector3(0, halfHeight, 0));
                Gizmos.DrawLine(startPos + new Vector3(0, -halfHeight, 0), endPosLeft + new Vector3(0, -halfHeight, 0));
            }

            // Wall check
            Gizmos.color = Color.blue;
            if (wallCheck != null)
            {
                Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * data.wallCheckDistance);
                Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.left * data.wallCheckDistance);
            }
        }
    }
}
