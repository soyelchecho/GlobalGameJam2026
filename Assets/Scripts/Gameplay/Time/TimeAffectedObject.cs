using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Temporal
{
    public class TimeAffectedObject : MonoBehaviour
    {
        [Header("Time States")]
        [Tooltip("GameObject to show in Present time")]
        [SerializeField] private GameObject presentObject;

        [Tooltip("GameObject to show in Past time")]
        [SerializeField] private GameObject pastObject;

        [Header("Visual Effect")]
        [SerializeField] private bool applyPastTint = true;
        [SerializeField] private Color pastTint = new Color(0.7f, 0.85f, 1f, 1f);

        [Header("Animation Events")]
        [Tooltip("Called when entering Past (mask equipped). Use for rebuild animations.")]
        public UnityEvent OnEnterPast;

        [Tooltip("Called when entering Present (mask removed). Use for destroy animations.")]
        public UnityEvent OnEnterPresent;

        [Header("Timing")]
        [Tooltip("If true, objects switch immediately. If false, call ApplyPastState()/ApplyPresentState() from animation events.")]
        [SerializeField] private bool immediateSwitch = true;

        private Color[] originalColors;
        private SpriteRenderer[] pastSpriteRenderers;
        private TimeState pendingState;

        private void Awake()
        {
            if (pastObject != null)
            {
                pastSpriteRenderers = pastObject.GetComponentsInChildren<SpriteRenderer>(true);
                originalColors = new Color[pastSpriteRenderers.Length];

                for (int i = 0; i < pastSpriteRenderers.Length; i++)
                {
                    originalColors[i] = pastSpriteRenderers[i].color;
                }
            }
        }

        private void OnEnable()
        {
            TimeManager.Instance.OnTimeStateChanged += OnTimeStateChanged;
            UpdateVisualState(TimeManager.Instance.CurrentState);
        }

        private void OnDisable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeStateChanged -= OnTimeStateChanged;
            }
        }

        private void OnTimeStateChanged(TimeState newState)
        {
            pendingState = newState;

            // Invoke events for animations
            if (newState == TimeState.Past)
            {
                OnEnterPast?.Invoke();
            }
            else
            {
                OnEnterPresent?.Invoke();
            }

            // If immediate switch, apply now. Otherwise wait for manual call.
            if (immediateSwitch)
            {
                UpdateVisualState(newState);
            }
        }

        /// <summary>
        /// Call this from animation event to apply the Past state (show past object, hide present).
        /// </summary>
        public void ApplyPastState()
        {
            UpdateVisualState(TimeState.Past);
        }

        /// <summary>
        /// Call this from animation event to apply the Present state (show present object, hide past).
        /// </summary>
        public void ApplyPresentState()
        {
            UpdateVisualState(TimeState.Present);
        }

        /// <summary>
        /// Apply the pending state (whatever the last time change requested).
        /// </summary>
        public void ApplyPendingState()
        {
            UpdateVisualState(pendingState);
        }

        private void UpdateVisualState(TimeState state)
        {
            bool isPast = state == TimeState.Past;

            if (presentObject != null)
            {
                presentObject.SetActive(!isPast);
            }

            if (pastObject != null)
            {
                pastObject.SetActive(isPast);

                if (isPast && applyPastTint)
                {
                    ApplyPastTint();
                }
                else if (!isPast)
                {
                    RestoreOriginalColors();
                }
            }
        }

        private void ApplyPastTint()
        {
            if (pastSpriteRenderers == null) return;

            for (int i = 0; i < pastSpriteRenderers.Length; i++)
            {
                if (pastSpriteRenderers[i] != null)
                {
                    pastSpriteRenderers[i].color = originalColors[i] * pastTint;
                }
            }
        }

        private void RestoreOriginalColors()
        {
            if (pastSpriteRenderers == null || originalColors == null) return;

            for (int i = 0; i < pastSpriteRenderers.Length; i++)
            {
                if (pastSpriteRenderers[i] != null)
                {
                    pastSpriteRenderers[i].color = originalColors[i];
                }
            }
        }

        private void OnValidate()
        {
            if (presentObject == null)
            {
                Transform present = transform.Find("Present");
                if (present != null) presentObject = present.gameObject;
            }

            if (pastObject == null)
            {
                Transform past = transform.Find("Past");
                if (past != null) pastObject = past.gameObject;
            }
        }
    }
}
