using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Gameplay.Player;
using Gameplay.Masks;
using Gameplay.UI;
using Gameplay.Audio;

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

        [Header("Catch-up")]
        [Tooltip("Distance ahead of lava the player must be to trigger catch-up speed")]
        [SerializeField] private float catchUpDistance = 10f;
        [Tooltip("Rise speed used when player is beyond catch-up distance")]
        [SerializeField] private float catchUpSpeed = 3f;

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

        private static bool playerIsDead;

        private bool playIntenseThemeOnRise = true;
        public bool PlayIntenseThemeOnRise { get => playIntenseThemeOnRise; set => playIntenseThemeOnRise = value; }

        private bool isRising;
        private bool hasReachedMax;
        private Transform player;
        private float initialDistance;

        public bool IsRising => isRising;
        public float RiseSpeed
        {
            get => riseSpeed;
            set => riseSpeed = value;
        }

        private void Start()
        {
            playerIsDead = false;

            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
                player = playerObj.transform;

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
            if (!isRising || playerIsDead) return;

            // Rise upward, speeding up if player is too far ahead
            float currentSpeed = riseSpeed;
            if (player != null && catchUpDistance > 0f)
            {
                float distance = (player.position.y - transform.position.y) - initialDistance;
                if (distance > catchUpDistance * 1.5f)
                    currentSpeed = catchUpSpeed * 4f;
                else if (distance > catchUpDistance * 1.25f)
                    currentSpeed = catchUpSpeed * 2f;
                else if (distance > catchUpDistance)
                    currentSpeed = catchUpSpeed;
            }

            transform.position += Vector3.up * currentSpeed * Time.deltaTime;

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
            if (playerIsDead) return;
            playerIsDead = true;

            // Stop this lava
            StopAllCoroutines();
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

            // Play death sound
            if (PlayerAudioManager.Instance != null)
                PlayerAudioManager.Instance.PlayDeath();

            // Show death panel and reload after dismissed
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            yield return new WaitForSeconds(reloadDelay);

            // Show death panel — navigation handled by buttons on the panel
            if (UIManager.Instance != null)
                UIManager.Instance.ShowDeathPanel();
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

            if (player != null)
                initialDistance = player.position.y - transform.position.y;

            isRising = true;

            // Switch to intense music
            if (playIntenseThemeOnRise && AudioManager.Instance != null)
                AudioManager.Instance.PlayIntenseTheme();
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
