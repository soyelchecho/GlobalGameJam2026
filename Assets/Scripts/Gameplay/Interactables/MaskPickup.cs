using UnityEngine;
using UnityEngine.Events;
using Gameplay.Masks;

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
            maskManager.UnlockMaskEquipping();

            if (objectToHide != null)
            {
                if (destroyInsteadOfHide)
                    Destroy(objectToHide);
                else
                    objectToHide.SetActive(false);
            }

            OnPickedUp?.Invoke();
        }
    }
}
