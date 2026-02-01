using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Interactables
{
    /// <summary>
    /// A breakable object that can be destroyed by the player.
    /// Place on the Breakable layer so player bounces off it like a wall.
    /// </summary>
    public class BreakableObject : MonoBehaviour, IBreakable
    {
        [Header("Visuals")]
        [SerializeField] private Sprite intactSprite;
        [SerializeField] private Sprite brokenSprite;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Collision")]
        [SerializeField] private Collider2D solidCollider;

        [Header("Break Requirements")]
        [Tooltip("If true, requires a mask to break")]
        [SerializeField] private bool requiresMask = true;

        [Tooltip("Specific mask ID required (empty = any mask)")]
        [SerializeField] private string requiredMaskId = "";

        [Header("Events")]
        public UnityEvent OnBroken;
        public UnityEvent OnBreakAttemptFailed;
        public UnityEvent OnRepaired;

        public bool IsBroken { get; private set; }

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (solidCollider == null)
                solidCollider = GetComponent<Collider2D>();

            if (intactSprite == null && spriteRenderer != null)
                intactSprite = spriteRenderer.sprite;
        }

        public bool CanBreak(string maskId)
        {
            if (IsBroken) return false;

            if (!requiresMask) return true;

            if (string.IsNullOrEmpty(maskId)) return false;

            // If specific mask required, check it matches
            if (!string.IsNullOrEmpty(requiredMaskId))
                return maskId == requiredMaskId;

            // Any mask is fine
            return true;
        }

        public void Break()
        {
            if (IsBroken) return;

            IsBroken = true;
            Debug.Log($"[BreakableObject] {name} - Break() called");

            // Disable collision so player can pass through
            if (solidCollider != null)
                solidCollider.enabled = false;

            // Invoke event FIRST (for animations, VFX, etc.)
            int listenerCount = OnBroken?.GetPersistentEventCount() ?? 0;
            Debug.Log($"[BreakableObject] {name} - OnBroken has {listenerCount} listeners, invoking...");
            OnBroken?.Invoke();
            Debug.Log($"[BreakableObject] {name} - OnBroken invoked");
        }

        /// <summary>
        /// Apply the broken visual state. Call this after your break animation finishes.
        /// Can also be called directly from OnBroken if no animation is needed.
        /// </summary>
        public void ApplyBrokenVisual()
        {
            if (spriteRenderer != null && brokenSprite != null)
                spriteRenderer.sprite = brokenSprite;
        }

        /// <summary>
        /// Try to break this object with the given mask.
        /// </summary>
        public void TryBreak(string maskId)
        {
            if (CanBreak(maskId))
            {
                Break();
            }
            else
            {
                OnBreakAttemptFailed?.Invoke();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            CacheComponents();
        }
#endif
    }
}
