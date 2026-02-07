using UnityEngine;
using Gameplay.Masks;

namespace Gameplay.Player
{
    /// <summary>
    /// Connects swipe input to mask equip/unequip
    /// </summary>
    public class PlayerMaskController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskManager maskManager;
        [SerializeField] private TouchInputHandler inputHandler;

        [Header("Masks")]
        [SerializeField] private MaskBase timeMask;

        public MaskBase TimeMask => timeMask;

        private void Awake()
        {
            if (maskManager == null)
                maskManager = GetComponent<MaskManager>();

            if (inputHandler == null)
                inputHandler = GetComponent<TouchInputHandler>();
        }

        private void OnEnable()
        {
            if (inputHandler != null)
            {
                inputHandler.OnSwipeUp.AddListener(OnSwipeUp);
                inputHandler.OnSwipeDown.AddListener(OnSwipeDown);
            }
        }

        private void OnDisable()
        {
            if (inputHandler != null)
            {
                inputHandler.OnSwipeUp.RemoveListener(OnSwipeUp);
                inputHandler.OnSwipeDown.RemoveListener(OnSwipeDown);
            }
        }

        private void OnSwipeDown()
        {
#if UNITY_EDITOR
            Debug.Log("[PlayerMaskController] Swipe Down - Equip mask");
#endif
            if (maskManager == null || timeMask == null) return;

            // Only equip if not already wearing this mask
            if ((object)maskManager.CurrentMask != timeMask)
            {
                maskManager.EquipMask(timeMask);
            }
        }

        private void OnSwipeUp()
        {
#if UNITY_EDITOR
            Debug.Log("[PlayerMaskController] Swipe up - Unequip mask");
#endif
            if (maskManager == null) return;

            // Unequip if wearing any mask
            if (maskManager.HasMask)
            {
                maskManager.UnequipCurrentMask();
            }
        }
    }
}
