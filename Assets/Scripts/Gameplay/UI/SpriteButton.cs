using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI
{
    /// <summary>
    /// Button that uses sprites instead of text, with hover/press visual feedback.
    /// Assign normal, hover and pressed sprites in the inspector.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SpriteButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Sprites")]
        [Tooltip("Default sprite shown normally")]
        [SerializeField] private Sprite normalSprite;
        [Tooltip("Sprite shown on hover (mouse/touch over)")]
        [SerializeField] private Sprite hoverSprite;
        [Tooltip("Sprite shown when pressed down")]
        [SerializeField] private Sprite pressedSprite;

        [Header("Scale Animation")]
        [Tooltip("Scale multiplier on hover")]
        [SerializeField] private float hoverScale = 1.1f;
        [Tooltip("Scale multiplier on press")]
        [SerializeField] private float pressedScale = 0.95f;
        [Tooltip("Speed of scale animation")]
        [SerializeField] private float scaleSpeed = 10f;

        [Header("Events")]
        public UnityEvent OnClick;

        private Image image;
        private Vector3 originalScale;
        private Vector3 targetScale;
        private bool isHovered;
        private bool isPressed;

        private void Awake()
        {
            image = GetComponent<Image>();
            originalScale = transform.localScale;
            targetScale = originalScale;

            if (normalSprite != null)
                image.sprite = normalSprite;
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

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }

        private void UpdateVisual()
        {
            if (isPressed)
            {
                if (pressedSprite != null) image.sprite = pressedSprite;
                else if (hoverSprite != null) image.sprite = hoverSprite;
                targetScale = originalScale * pressedScale;
            }
            else if (isHovered)
            {
                if (hoverSprite != null) image.sprite = hoverSprite;
                targetScale = originalScale * hoverScale;
            }
            else
            {
                if (normalSprite != null) image.sprite = normalSprite;
                targetScale = originalScale;
            }
        }

        /// <summary>
        /// Change normal sprite at runtime.
        /// </summary>
        public void SetNormalSprite(Sprite sprite)
        {
            normalSprite = sprite;
            if (!isHovered && !isPressed)
                image.sprite = normalSprite;
        }
    }
}
