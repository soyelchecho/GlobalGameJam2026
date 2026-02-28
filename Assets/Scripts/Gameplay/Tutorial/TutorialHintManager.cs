using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Tutorial
{
    /// <summary>
    /// Singleton que gestiona el panel de tutorial.
    /// Crear en escena con Tools / UI / Create Tutorial Hint Manager.
    /// </summary>
    public class TutorialHintManager : MonoBehaviour
    {
        public static TutorialHintManager Instance { get; private set; }

        [Header("References (auto-assigned by menu item)")]
        [SerializeField] private RectTransform hintPanel;
        [SerializeField] private Animator hintAnimator;
        [SerializeField] private Text hintText;

        private Transform trackedTransform;
        private Vector3 trackedOffset;

        public bool IsShowing { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (hintAnimator != null)
                hintAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            if (hintPanel != null)
                hintPanel.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LateUpdate()
        {
            if (!IsShowing || trackedTransform == null) return;

            Vector3 worldPos = trackedTransform.position + trackedOffset;
            hintPanel.position = Camera.main.WorldToScreenPoint(worldPos);
        }

        /// <summary>
        /// Muestra el hint si no se ha visto antes (PlayerPrefs por hintId).
        /// Asigna el AnimatorController y reproduce el estado indicado.
        /// Retorna true si se mostró, false si ya fue visto o panel no asignado.
        /// </summary>
        public bool ShowHint(string hintId, RuntimeAnimatorController controller, string stateName,
                             string text, Transform trackTarget, Vector3 offset)
        {
            if (hintPanel == null) return false;
            if (PlayerPrefs.GetInt("TutorialHint_" + hintId, 0) == 1) return false;

            PlayerPrefs.SetInt("TutorialHint_" + hintId, 1);
            PlayerPrefs.Save();

            trackedTransform = trackTarget;
            trackedOffset = offset;

            if (hintText != null)
                hintText.text = text;

            if (hintAnimator != null && controller != null)
            {
                hintAnimator.runtimeAnimatorController = controller;
                hintAnimator.Play(stateName);
            }

            hintPanel.gameObject.SetActive(true);
            IsShowing = true;
            Time.timeScale = 0f;

            return true;
        }

        public void DismissHint()
        {
            if (!IsShowing) return;

            hintPanel.gameObject.SetActive(false);

            if (hintAnimator != null)
                hintAnimator.runtimeAnimatorController = null;

            IsShowing = false;
            trackedTransform = null;
            Time.timeScale = 1f;
        }
    }
}
