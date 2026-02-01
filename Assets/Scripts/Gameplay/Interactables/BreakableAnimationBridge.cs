using UnityEngine;

namespace Gameplay.Interactables
{
    /// <summary>
    /// Bridge script to call parent BreakableObject methods from child Animation Events.
    /// Place this on the child that has the Animator.
    /// </summary>
    public class BreakableAnimationBridge : MonoBehaviour
    {
        [SerializeField] private BreakableObject breakableObject;

        private void Awake()
        {
            // Auto-find in parent if not assigned
            if (breakableObject == null)
            {
                breakableObject = GetComponentInParent<BreakableObject>();
            }
        }

        /// <summary>
        /// Call this from Animation Event to apply broken visual.
        /// </summary>
        public void ApplyBrokenVisual()
        {
            if (breakableObject != null)
            {
                breakableObject.ApplyBrokenVisual();
            }
        }

        /// <summary>
        /// Call this from Animation Event to trigger break.
        /// </summary>
        public void TriggerBreak()
        {
            if (breakableObject != null)
            {
                breakableObject.Break();
            }
        }
    }
}
