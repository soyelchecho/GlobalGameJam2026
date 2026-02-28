using System.Collections;
using UnityEngine;
using Gameplay.Masks;

namespace Gameplay.Tutorial
{
    public enum TriggerMode { OnSceneStart, OnMaskUnlocked }
    public enum DismissMode { OnTap, OnSwipe }

    /// <summary>
    /// Place in scene to configure when and how a tutorial hint appears.
    /// Handles both OnSceneStart (with delay) and OnMaskUnlocked trigger modes.
    /// </summary>
    public class TutorialHintTrigger : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Unique key stored in PlayerPrefs to remember if hint was shown")]
        [SerializeField] private string hintId;

        [Header("Trigger")]
        [SerializeField] private TriggerMode triggerMode = TriggerMode.OnSceneStart;
        [Tooltip("Seconds to wait before showing (OnSceneStart only)")]
        [SerializeField] private float delay = 2f;
        [Tooltip("Required for OnMaskUnlocked trigger mode")]
        [SerializeField] private MaskManager maskManager;

        [Header("Content")]
        [Tooltip("AnimatorController con la animación del gesto (tap, swipe, etc.)")]
        [SerializeField] private RuntimeAnimatorController animatorController;
        [Tooltip("Nombre del estado en el Animator a reproducir")]
        [SerializeField] private string stateName = "Play";
        [SerializeField] [TextArea] private string hintText;

        [Header("Position")]
        [Tooltip("Offset from player position in world space")]
        [SerializeField] private Vector3 playerOffset = new Vector3(0f, 0f, 0f);

        [Header("Dismiss")]
        [SerializeField] private DismissMode dismissMode = DismissMode.OnTap;

        private TouchInputHandler inputHandler;
        private bool triggered;

        private void Start()
        {
            switch (triggerMode)
            {
                case TriggerMode.OnSceneStart:
                    StartCoroutine(WaitAndShow(delay));
                    break;

                case TriggerMode.OnMaskUnlocked:
                    if (maskManager != null)
                        maskManager.OnMaskUnlocked.AddListener(Show);
                    else
                        Debug.LogWarning("[TutorialHintTrigger] MaskManager not assigned for OnMaskUnlocked trigger.", this);
                    break;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromInput();

            if (maskManager != null)
                maskManager.OnMaskUnlocked.RemoveListener(Show);
        }

        private IEnumerator WaitAndShow(float waitSeconds)
        {
            yield return new WaitForSecondsRealtime(waitSeconds);
            Show();
        }

        private void Show()
        {
            if (triggered) return;

            if (TutorialHintManager.Instance == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[TutorialHintTrigger] No GameObject with tag 'Player' found.", this);
                return;
            }

            bool shown = TutorialHintManager.Instance.ShowHint(hintId, animatorController, stateName, hintText, player.transform, playerOffset);

            if (!shown)
            {
                // Already seen — nothing to subscribe to
                return;
            }

            triggered = true;

            inputHandler = player.GetComponent<TouchInputHandler>();
            if (inputHandler == null)
            {
                Debug.LogWarning("[TutorialHintTrigger] TouchInputHandler not found on Player.", this);
                return;
            }

            SubscribeToInput();
        }

        private void SubscribeToInput()
        {
            switch (dismissMode)
            {
                case DismissMode.OnTap:
                    inputHandler.OnTap.AddListener(OnTapDismiss);
                    break;

                case DismissMode.OnSwipe:
                    inputHandler.OnSwipeUp.AddListener(OnDismissAction);
                    inputHandler.OnSwipeDown.AddListener(OnDismissAction);
                    break;
            }
        }

        private void UnsubscribeFromInput()
        {
            if (inputHandler == null) return;

            inputHandler.OnTap.RemoveListener(OnTapDismiss);
            inputHandler.OnSwipeUp.RemoveListener(OnDismissAction);
            inputHandler.OnSwipeDown.RemoveListener(OnDismissAction);
        }

        private void OnTapDismiss(Vector2 _)
        {
            OnDismissAction();
        }

        private void OnDismissAction()
        {
            if (TutorialHintManager.Instance == null || !TutorialHintManager.Instance.IsShowing) return;

            TutorialHintManager.Instance.DismissHint();
            UnsubscribeFromInput();
            enabled = false;
        }
    }
}
