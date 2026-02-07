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

        [Header("Debug")]
        [SerializeField] private MaskBase startingMask;

        private PlayerController playerController;
        private IMask currentMask;
        private bool canEquipMask;

        public IMask CurrentMask => currentMask;
        public bool HasMask => currentMask != null;
        public bool CanEquipMask => canEquipMask;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
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

        public void EquipMask(IMask mask)
        {
            if (mask == null) return;
            if (!canEquipMask) return;

            if (currentMask != null)
            {
                UnequipCurrentMask();
            }

            // Equip mask logic immediately
            currentMask = mask;
            currentMask.OnEquip(playerController);
            OnMaskEquipped?.Invoke(currentMask);

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

        public T GetMaskAs<T>() where T : class, IMask
        {
            return currentMask as T;
        }
    }
}
