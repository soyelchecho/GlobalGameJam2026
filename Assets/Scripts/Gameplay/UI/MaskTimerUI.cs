using UnityEngine;
using UnityEngine.UI;
using Gameplay.Masks;

namespace Gameplay.UI
{
    /// <summary>
    /// Circular timer UI that drains while a mask is equipped.
    /// Assign a UI Image with Fill Method set to Radial360.
    /// </summary>
    public class MaskTimerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskManager maskManager;
        [SerializeField] private Image fillImage;

        private void OnEnable()
        {
            if (maskManager != null)
            {
                maskManager.OnMaskEquipped.AddListener(OnMaskEquipped);
                maskManager.OnMaskUnequipped.AddListener(OnMaskUnequipped);
            }

            // Sync initial state
            if (maskManager != null && maskManager.HasMask)
                Show();
            else
                Hide();
        }

        private void OnDisable()
        {
            if (maskManager != null)
            {
                maskManager.OnMaskEquipped.RemoveListener(OnMaskEquipped);
                maskManager.OnMaskUnequipped.RemoveListener(OnMaskUnequipped);
            }
        }

        private void Update()
        {
            if (maskManager == null || fillImage == null) return;
            if (!maskManager.HasMask) return;

            fillImage.fillAmount = maskManager.MaskTimeNormalized;
        }

        private void OnMaskEquipped(Masks.IMask mask)
        {
            Show();
        }

        private void OnMaskUnequipped(Masks.IMask mask)
        {
            Hide();
        }

        private void Show()
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = 1f;
                fillImage.enabled = true;
            }
        }

        private void Hide()
        {
            if (fillImage != null)
                fillImage.enabled = false;
        }
    }
}
