using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// Invisible hotspot button with hover/press feedback.
    /// Place over a map location. On hover/press, shows a highlight or scales up.
    /// Works with touch (mobile) and mouse.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LevelHotspotButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("Hover Feedback")]
        [Tooltip("Color tint when hovering (applied to the Image)")]
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.3f);
        [Tooltip("Color tint when pressed")]
        [SerializeField] private Color pressedColor = new Color(1f, 1f, 1f, 0.5f);
        [Tooltip("Normal color (transparent = invisible)")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0f);

        [Header("Scale Feedback")]
        [Tooltip("Scale multiplier on hover")]
        [SerializeField] private float hoverScale = 1.1f;
        [Tooltip("Scale multiplier on press")]
        [SerializeField] private float pressedScale = 0.95f;
        [Tooltip("Speed of scale animation")]
        [SerializeField] private float scaleSpeed = 10f;

        [Header("Optional Highlight")]
        [Tooltip("Optional highlight GameObject to show on hover")]
        [SerializeField] private GameObject highlightObject;

        private Image image;
        private Vector3 originalScale;
        private Vector3 targetScale;
        private bool isHovered;
        private bool isPressed;

        private void Awake()
        {
            image = GetComponent<Image>();
            image.color = normalColor;
            originalScale = transform.localScale;
            targetScale = originalScale;

            if (highlightObject != null)
                highlightObject.SetActive(false);
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateVisual();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            isPressed = false;
            UpdateVisual();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            UpdateVisual();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (isPressed)
            {
                image.color = pressedColor;
                targetScale = originalScale * pressedScale;
            }
            else if (isHovered)
            {
                image.color = hoverColor;
                targetScale = originalScale * hoverScale;
            }
            else
            {
                image.color = normalColor;
                targetScale = originalScale;
            }

            if (highlightObject != null)
                highlightObject.SetActive(isHovered || isPressed);
        }
    }
}
