using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Gameplay.CameraSystem;
using Gameplay.Player;
using Gameplay.UI;
using Gameplay.Hazards;

namespace Gameplay.Cinematics
{
    /// <summary>
    /// Orchestrates the lava warning cinematic sequence:
    /// 1. Freeze player
    /// 2. Camera shake (earthquake effect)
    /// 3. Pan camera down to show lava
    /// 4. Hold for a moment
    /// 5. Pan back to player
    /// 6. Unfreeze player
    /// 7. Start lava rising
    /// </summary>
    public class LavaWarningSequence : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraFollowY cameraFollow;
        [SerializeField] private Transform lavaTransform;
        [SerializeField] private RisingLava[] risingLavas;
        [SerializeField] private PlayerController playerController;

        [Header("Shake Settings")]
        [Tooltip("Duration of earthquake shake")]
        [SerializeField] private float shakeDuration = 1f;
        [Tooltip("Intensity of shake")]
        [SerializeField] private float shakeIntensity = 0.3f;

        [Header("Pan Settings")]
        [Tooltip("How long to pan down to lava")]
        [SerializeField] private float panDownDuration = 1.5f;
        [Tooltip("How long to hold on lava")]
        [SerializeField] private float holdDuration = 1.5f;
        [Tooltip("How long to pan back to player")]
        [SerializeField] private float panUpDuration = 1f;
        [Tooltip("Offset above lava to show")]
        [SerializeField] private float lavaViewOffset = 5f;

        [Header("Trigger")]
        [Tooltip("Automatically trigger after mask info panel is dismissed")]
        [SerializeField] private bool triggerOnMaskDismissed = true;

        [Header("Events")]
        public UnityEvent OnSequenceStart;
        public UnityEvent OnSequenceEnd;

        private bool hasPlayed;
        private Coroutine sequenceCoroutine;

        private void Start()
        {
            // Auto-find references if not assigned
            if (cameraFollow == null)
                cameraFollow = Camera.main?.GetComponent<CameraFollowY>();

            if (playerController == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerController = player.GetComponent<PlayerController>();
            }

            if (risingLavas == null || risingLavas.Length == 0)
                risingLavas = FindObjectsOfType<RisingLava>();

            if (lavaTransform == null && risingLavas.Length > 0)
                lavaTransform = risingLavas[0].transform;

            // Subscribe to mask dismissed event
            if (triggerOnMaskDismissed && UIManager.Instance != null)
            {
                UIManager.Instance.OnMaskInfoDismissed.AddListener(OnMaskDismissed);
            }
        }

        private void OnDestroy()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnMaskInfoDismissed.RemoveListener(OnMaskDismissed);
            }
        }

        private void OnMaskDismissed()
        {
            PlaySequence();
        }

        /// <summary>
        /// Play the lava warning sequence
        /// </summary>
        public void PlaySequence()
        {
            if (hasPlayed) return;
            if (cameraFollow == null || lavaTransform == null || risingLavas.Length == 0) return;

            hasPlayed = true;

            if (sequenceCoroutine != null)
                StopCoroutine(sequenceCoroutine);

            sequenceCoroutine = StartCoroutine(SequenceCoroutine());
        }

        private IEnumerator SequenceCoroutine()
        {
            OnSequenceStart?.Invoke();

            // Store player's current position for return
            float playerY = cameraFollow.GetCurrentY();

            // 1. Freeze player
            if (playerController != null)
                playerController.enabled = false;

            // 2. Enable cinematic mode
            cameraFollow.EnableCinematicMode();

            // 3. Shake camera (earthquake)
            cameraFollow.Shake(shakeDuration, shakeIntensity);
            yield return new WaitForSeconds(shakeDuration);

            // 4. Pan down to lava
            float lavaY = lavaTransform.position.y + lavaViewOffset;
            yield return StartCoroutine(cameraFollow.PanToY(lavaY, panDownDuration));

            // 5. Hold on lava
            yield return new WaitForSeconds(holdDuration);

            // 6. Pan back to player
            yield return StartCoroutine(cameraFollow.PanToY(playerY, panUpDuration));

            // 7. Disable cinematic mode
            cameraFollow.DisableCinematicMode();

            // 8. Unfreeze player
            if (playerController != null)
                playerController.enabled = true;

            // 9. Start all lavas rising
            foreach (var lava in risingLavas)
            {
                if (lava != null)
                    lava.StartRising();
            }

            OnSequenceEnd?.Invoke();
            sequenceCoroutine = null;
        }

        /// <summary>
        /// Reset sequence so it can play again (useful for testing)
        /// </summary>
        public void ResetSequence()
        {
            hasPlayed = false;
        }
    }
}
