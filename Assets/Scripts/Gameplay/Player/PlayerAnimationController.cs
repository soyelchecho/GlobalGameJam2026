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
            events.OnJump.AddListener(OnJump);
            events.OnLand.AddListener(OnLand);
            events.OnWallCling.AddListener(OnWallCling);
            events.OnWallJump.AddListener(OnWallJump);
            events.OnPunch.AddListener(OnPunch);
        }

        private void OnDisable()
        {
            if (events == null) return;

            events.OnStateChanged.RemoveListener(OnStateChanged);
            events.OnJump.RemoveListener(OnJump);
            events.OnLand.RemoveListener(OnLand);
            events.OnWallCling.RemoveListener(OnWallCling);
            events.OnWallJump.RemoveListener(OnWallJump);
            events.OnPunch.RemoveListener(OnPunch);
        }

        private void OnStateChanged(PlayerState state)
        {
            if (animator == null) return;

            switch (state)
            {
                case PlayerState.Moving:
#if UNITY_EDITOR
                    Debug.Log($"[Anim] Trigger: {landTrigger}");
#endif
                    animator.SetTrigger(landTrigger);
                    break;

                case PlayerState.Falling:
#if UNITY_EDITOR
                    Debug.Log($"[Anim] Trigger: {fallTrigger}");
#endif
                    animator.SetTrigger(fallTrigger);
                    break;

                case PlayerState.WallCling:
#if UNITY_EDITOR
                    Debug.Log($"[Anim] Trigger: {wallClingTrigger}");
#endif
                    animator.SetTrigger(wallClingTrigger);
                    break;

                case PlayerState.WallJump:
#if UNITY_EDITOR
                    Debug.Log($"[Anim] Trigger: {wallJumpTrigger}");
#endif
                    animator.SetTrigger(wallJumpTrigger);
                    break;
            }
        }

        private void OnJump(int jumpCount)
        {
            if (animator == null) return;
            animator.SetTrigger(jumpTrigger);
        }

        private void OnLand(float fallSpeed)
        {
#if UNITY_EDITOR
            Debug.Log($"[Anim] Trigger: {landTrigger} (fallSpeed: {fallSpeed:F1})");
#endif
            if (animator == null) return;
            animator.SetTrigger(landTrigger);
        }

        private void OnWallCling(Vector2 position)
        {
#if UNITY_EDITOR
            Debug.Log($"[Anim] Trigger: {wallClingTrigger}");
#endif
            if (animator == null) return;
            animator.SetTrigger(wallClingTrigger);
        }

        private void OnWallJump(int newDirection)
        {
#if UNITY_EDITOR
            Debug.Log($"[Anim] Trigger: {wallJumpTrigger} (dir: {newDirection})");
#endif
            if (animator == null) return;
            animator.SetTrigger(wallJumpTrigger);
        }

        private void OnPunch()
        {
#if UNITY_EDITOR
            Debug.Log($"[Anim] Trigger: {punchTrigger}");
#endif
            if (animator == null) return;
            animator.SetTrigger(punchTrigger);
        }
    }
}
