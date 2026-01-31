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

        [Header("Debug")]
        [SerializeField] private MaskBase startingMask;

        private PlayerController playerController;
        private IMask currentMask;

        public IMask CurrentMask => currentMask;
        public bool HasMask => currentMask != null;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            if (startingMask != null)
            {
                EquipMask(startingMask);
            }
        }

        public void EquipMask(IMask mask)
        {
            if (mask == null) return;

            if (currentMask != null)
            {
                UnequipCurrentMask();
            }

            currentMask = mask;
            currentMask.OnEquip(playerController);
            OnMaskEquipped?.Invoke(currentMask);

            Debug.Log($"Equipped mask: {mask.MaskId}");
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

            IMask previousMask = currentMask;
            currentMask.OnUnequip(playerController);
            currentMask = null;
            OnMaskUnequipped?.Invoke(previousMask);

            Debug.Log($"Unequipped mask: {previousMask.MaskId}");
        }

        public void UseAbility()
        {
            if (currentMask == null) return;

            OnMaskAbilityUsed?.Invoke(currentMask);
        }

        public T GetMaskAs<T>() where T : class, IMask
        {
            return currentMask as T;
        }
    }
}
