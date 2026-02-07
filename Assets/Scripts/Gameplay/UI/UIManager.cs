using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Gameplay.Audio;

namespace Gameplay.UI
{
    public enum ActivePanel
    {
        None,
        MaskInfo,
        Death,
        NextLevel,
        LavaWarning
    }

    /// <summary>
    /// Manages UI panels: mask info, death, next level, lava warning.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject maskInfoPanel;
        [SerializeField] private GameObject deathPanel;
        [SerializeField] private GameObject nextLevelPanel;
        [SerializeField] private GameObject lavaWarningPanel;

        [Header("Lava Warning")]
        [SerializeField] private float lavaWarningDuration = 2f;

        [Header("Events")]
        public UnityEvent OnMaskInfoShown;
        public UnityEvent OnMaskInfoDismissed;
        public UnityEvent OnDeathShown;
        public UnityEvent OnDeathDismissed;
        public UnityEvent OnNextLevelShown;
        public UnityEvent OnNextLevelDismissed;
        public UnityEvent OnLavaWarningShown;
        public UnityEvent OnLavaWarningHidden;

        private ActivePanel currentPanel = ActivePanel.None;
        private Coroutine lavaWarningCoroutine;

        public ActivePanel CurrentPanel => currentPanel;
        public bool IsShowingPanel => currentPanel != ActivePanel.None;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            HideAllPanels();
        }

        private void Update()
        {
            if (currentPanel == ActivePanel.MaskInfo || currentPanel == ActivePanel.Death)
            {
                if (DetectTapOrSwipe())
                {
                    DismissCurrentPanel();
                }
            }
        }

        private bool DetectTapOrSwipe()
        {
            // Touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                {
                    return true;
                }
            }

            // Mouse input (for editor)
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }

            // Keyboard (for testing)
            if (Input.anyKeyDown)
            {
                return true;
            }

            return false;
        }

        private void HideAllPanels()
        {
            if (maskInfoPanel != null) maskInfoPanel.SetActive(false);
            if (deathPanel != null) deathPanel.SetActive(false);
            if (nextLevelPanel != null) nextLevelPanel.SetActive(false);
            if (lavaWarningPanel != null) lavaWarningPanel.SetActive(false);
        }

        #region Mask Info Panel

        public void ShowMaskInfoPanel()
        {
            if (maskInfoPanel == null) return;

            HideAllPanels();
            currentPanel = ActivePanel.MaskInfo;
            maskInfoPanel.SetActive(true);
            OnMaskInfoShown?.Invoke();
        }

        public void HideMaskInfoPanel()
        {
            if (maskInfoPanel == null) return;
            if (currentPanel != ActivePanel.MaskInfo) return;

            currentPanel = ActivePanel.None;
            maskInfoPanel.SetActive(false);
            OnMaskInfoDismissed?.Invoke();
        }

        #endregion

        #region Death Panel

        public void ShowDeathPanel()
        {
            if (deathPanel == null) return;

            HideAllPanels();
            currentPanel = ActivePanel.Death;
            deathPanel.SetActive(true);

            // Duck music to 20%
            if (AudioManager.Instance != null)
                AudioManager.Instance.DuckMusic(0.2f);

            OnDeathShown?.Invoke();
        }

        public void HideDeathPanel()
        {
            if (deathPanel == null) return;
            if (currentPanel != ActivePanel.Death) return;

            currentPanel = ActivePanel.None;
            deathPanel.SetActive(false);

            // Restore music volume
            if (AudioManager.Instance != null)
                AudioManager.Instance.RestoreMusicVolume();

            OnDeathDismissed?.Invoke();
        }

        #endregion

        #region Next Level Panel

        public void ShowNextLevelPanel()
        {
            if (nextLevelPanel == null) return;

            HideAllPanels();
            currentPanel = ActivePanel.NextLevel;
            nextLevelPanel.SetActive(true);
            OnNextLevelShown?.Invoke();
        }

        public void HideNextLevelPanel()
        {
            if (nextLevelPanel == null) return;
            if (currentPanel != ActivePanel.NextLevel) return;

            currentPanel = ActivePanel.None;
            nextLevelPanel.SetActive(false);
            OnNextLevelDismissed?.Invoke();
        }

        #endregion

        #region Lava Warning

        public void ShowLavaWarning()
        {
            if (lavaWarningPanel == null) return;

            lavaWarningPanel.SetActive(true);
            OnLavaWarningShown?.Invoke();

            if (lavaWarningCoroutine != null)
                StopCoroutine(lavaWarningCoroutine);
            lavaWarningCoroutine = StartCoroutine(AutoHideLavaWarning());
        }

        public void HideLavaWarning()
        {
            if (lavaWarningPanel == null) return;

            lavaWarningPanel.SetActive(false);
            OnLavaWarningHidden?.Invoke();

            if (lavaWarningCoroutine != null)
            {
                StopCoroutine(lavaWarningCoroutine);
                lavaWarningCoroutine = null;
            }
        }

        private IEnumerator AutoHideLavaWarning()
        {
            yield return new WaitForSeconds(lavaWarningDuration);
            HideLavaWarning();
        }

        #endregion

        private void DismissCurrentPanel()
        {
            switch (currentPanel)
            {
                case ActivePanel.MaskInfo:
                    HideMaskInfoPanel();
                    break;
                case ActivePanel.Death:
                    HideDeathPanel();
                    break;
            }
        }
    }
}
