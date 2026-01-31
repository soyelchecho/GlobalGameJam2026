using UnityEngine;
using Gameplay.Masks;

namespace Gameplay.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Header("Data")]
        [SerializeField] private PlayerData data;
        [SerializeField] private PlayerEvents events;

        [Header("Initial Settings")]
        [SerializeField] private int initialDirection = 1;

        private PlayerMotor motor;
        private PlayerStateMachine stateMachine;
        private MaskManager maskManager;

        private int moveDirection;
        private int jumpCount;

        public PlayerData Data => data;
        public PlayerMotor Motor => motor;
        public PlayerStateMachine StateMachine => stateMachine;
        public PlayerEvents Events => events;
        public MaskManager MaskManager => maskManager;

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

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            maskManager = GetComponent<MaskManager>();

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
        }

        private void Update()
        {
            stateMachine.Update();
            HandleInput();
            maskManager?.CurrentMask?.OnUpdate(this);
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                OnJumpInput();
            }

            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnJumpInput();
            }

            // Drop through one-way platforms with S or Down arrow
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                TryDropThroughPlatform();
            }
            #endif
        }

        public void TryDropThroughPlatform()
        {
            if (IsOnOneWayPlatform && IsGrounded)
            {
                motor.DropThroughPlatform();
            }
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

            GUILayout.BeginArea(new Rect(10, 10, 220, 180));
            GUILayout.Label($"State: {stateMachine.CurrentStateType}");
            GUILayout.Label($"Direction: {moveDirection}");
            GUILayout.Label($"Jump Count: {jumpCount}/{data.maxJumps}");
            GUILayout.Label($"Grounded: {IsGrounded}");
            GUILayout.Label($"One-Way Platform: {IsOnOneWayPlatform}");
            GUILayout.Label($"Wall: {IsTouchingWall} (Dir: {WallDirection})");
            GUILayout.Label($"Velocity: {motor.Velocity:F1}");
            GUILayout.EndArea();
        }
        #endif
    }
}
