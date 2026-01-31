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

        [Header("Drop Through")]
        [SerializeField] private float dropThroughDuration = 0.25f;

        private Rigidbody2D rb;
        private Collider2D playerCollider;
        private PlayerData data;
        private float originalGravityScale;
        private Coroutine dropThroughCoroutine;

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
            return Physics2D.OverlapCircle(groundCheck.position, data.groundCheckRadius, data.AllGroundLayers);
        }

        public bool IsOnOneWayPlatform()
        {
            if (groundCheck == null) return false;
            return Physics2D.OverlapCircle(groundCheck.position, data.groundCheckRadius, data.oneWayPlatformLayer);
        }

        public bool CheckWall(int direction)
        {
            if (wallCheck == null) return false;
            Vector2 wallCheckDir = new Vector2(direction, 0);
            RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, wallCheckDir, data.wallCheckDistance, data.wallLayer);
            return hit.collider != null;
        }

        public int GetWallDirection()
        {
            if (CheckWall(1)) return 1;
            if (CheckWall(-1)) return -1;
            return 0;
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

            // Ground check (green = solid, yellow = includes one-way)
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, data.groundCheckRadius);
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
