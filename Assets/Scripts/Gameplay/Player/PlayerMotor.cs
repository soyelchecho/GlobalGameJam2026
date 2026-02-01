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

            // If moving upward, we're not grounded
            // This prevents false positives when jumping through one-way platforms
            if (rb.velocity.y > 0.5f) return false;

            // Use BoxCast - groundCheckSize controls both width and distance
            float boxWidth = data.groundCheckSize * 2f; // Size is "radius", so diameter = size * 2
            float boxHeight = 0.05f; // Thin box
            Vector2 boxSize = new Vector2(boxWidth, boxHeight);
            Vector2 boxOrigin = groundCheck.position;

            RaycastHit2D hit = Physics2D.BoxCast(
                boxOrigin,
                boxSize,
                0f, // No rotation
                Vector2.down,
                data.groundCheckSize, // Distance also uses groundCheckSize
                data.AllGroundLayers
            );

            if (hit.collider == null) return false;

            // Only grounded if the surface normal points UP (we're on top, not inside/below)
            // normal.y > 0.7 means surface is roughly horizontal (floor)
            if (hit.normal.y <= 0.7f) return false;

            // For one-way platforms: make sure player is FULLY above, not passing through
            if (((1 << hit.collider.gameObject.layer) & data.oneWayPlatformLayer) != 0)
            {
                // Player's bottom must be at or above the platform's top surface
                float playerBottom = playerCollider.bounds.min.y;
                float platformTop = hit.collider.bounds.max.y;

                // Tolerance for physics penetration when landing
                // Use smaller tolerance for thin platforms
                float tolerance = Mathf.Min(0.2f, hit.collider.bounds.size.y * 0.5f);
                if (playerBottom < platformTop - tolerance)
                {
                    return false; // Still passing through, not standing on top
                }
            }

            return true;
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

            // Ground check (BoxCast visualization)
            if (groundCheck != null)
            {
                float boxWidth = data.groundCheckSize * 2f;
                float boxHeight = 0.05f;

                Gizmos.color = Color.green;

                // Draw box at start position
                Vector3 startPos = groundCheck.position;
                Gizmos.DrawWireCube(startPos, new Vector3(boxWidth, boxHeight, 0));

                // Draw box at end position (after cast distance)
                Vector3 endPos = startPos + Vector3.down * data.groundCheckSize;
                Gizmos.DrawWireCube(endPos, new Vector3(boxWidth, boxHeight, 0));

                // Draw lines connecting corners
                float halfWidth = boxWidth / 2f;
                Gizmos.DrawLine(startPos + new Vector3(-halfWidth, 0, 0), endPos + new Vector3(-halfWidth, 0, 0));
                Gizmos.DrawLine(startPos + new Vector3(halfWidth, 0, 0), endPos + new Vector3(halfWidth, 0, 0));
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
