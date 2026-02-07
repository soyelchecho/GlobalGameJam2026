using UnityEngine;
using Gameplay.Player;

namespace Gameplay.Audio
{
    /// <summary>
    /// Hooks player events to audio.
    /// Attach to the Player GameObject or any persistent object.
    /// Requires a reference to the PlayerEvents ScriptableObject.
    /// </summary>
    public class PlayerAudioHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerEvents playerEvents;

        [Header("Footstep Settings")]
        [Tooltip("Time between footstep sounds while moving")]
        [SerializeField] private float footstepInterval = 0.3f;

        private float footstepTimer;
        private bool isMoving;
        private PlayerState currentState;

        private void OnEnable()
        {
            if (playerEvents == null) return;

            playerEvents.OnJump.AddListener(OnJump);
            playerEvents.OnWallCling.AddListener(OnWallCling);
            playerEvents.OnStateChanged.AddListener(OnStateChanged);
        }

        private void OnDisable()
        {
            if (playerEvents == null) return;

            playerEvents.OnJump.RemoveListener(OnJump);
            playerEvents.OnWallCling.RemoveListener(OnWallCling);
            playerEvents.OnStateChanged.RemoveListener(OnStateChanged);
        }

        private void Update()
        {
            // Handle footsteps while moving
            if (isMoving && currentState == PlayerState.Moving)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    PlayFootstep();
                    footstepTimer = footstepInterval;
                }
            }
        }

        private void OnJump(int jumpCount)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayJump();
        }

        private void OnWallCling(Vector2 position)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWallScratch();
        }

        private void OnStateChanged(PlayerState newState)
        {
            currentState = newState;

            // Start/stop footstep tracking based on state
            if (newState == PlayerState.Moving)
            {
                isMoving = true;
                footstepTimer = 0f; // Play first footstep immediately
            }
            else
            {
                isMoving = false;
            }
        }

        private void PlayFootstep()
        {
            if (AudioManager.Instance == null) return;

            // TODO: You can add logic here to detect surface type
            // For now, default to amethyst steps
            AudioManager.Instance.PlayFootstepAmethyst();
        }

        /// <summary>
        /// Call this when player dies (from RisingLava or other death sources)
        /// </summary>
        public void PlayDeath()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayDeath();
        }
    }
}
