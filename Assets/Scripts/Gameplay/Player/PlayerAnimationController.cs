using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// Connects PlayerEvents to Animator for player animations.
    /// Attach to the same GameObject as PlayerController.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerEvents events;

        // Animator parameter names - customize to match your Animator Controller
        [Header("Animator Parameters")]
        [SerializeField] private string stateParam = "State";
        [SerializeField] private string jumpTrigger = "Jump";
        [SerializeField] private string landTrigger = "Land";
        [SerializeField] private string wallClingTrigger = "WallCling";
        [SerializeField] private string wallJumpTrigger = "WallJump";

        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (events == null && playerController != null)
                events = playerController.Events;
        }

        private void OnEnable()
        {
            if (events == null) return;

            events.OnStateChanged.AddListener(OnStateChanged);
            events.OnJump.AddListener(OnJump);
            events.OnLand.AddListener(OnLand);
            events.OnWallCling.AddListener(OnWallCling);
            events.OnWallJump.AddListener(OnWallJump);
            events.OnDirectionChanged.AddListener(OnDirectionChanged);
        }

        private void OnDisable()
        {
            if (events == null) return;

            events.OnStateChanged.RemoveListener(OnStateChanged);
            events.OnJump.RemoveListener(OnJump);
            events.OnLand.RemoveListener(OnLand);
            events.OnWallCling.RemoveListener(OnWallCling);
            events.OnWallJump.RemoveListener(OnWallJump);
            events.OnDirectionChanged.RemoveListener(OnDirectionChanged);
        }

        private void OnStateChanged(PlayerState state)
        {
            if (animator == null) return;

            // Set integer parameter for state-based blend tree or transitions
            animator.SetInteger(stateParam, (int)state);

            // Alternative: use bools for each state
            // animator.SetBool("IsMoving", state == PlayerState.Moving);
            // animator.SetBool("IsFalling", state == PlayerState.Falling);
            // etc.
        }

        private void OnJump(int jumpCount)
        {
            if (animator == null) return;

            animator.SetTrigger(jumpTrigger);

            // Optional: different animation for double jump
            // animator.SetInteger("JumpCount", jumpCount);
        }

        private void OnLand(float fallSpeed)
        {
            if (animator == null) return;

            animator.SetTrigger(landTrigger);

            // Optional: harder landing animation for high falls
            // animator.SetFloat("LandImpact", fallSpeed);
        }

        private void OnWallCling(Vector2 position)
        {
            if (animator == null) return;

            animator.SetTrigger(wallClingTrigger);
        }

        private void OnWallJump(int newDirection)
        {
            if (animator == null) return;

            animator.SetTrigger(wallJumpTrigger);
        }

        private void OnDirectionChanged(int direction)
        {
            // Direction flip is already handled by transform.localScale in PlayerController
            // But you could trigger a turn animation here if needed
            // animator.SetTrigger("Turn");
        }
    }
}
