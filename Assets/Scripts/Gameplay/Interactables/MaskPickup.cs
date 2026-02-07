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

        [Header("Events")]
        public UnityEvent OnPickedUp;

        private bool hasBeenPickedUp;

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("[MaskPickup] Player touched pickup collider");
            if (hasBeenPickedUp) return;
            if (!other.CompareTag(playerTag)) return;

            var maskManager = other.GetComponent<MaskManager>();
            if (maskManager == null) return;

            hasBeenPickedUp = true;
            Debug.Log("[MaskPickup] Player touched pickup collider");
            maskManager.UnlockMaskEquipping();
            OnPickedUp?.Invoke();
        }
    }
}
