using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Gameplay.Player;
using Gameplay.Masks;

namespace Gameplay.Hazards
{
    public enum LavaStartMode
    {
        Manual,
        OnAwake,
        OnFirstJump,
        OnFirstJumpAfterMask
    }

    /// <summary>
    /// Rising lava that chases the player upward.
    /// Kills the player on contact.
    /// </summary>
    public class RisingLava : MonoBehaviour
    {
        [Header("Rising Settings")]
        [Tooltip("Speed at which lava rises (units per second)")]
        [SerializeField] private float riseSpeed = 1f;

        [Tooltip("When the lava starts rising")]
        [SerializeField] private LavaStartMode startMode = LavaStartMode.OnFirstJump;

        [Tooltip("Delay in seconds before lava starts rising")]
        [SerializeField] private float startDelay = 2f;

        [Tooltip("PlayerEvents asset (required for OnFirstJump/OnFirstJumpAfterMask mode)")]
        [SerializeField] private PlayerEvents playerEvents;

        [Tooltip("MaskManager reference (required for OnFirstJumpAfterMask mode)")]
        [SerializeField] private MaskManager maskManager;

        [Tooltip("Maximum Y position the lava can reach (0 = no limit)")]
        [SerializeField] private float maxHeight = 0f;

        [Header("Detection")]
        [Tooltip("Tag used to identify the player")]
        [SerializeField] private string playerTag = "Player";

        [Header("Animation")]
        [Tooltip("Animator trigger for death animation")]
        [SerializeField] private string deathTrigger = "Death";

        [Header("Death Timing")]
        [Tooltip("Delay before reloading scene after death")]
        [SerializeField] private float reloadDelay = 1.5f;

        [Header("Events")]
        public UnityEvent OnPlayerTouched;
        public UnityEvent OnMaxHeightReached;

        private bool isRising;
        private bool hasReachedMax;
        private bool hasKilledPlayer;

        public bool IsRising => isRising;
        public float RiseSpeed
        {
            get => riseSpeed;
            set => riseSpeed = value;
        }

        private void Start()
        {
            if (startMode == LavaStartMode.OnAwake)
            {
                StartRising();
            }
            else if (startMode == LavaStartMode.OnFirstJump && playerEvents != null)
            {
                playerEvents.OnJump.AddListener(OnFirstJump);
            }
            else if (startMode == LavaStartMode.OnFirstJumpAfterMask)
            {
                if (maskManager != null)
                {
                    maskManager.OnMaskUnlocked.AddListener(OnMaskUnlocked);
                }
            }
        }

        private void OnDestroy()
        {
            if (playerEvents != null)
            {
                playerEvents.OnJump.RemoveListener(OnFirstJump);
            }
            if (maskManager != null)
            {
                maskManager.OnMaskUnlocked.RemoveListener(OnMaskUnlocked);
            }
        }

        private void OnMaskUnlocked()
        {
            maskManager.OnMaskUnlocked.RemoveListener(OnMaskUnlocked);

            if (playerEvents != null)
            {
                playerEvents.OnJump.AddListener(OnFirstJump);
            }
        }

        private void OnFirstJump(int jumpCount)
        {
            StartRising();
            playerEvents.OnJump.RemoveListener(OnFirstJump);
        }

        private void Update()
        {
            if (!isRising) return;

            // Rise upward
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // Check max height
            if (maxHeight > 0 && !hasReachedMax && transform.position.y >= maxHeight)
            {
                hasReachedMax = true;
                OnMaxHeightReached?.Invoke();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
            {
                OnPlayerTouched?.Invoke();
                KillPlayer(other.gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(playerTag))
            {
                OnPlayerTouched?.Invoke();
                KillPlayer(collision.gameObject);
            }
        }

        private void KillPlayer(GameObject player)
        {
            if (hasKilledPlayer) return;
            hasKilledPlayer = true;

            // Freeze lava
            isRising = false;

            // Disable ALL player components that could cause movement
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Static;
            }

            // Trigger death animation
            var animator = player.GetComponentInChildren<Animator>();
            if (animator != null)
                animator.SetTrigger(deathTrigger);

            // Reload scene after delay
            Invoke(nameof(ReloadScene), reloadDelay);
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Start the lava rising (after delay)
        /// </summary>
        public void StartRising()
        {
            StartCoroutine(StartRisingAfterDelay());
        }

        private IEnumerator StartRisingAfterDelay()
        {
            yield return new WaitForSeconds(startDelay);
            isRising = true;
        }

        /// <summary>
        /// Stop the lava from rising
        /// </summary>
        public void StopRising()
        {
            isRising = false;
        }

        /// <summary>
        /// Set rise speed at runtime
        /// </summary>
        public void SetRiseSpeed(float speed)
        {
            riseSpeed = speed;
        }

        /// <summary>
        /// Reset lava to a specific position
        /// </summary>
        public void ResetToPosition(Vector3 position)
        {
            transform.position = position;
            hasReachedMax = false;
        }
    }
}
