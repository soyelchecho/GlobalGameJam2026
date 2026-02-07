using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Gameplay.Masks;
using Gameplay.Player;
using Gameplay.UI;

namespace Gameplay.Interactables
{
    /// <summary>
    /// Trigger that unlocks mask equipping when the player enters.
    /// </summary>
    public class MaskPickup : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private string playerTag = "Player";

        [Header("Visuals")]
        [Tooltip("Object to hide/destroy when picked up (optional)")]
        [SerializeField] private GameObject objectToHide;
        [SerializeField] private bool destroyInsteadOfHide;

        [Header("Pickup Animation")]
        [Tooltip("Pause player and play equip animation on pickup")]
        [SerializeField] private bool equipOnPickup;
        [SerializeField] private float pauseDuration = 0.5f;

        [Header("UI")]
        [Tooltip("Show mask info panel after pickup")]
        [SerializeField] private bool showMaskInfoPanel;

        [Header("Events")]
        public UnityEvent OnPickedUp;

        private bool hasBeenPickedUp;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasBeenPickedUp) return;
            if (!other.CompareTag(playerTag)) return;

            var maskManager = other.GetComponent<MaskManager>();
            if (maskManager == null) return;

            hasBeenPickedUp = true;

            if (objectToHide != null)
            {
                if (destroyInsteadOfHide)
                    Destroy(objectToHide);
                else
                    objectToHide.SetActive(false);
            }

            if (equipOnPickup)
            {
                var playerController = other.GetComponent<PlayerController>();
                var playerMaskController = other.GetComponent<PlayerMaskController>();
                StartCoroutine(PickupSequence(playerController, maskManager, playerMaskController));
            }
            else
            {
                maskManager.UnlockMaskEquipping();

                if (showMaskInfoPanel && UIManager.Instance != null)
                    UIManager.Instance.ShowMaskInfoPanel();

                OnPickedUp?.Invoke();
            }
        }

        private IEnumerator PickupSequence(PlayerController player, MaskManager maskManager, PlayerMaskController playerMaskController)
        {
            // Freeze player
            if (player != null)
            {
                player.enabled = false;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.velocity = Vector2.zero;
            }

            // Unlock and equip mask (triggers animation)
            maskManager.UnlockMaskEquipping();
            if (playerMaskController != null && playerMaskController.TimeMask != null)
                maskManager.EquipMask(playerMaskController.TimeMask);

            yield return new WaitForSeconds(pauseDuration);

            // Show mask info panel and wait for dismiss
            if (showMaskInfoPanel && UIManager.Instance != null)
            {
                UIManager.Instance.ShowMaskInfoPanel();

                // Wait until panel is dismissed
                while (UIManager.Instance.CurrentPanel == ActivePanel.MaskInfo)
                {
                    yield return null;
                }
            }

            // Unfreeze player
            if (player != null)
                player.enabled = true;

            OnPickedUp?.Invoke();
        }
    }
}
