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

        [Header("Animator Triggers")]
        [SerializeField] private string jumpTrigger = "Jump";
        [SerializeField] private string landTrigger = "Land";
        [SerializeField] private string fallTrigger = "Fall";
        [SerializeField] private string wallClingTrigger = "WallCling";
        [SerializeField] private string wallJumpTrigger = "WallJump";
        [SerializeField] private string punchTrigger = "Punch";

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
            events.OnPunch.AddListener(OnPunch);
        }

        private void OnDisable()
        {
            if (events == null) return;

            events.OnStateChanged.RemoveListener(OnStateChanged);
            events.OnPunch.RemoveListener(OnPunch);
        }

        private void OnStateChanged(PlayerState state)
        {
            if (animator == null) return;

            switch (state)
            {
                case PlayerState.Moving:
                    animator.SetTrigger(landTrigger);
                    break;

                case PlayerState.Jumping:
                    animator.SetTrigger(jumpTrigger);
                    break;

                case PlayerState.Falling:
                    animator.SetTrigger(fallTrigger);
                    break;

                case PlayerState.WallCling:
                    animator.SetTrigger(wallClingTrigger);
                    break;

                case PlayerState.WallJump:
                    animator.SetTrigger(wallJumpTrigger);
                    break;
            }
        }

        private void OnPunch()
        {
            if (animator == null) return;
            animator.SetTrigger(punchTrigger);
        }
    }
}
