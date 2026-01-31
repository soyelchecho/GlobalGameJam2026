using UnityEngine;
using Gameplay.Masks;
using Gameplay.Interactables;

namespace Gameplay.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(TouchInputHandler))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Header("Data")]
        [SerializeField] private PlayerData data;
        [SerializeField] private PlayerEvents events;

        [Header("Initial Settings")]
        [SerializeField] private int initialDirection = 1;

        // Note: Swipe handling moved to TouchInputHandler events
        // Connected via PlayerMaskController (up/down) and HandleSwipeLeft/Right (break attacks)

        [Header("Break Attack")]
        [SerializeField] private float breakAttackRange = 1.5f;

        private PlayerMotor motor;
        private PlayerStateMachine stateMachine;
        private MaskManager maskManager;
        private TouchInputHandler touchInputHandler;

        private int moveDirection;
        private int jumpCount;
        private bool hasUsedWallCling;

        public PlayerData Data => data;
        public PlayerMotor Motor => motor;
        public PlayerStateMachine StateMachine => stateMachine;
        public PlayerEvents Events => events;
        public MaskManager MaskManager => maskManager;
        
        public Vector2 WallContactNormal { get; set; }

        public int MoveDirection
        {
            get => moveDirection;
            set
            {
                if (moveDirection != value)
                {
                    moveDirection = value;
                    UpdateSpriteDirection();
                }
            }
        }

        public int JumpCount
        {
            get => jumpCount;
            set => jumpCount = value;
        }

        public bool IsGrounded => motor.CheckGrounded();
        public bool IsOnOneWayPlatform => motor.IsOnOneWayPlatform();
        public bool IsTouchingWall => motor.GetWallDirection() != 0;
        public int WallDirection => motor.GetWallDirection();
        public bool CanWallCling => !hasUsedWallCling;

        public void MarkWallClingUsed()
        {
            hasUsedWallCling = true;
        }

        public void ResetWallCling()
        {
            hasUsedWallCling = false;
        }

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            maskManager = GetComponent<MaskManager>();
            touchInputHandler = GetComponent<TouchInputHandler>();

            if (data == null)
            {
                Debug.LogError("PlayerData is not assigned to PlayerController!");
                return;
            }

            if (events == null)
            {
                Debug.LogWarning("PlayerEvents is not assigned. Creating runtime instance.");
                events = ScriptableObject.CreateInstance<PlayerEvents>();
            }

            motor.Initialize(data);
            stateMachine = new PlayerStateMachine(this);

            moveDirection = initialDirection;
            UpdateSpriteDirection();
        }

        private void Start()
        {
            stateMachine.Initialize(PlayerState.Moving);

            // Subscribe to input events
            touchInputHandler.OnTap.AddListener(HandleTap);
            touchInputHandler.OnSwipeLeft.AddListener(HandleSwipeLeft);
            touchInputHandler.OnSwipeRight.AddListener(HandleSwipeRight);
            // Note: OnSwipeUp/Down handled by PlayerMaskController
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            touchInputHandler.OnTap.RemoveListener(HandleTap);
            touchInputHandler.OnSwipeLeft.RemoveListener(HandleSwipeLeft);
            touchInputHandler.OnSwipeRight.RemoveListener(HandleSwipeRight);
        }

        private void Update()
        {
            stateMachine.Update();
            maskManager?.CurrentMask?.OnUpdate(this);

            // Editor-only keyboard controls
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnJumpInput();
            }

            // S/DownArrow now used for swipe down (mask unequip) in TouchInputHandler
            #endif
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        private void HandleTap(Vector2 position)
        {
            OnJumpInput();
        }

        public void TryDropThroughPlatform()
        {
            if (IsOnOneWayPlatform && IsGrounded)
            {
                motor.DropThroughPlatform();
            }
        }

        private void HandleSwipeLeft()
        {
            TryBreakInDirection(-1);
        }

        private void HandleSwipeRight()
        {
            TryBreakInDirection(1);
        }

        /// <summary>
        /// Attempts to break a breakable object in the given direction
        /// </summary>
        public void TryBreakInDirection(int direction)
        {
            // Get current mask ID (if any)
            string currentMaskId = maskManager?.CurrentMask?.MaskId ?? "";

            // Raycast in the direction
            Vector2 origin = transform.position;
            Vector2 dir = new Vector2(direction, 0);

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, breakAttackRange, data.breakableLayer);

            if (hit.collider != null)
            {
                // Check if it has IBreakable component
                IBreakable breakable = hit.collider.GetComponent<IBreakable>();

                if (breakable != null)
                {
                    if (breakable is BreakableObject breakableObj)
                    {
                        breakableObj.TryBreak(currentMaskId);
                    }
                    else if (breakable.CanBreak(currentMaskId))
                    {
                        breakable.Break();
                    }
                }
            }

            Debug.Log($"[Player] Swipe attack in direction {direction}, mask: {(string.IsNullOrEmpty(currentMaskId) ? "none" : currentMaskId)}");
        }

        public void OnJumpInput()
        {
            stateMachine.OnJumpPressed();
        }

        public void ChangeState(PlayerState newState)
        {
            stateMachine.ChangeState(newState);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            stateMachine.OnCollisionEnter(collision);
        }

        private void UpdateSpriteDirection()
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * moveDirection;
            transform.localScale = scale;
        }

        public float GetModifiedJumpForce(float baseForce)
        {
            float force = baseForce;
            maskManager?.CurrentMask?.ModifyJump(ref force);
            return force;
        }

        public float GetModifiedSpeed(float baseSpeed)
        {
            float speed = baseSpeed;
            maskManager?.CurrentMask?.ModifySpeed(ref speed);
            return speed;
        }

        public float GetModifiedWallClingDuration(float baseDuration)
        {
            float duration = baseDuration;
            maskManager?.CurrentMask?.ModifyWallCling(ref duration);
            return duration;
        }

        #if UNITY_EDITOR
        private void OnGUI()
        {
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 280, 220));
            GUILayout.Label($"State: {stateMachine.CurrentStateType} ({stateMachine.CurrentStateTimer:F2}s)");
            GUILayout.Label($"Direction: {moveDirection}");
            GUILayout.Label($"Jump Count: {jumpCount}/{data.maxJumps}");
            GUILayout.Label($"Grounded: {IsGrounded}");
            GUILayout.Label($"One-Way Platform: {IsOnOneWayPlatform}");
            GUILayout.Label($"Wall: {IsTouchingWall} (Dir: {WallDirection})");
            GUILayout.Label($"Wall Cling: {(hasUsedWallCling ? "USED" : "READY")}");
            GUILayout.Label($"Velocity: {motor.Velocity:F1}");
            GUILayout.EndArea();
        }
        #endif
    }
}
