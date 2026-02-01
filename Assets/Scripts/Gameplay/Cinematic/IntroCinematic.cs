using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

namespace Gameplay.Cinematic
{
    /// <summary>
    /// Controls the intro cinematic sequence.
    /// Plays animation -> Shows panel -> Click to start game.
    /// </summary>
    public class IntroCinematic : MonoBehaviour
    {
        [Header("Intro Animation")]
        [Tooltip("Animator for the intro cinematic")]
        [SerializeField] private Animator introAnimator;

        [Tooltip("Animation state name to play")]
        [SerializeField] private string introAnimationName = "IntroAnimation";

        [Tooltip("Wait for animation to finish before showing panel")]
        [SerializeField] private bool waitForAnimation = true;

        [Tooltip("If not waiting for animation, delay before showing panel")]
        [SerializeField] private float delayBeforePanel = 0f;

        [Header("Start Panel")]
        [Tooltip("Panel to show after intro animation")]
        [SerializeField] private GameObject startPanel;

        [Tooltip("Button/Image to click to start the game")]
        [SerializeField] private Button startButton;

        [Tooltip("If no Button component, use this Image for click detection")]
        [SerializeField] private Image clickableImage;

        [Header("Game Start")]
        [Tooltip("Player GameObject to activate when game starts")]
        [SerializeField] private GameObject playerObject;

        [Tooltip("Objects to activate when game starts")]
        [SerializeField] private GameObject[] objectsToActivate;

        [Tooltip("Objects to deactivate when game starts")]
        [SerializeField] private GameObject[] objectsToDeactivate;

        [Header("Events")]
        [Tooltip("Called when cinematic starts")]
        public UnityEvent OnCinematicStart;

        [Tooltip("Called when intro animation starts")]
        public UnityEvent OnAnimationStart;

        [Tooltip("Called when intro animation ends")]
        public UnityEvent OnAnimationEnd;

        [Tooltip("Called when start panel is shown")]
        public UnityEvent OnPanelShown;

        [Tooltip("Called when player clicks to start")]
        public UnityEvent OnStartClicked;

        [Tooltip("Called when game officially starts (player activated)")]
        public UnityEvent OnGameStart;

        [Header("Settings")]
        [Tooltip("Auto-start cinematic on Awake")]
        [SerializeField] private bool autoStart = true;

        [Tooltip("Hide panel at start")]
        [SerializeField] private bool hidePanelOnStart = true;

        [Tooltip("Delay after click before activating player")]
        [SerializeField] private float delayBeforeGameStart = 0f;

        private bool hasStarted = false;
        private bool isWaitingForClick = false;

        private void Awake()
        {
            // Hide panel initially
            if (hidePanelOnStart && startPanel != null)
            {
                startPanel.SetActive(false);
            }

            // Setup button click
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }

            // Setup image click if no button
            if (startButton == null && clickableImage != null)
            {
                // Add EventTrigger for click
                var trigger = clickableImage.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
                entry.callback.AddListener((data) => OnStartButtonClicked());
                trigger.triggers.Add(entry);
            }
        }

        private void Start()
        {
            if (autoStart)
            {
                StartCinematic();
            }
        }

        /// <summary>
        /// Start the cinematic sequence. Call this if autoStart is false.
        /// </summary>
        public void StartCinematic()
        {
            if (hasStarted) return;
            hasStarted = true;

            OnCinematicStart?.Invoke();
            StartCoroutine(CinematicSequence());
        }

        private IEnumerator CinematicSequence()
        {
            // Play intro animation
            if (introAnimator != null && !string.IsNullOrEmpty(introAnimationName))
            {
                OnAnimationStart?.Invoke();
                introAnimator.Play(introAnimationName);

                if (waitForAnimation)
                {
                    // Wait for animation to start
                    yield return null;

                    // Wait for animation to finish
                    AnimatorStateInfo stateInfo = introAnimator.GetCurrentAnimatorStateInfo(0);
                    while (introAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                    {
                        yield return null;
                    }
                }
                else if (delayBeforePanel > 0)
                {
                    yield return new WaitForSeconds(delayBeforePanel);
                }

                OnAnimationEnd?.Invoke();
            }
            else if (delayBeforePanel > 0)
            {
                yield return new WaitForSeconds(delayBeforePanel);
            }

            // Show start panel
            ShowStartPanel();
        }

        /// <summary>
        /// Show the start panel. Can be called from animation event.
        /// </summary>
        public void ShowStartPanel()
        {
            if (startPanel != null)
            {
                startPanel.SetActive(true);
            }

            isWaitingForClick = true;
            OnPanelShown?.Invoke();
        }

        /// <summary>
        /// Hide the start panel.
        /// </summary>
        public void HideStartPanel()
        {
            if (startPanel != null)
            {
                startPanel.SetActive(false);
            }
        }

        private void OnStartButtonClicked()
        {
            if (!isWaitingForClick) return;
            isWaitingForClick = false;

            OnStartClicked?.Invoke();

            if (delayBeforeGameStart > 0)
            {
                StartCoroutine(DelayedGameStart());
            }
            else
            {
                StartGame();
            }
        }

        private IEnumerator DelayedGameStart()
        {
            yield return new WaitForSeconds(delayBeforeGameStart);
            StartGame();
        }

        /// <summary>
        /// Start the game. Activates player and configured objects.
        /// </summary>
        public void StartGame()
        {
            // Hide panel
            HideStartPanel();

            // Activate player
            if (playerObject != null)
            {
                playerObject.SetActive(true);
            }

            // Activate objects
            foreach (var obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            // Deactivate objects
            foreach (var obj in objectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            OnGameStart?.Invoke();

            Debug.Log("[IntroCinematic] Game started!");
        }

        /// <summary>
        /// Skip the cinematic and go directly to start panel.
        /// </summary>
        public void SkipToPanel()
        {
            StopAllCoroutines();

            if (introAnimator != null)
            {
                introAnimator.enabled = false;
            }

            OnAnimationEnd?.Invoke();
            ShowStartPanel();
        }

        /// <summary>
        /// Skip everything and start the game immediately.
        /// </summary>
        public void SkipAll()
        {
            StopAllCoroutines();
            hasStarted = true;
            isWaitingForClick = false;
            StartGame();
        }

        private void OnDestroy()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        private void OnGUI()
        {
            if (!debugMode || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 150));
            GUILayout.Label("<b>Intro Cinematic Debug</b>");
            GUILayout.Label($"Started: {hasStarted}");
            GUILayout.Label($"Waiting for click: {isWaitingForClick}");

            if (GUILayout.Button("Skip to Panel"))
            {
                SkipToPanel();
            }

            if (GUILayout.Button("Skip All (Start Game)"))
            {
                SkipAll();
            }

            GUILayout.EndArea();
        }
#endif
    }
}
