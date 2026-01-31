using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Interactables
{
    public class BreakableObject : MonoBehaviour, IBreakable
    {
        [Header("Sprites")]
        [SerializeField] private Sprite intactSprite;
        [SerializeField] private Sprite brokenSprite;

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D objectCollider;

        [Header("Mask Requirements")]
        [Tooltip("If true, requires a mask to break. If false, can be broken anytime.")]
        [SerializeField] private bool requiresMask = true;

        [Tooltip("Specific mask ID required. Leave empty to allow any mask.")]
        [SerializeField] private string requiredMaskId = "";

        [Header("Events")]
        public UnityEvent OnBroken;
        public UnityEvent OnBreakAttemptFailed;

        public bool IsBroken { get; private set; }

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (objectCollider == null)
                objectCollider = GetComponent<Collider2D>();

            if (intactSprite == null && spriteRenderer != null)
                intactSprite = spriteRenderer.sprite;
        }

        public bool CanBreak(string maskId)
        {
            if (IsBroken) return false;

            if (!requiresMask) return true;

            // Requires mask but none provided
            if (string.IsNullOrEmpty(maskId)) return false;

            // Specific mask required
            if (!string.IsNullOrEmpty(requiredMaskId))
            {
                return maskId == requiredMaskId;
            }

            // Any mask is fine
            return true;
        }

        public void Break()
        {
            if (IsBroken) return;

            IsBroken = true;

            // Change sprite
            if (spriteRenderer != null && brokenSprite != null)
            {
                spriteRenderer.sprite = brokenSprite;
            }

            // Disable collider
            if (objectCollider != null)
            {
                objectCollider.enabled = false;
            }

            OnBroken?.Invoke();

            Debug.Log($"[Breakable] {gameObject.name} was broken!");
        }

        public void TryBreak(string maskId)
        {
            if (CanBreak(maskId))
            {
                Break();
            }
            else
            {
                OnBreakAttemptFailed?.Invoke();
                Debug.Log($"[Breakable] Cannot break {gameObject.name} - mask required: {requiredMaskId}, provided: {maskId}");
            }
        }

        /// <summary>
        /// Restore the object to its intact state
        /// </summary>
        public void Repair()
        {
            if (!IsBroken) return;

            IsBroken = false;

            if (spriteRenderer != null && intactSprite != null)
            {
                spriteRenderer.sprite = intactSprite;
            }

            if (objectCollider != null)
            {
                objectCollider.enabled = true;
            }

            Debug.Log($"[Breakable] {gameObject.name} was repaired!");
        }

        private void OnValidate()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (objectCollider == null)
                objectCollider = GetComponent<Collider2D>();
        }
    }
}
