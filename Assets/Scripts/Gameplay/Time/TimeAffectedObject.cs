using UnityEngine;

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

        private Color[] originalColors;
        private SpriteRenderer[] pastSpriteRenderers;

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
            UpdateVisualState(newState);
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
