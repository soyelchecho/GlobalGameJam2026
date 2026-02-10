using UnityEngine;
using Gameplay.Player;
using Gameplay.UI;

namespace Gameplay.Audio
{
    /// <summary>
    /// Handles all player-related audio: jump, death, footsteps, wall interactions.
    /// Volume is controlled via VolumeManager.GetSFXVolume().
    ///
    /// SETUP:
    /// 1. Add to Player GameObject (or a child)
    /// 2. Assign PlayerEvents ScriptableObject
    /// 3. Assign audio clips in inspector
    /// </summary>
    public class PlayerAudioManager : MonoBehaviour
    {
        public static PlayerAudioManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerEvents playerEvents;

        [Header("Audio Source")]
        [SerializeField] private AudioSource audioSource;

        [Header("Movement Clips")]
        public AudioClip jumpClip;
        public AudioClip deathClip;
        public AudioClip wallScratchClip;

        [Header("Footsteps - Amethyst")]
        public AudioClip[] stepsAmethyst;

        [Header("Footsteps - Burning Rock")]
        public AudioClip[] stepsBurningRock;

        [Header("Footstep Settings")]
        [SerializeField] private float footstepInterval = 0.3f;
        [Range(0f, 1f)] public float footstepVolume = 0.75f;

        private float footstepTimer;
        private bool isMoving;
        private PlayerState currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

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
            // Stop footsteps if death panel is showing
            if (UIManager.Instance != null && UIManager.Instance.CurrentPanel == ActivePanel.Death)
                return;

            if (isMoving && currentState == PlayerState.Moving)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    PlayFootstepBurningRock();
                    footstepTimer = footstepInterval;
                }
            }
        }

        // ==========================================
        // EVENT HANDLERS
        // ==========================================

        private void OnJump(int jumpCount)
        {
            PlayJump();
        }

        private void OnWallCling(Vector2 position)
        {
            PlayWallScratch();
        }

        private void OnStateChanged(PlayerState newState)
        {
            currentState = newState;

            if (newState == PlayerState.Moving)
            {
                isMoving = true;
                footstepTimer = 0f;
            }
            else
            {
                isMoving = false;
            }
        }

        // ==========================================
        // PUBLIC PLAY METHODS
        // ==========================================

        public void PlayJump()
        {
            PlayClip(jumpClip);
        }

        public void PlayDeath()
        {
            PlayClip(deathClip);
        }

        public void PlayWallScratch()
        {
            PlayClip(wallScratchClip);
        }

        public void PlayFootstepAmethyst()
        {
            PlayRandomClip(stepsAmethyst, footstepVolume);
        }

        public void PlayFootstepBurningRock()
        {
            PlayRandomClip(stepsBurningRock, footstepVolume);
        }

        // ==========================================
        // CORE AUDIO METHODS
        // ==========================================

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || audioSource == null) return;
            audioSource.PlayOneShot(clip, VolumeManager.GetSFXVolume());
        }

        private void PlayRandomClip(AudioClip[] clips, float clipVolume = -1f)
        {
            if (clips == null || clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            float vol = clipVolume < 0 ? VolumeManager.GetSFXVolume() : clipVolume * VolumeManager.GetSFXVolume();
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip, vol);
        }
    }
}
