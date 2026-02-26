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

        [Header("Animation Events")]
        [Tooltip("Called when entering Past (mask equipped). Use for rebuild animations.")]
        public UnityEvent OnEnterPast;

        [Tooltip("Called when entering Present (mask removed). Use for destroy animations.")]
        public UnityEvent OnEnterPresent;

        [Header("Timing")]
        [Tooltip("Switch to Past instantly when mask is equipped. Disable if you want to trigger the show via ApplyPastState() from an animation event.")]
        [SerializeField] private bool immediateEnterSwitch = true;
        [Tooltip("Switch back to Present instantly when mask is removed. Disable to allow an exit animation on pastObject — call ApplyPresentState() from the animation event at the end of it.")]
        [SerializeField] private bool immediateExitSwitch = true;

        private TimeState pendingState;

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

            if (newState == TimeState.Past)
            {
                // Enter past: fire event, then switch immediately if configured
                OnEnterPast?.Invoke();
                if (immediateEnterSwitch)
                    UpdateVisualState(TimeState.Past);
            }
            else
            {
                // Enter present: show presentObject FIRST so there is no visual gap,
                // then fire the exit event while pastObject is still active so its
                // Animator can process the trigger and play the exit animation.
                if (presentObject != null)
                    presentObject.SetActive(true);

                OnEnterPresent?.Invoke();

                // If immediateExitSwitch, hide pastObject now (no exit animation).
                // If false, pastObject stays alive — call ApplyPresentState() from
                // the animation event at the END of the exit animation to hide it.
                if (immediateExitSwitch)
                {
                    if (pastObject != null)
                        pastObject.SetActive(false);
                }
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
                presentObject.SetActive(!isPast);

            if (pastObject != null)
                pastObject.SetActive(isPast);
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
