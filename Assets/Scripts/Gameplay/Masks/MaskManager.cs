using UnityEngine;
using UnityEngine.Events;
using Gameplay.Player;

namespace Gameplay.Masks
{
    [System.Serializable]
    public class MaskEquippedEvent : UnityEvent<IMask> { }

    [System.Serializable]
    public class MaskUnequippedEvent : UnityEvent<IMask> { }

    [System.Serializable]
    public class MaskAbilityUsedEvent : UnityEvent<IMask> { }

    public class MaskManager : MonoBehaviour
    {
        [Header("Events")]
        public MaskEquippedEvent OnMaskEquipped = new MaskEquippedEvent();
        public MaskUnequippedEvent OnMaskUnequipped = new MaskUnequippedEvent();
        public MaskAbilityUsedEvent OnMaskAbilityUsed = new MaskAbilityUsedEvent();
        public UnityEvent OnMaskUnlocked = new UnityEvent();

        [Header("Visuals")]
        [Tooltip("Child GameObject containing the mask sprite overlay")]
        [SerializeField] private GameObject maskSpriteOverlay;

        [Header("Animation")]
        [Tooltip("Animator for mask equip/unequip animations")]
        [SerializeField] private Animator maskAnimator;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Pitch for unequip sound (higher = faster)")]
        [SerializeField] private float unequipPitch = 1.3f;
        [Tooltip("Volume for unequip sound (lower than equip)")]
        [Range(0f, 1f)]
        [SerializeField] private float unequipVolume = 0.6f;

        [Header("Duration")]
        [Tooltip("How long the mask stays equipped before auto-unequipping (seconds)")]
        [SerializeField] private float maskDuration = 10f;
        [Tooltip("Cooldown before the mask can be re-equipped after expiring (0 = no cooldown)")]
        [SerializeField] private float cooldownDuration = 0f;

        [Header("Debug")]
        [SerializeField] private MaskBase startingMask;

        private PlayerController playerController;
        private IMask currentMask;
        private bool canEquipMask;
        private float remainingTime;
        private float cooldownTimer;
        private bool isOnCooldown;
        private bool timerPaused;

        public IMask CurrentMask => currentMask;
        public bool HasMask => currentMask != null;
        public bool CanEquipMask => canEquipMask;
        public bool IsOnCooldown => isOnCooldown;

        /// <summary>
        /// Normalized mask time remaining (1 = full, 0 = expired). Returns 0 when no mask equipped.
        /// </summary>
        public float MaskTimeNormalized => (currentMask != null && maskDuration > 0f)
            ? Mathf.Clamp01(remainingTime / maskDuration)
            : 0f;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            // Hide mask overlay initially
            if (maskSpriteOverlay != null)
                maskSpriteOverlay.SetActive(false);

            if (startingMask != null)
            {
                EquipMask(startingMask);
            }
        }

        private void Update()
        {
            // Tick mask duration
            if (currentMask != null && maskDuration > 0f && !timerPaused)
            {
                remainingTime -= Time.deltaTime;
                if (remainingTime <= 0f)
                {
                    remainingTime = 0f;
                    UnequipCurrentMask();

                    // Start cooldown if configured
                    if (cooldownDuration > 0f)
                    {
                        isOnCooldown = true;
                        cooldownTimer = cooldownDuration;
                    }
                }
            }

            // Tick cooldown
            if (isOnCooldown)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    isOnCooldown = false;
                }
            }
        }

        public void EquipMask(IMask mask)
        {
            if (mask == null) return;
            if (!canEquipMask) return;
            if (isOnCooldown) return;

            if (currentMask != null)
            {
                UnequipCurrentMask();
            }

            // Equip mask logic immediately
            currentMask = mask;
            remainingTime = maskDuration;
            currentMask.OnEquip(playerController);
            OnMaskEquipped?.Invoke(currentMask);

            // Play equip sound
            if (currentMask.EquipSound != null && audioSource != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(currentMask.EquipSound, Audio.VolumeManager.GetSFXVolume());
            }

            // Play animation, sprite will show when animation calls OnEquipAnimationComplete()
            if (maskAnimator != null && !string.IsNullOrEmpty(currentMask.EquipAnimationTrigger))
            {
                maskAnimator.SetTrigger(currentMask.EquipAnimationTrigger);
            }
            else
            {
                // No animator, show sprite immediately
                if (maskSpriteOverlay != null)
                    maskSpriteOverlay.SetActive(true);
            }
        }

        /// <summary>
        /// Called by Animation Event at end of equip animation to show mask sprite
        /// </summary>
        public void OnEquipAnimationComplete()
        {
            Debug.Log("mask animation complete");
            if (maskSpriteOverlay != null)
                maskSpriteOverlay.SetActive(true);
        }

        public void EquipStartingMask()
        {
            if (startingMask != null)
            {
                EquipMask(startingMask);
            }
        }

        public void UnequipCurrentMask()
        {
            if (currentMask == null) return;

            // Unequip mask logic immediately
            IMask previousMask = currentMask;
            currentMask.OnUnequip(playerController);
            currentMask = null;
            OnMaskUnequipped?.Invoke(previousMask);

            // Play unequip sound faster and quieter
            if (previousMask.UnequipSound != null && audioSource != null)
            {
                audioSource.pitch = unequipPitch;
                audioSource.PlayOneShot(previousMask.UnequipSound, unequipVolume * Audio.VolumeManager.GetSFXVolume());
            }

            // Play animation, sprite will hide when animation calls OnUnequipAnimationComplete()
            if (maskAnimator != null && !string.IsNullOrEmpty(previousMask.UnequipAnimationTrigger))
            {
                maskAnimator.SetTrigger(previousMask.UnequipAnimationTrigger);
            }
            else
            {
                // No animator, hide sprite immediately
                if (maskSpriteOverlay != null)
                    maskSpriteOverlay.SetActive(false);
            }
        }

        /// <summary>
        /// Called by Animation Event at end of unequip animation to hide mask sprite
        /// </summary>
        public void OnUnequipAnimationComplete()
        {
            if (maskSpriteOverlay != null)
                maskSpriteOverlay.SetActive(false);
        }

        public void UseAbility()
        {
            if (currentMask == null) return;

            OnMaskAbilityUsed?.Invoke(currentMask);
        }

        public void UnlockMaskEquipping()
        {
            if (canEquipMask) return;

            canEquipMask = true;
            Debug.Log("[MaskManager] Mask equipping unlocked");
            OnMaskUnlocked?.Invoke();
        }

        public void PauseTimer()
        {
            timerPaused = true;
        }

        public void ResumeTimer()
        {
            timerPaused = false;
        }

        public T GetMaskAs<T>() where T : class, IMask
        {
            return currentMask as T;
        }
    }
}
