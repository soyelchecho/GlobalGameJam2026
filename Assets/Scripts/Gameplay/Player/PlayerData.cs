using UnityEngine;

namespace Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 8f;

        [Header("Jump")]
        public float jumpForce = 16.5f;
        public float doubleJumpForce = 16.5f;
        public int maxJumps = 2;

        [Header("Wall")]
        public float wallSlideSpeed = 3f;
        public float wallClingDuration = 0.5f;
        public Vector2 wallJumpForce = new Vector2(8f, 16.5f);

        [Header("Physics")]
        public float gravityScale = 3f;
        public float fallMultiplier = 2.5f;
        public float lowJumpMultiplier = 2f;

        [Header("Detection")]
        [Tooltip("Solid ground that cannot be passed through")]
        public LayerMask groundLayer;
        [Tooltip("One-way platforms (use Platform Effector 2D)")]
        public LayerMask oneWayPlatformLayer;
        public LayerMask wallLayer;
        public float groundCheckRadius = 0.2f;
        public float wallCheckDistance = 0.5f;

        public LayerMask AllGroundLayers => groundLayer | oneWayPlatformLayer;
    }
}
