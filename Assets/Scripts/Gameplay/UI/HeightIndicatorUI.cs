using UnityEngine;

namespace Gameplay.UI
{
    /// <summary>
    /// Moves a mask sprite along a vertical track to show climbing progress.
    /// The sprite sits at the bottom at ground level and reaches the top at the cave/end trigger.
    /// </summary>
    public class HeightIndicatorUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The mask sprite RectTransform that moves along the track")]
        [SerializeField] private RectTransform indicatorImage;
        [Tooltip("The vertical bar/line RectTransform that defines the track range")]
        [SerializeField] private RectTransform trackRectTransform;

        [Header("World Bounds")]
        [Tooltip("World Y position of the ground / level start")]
        [SerializeField] private float groundY;
        [Tooltip("World Y position of the cave / end level trigger")]
        [SerializeField] private float targetY;

        private Transform player;

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            if (UIManager.Instance != null)
                UIManager.Instance.OnDeathShown.AddListener(Hide);
        }

        private void OnDestroy()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.OnDeathShown.RemoveListener(Hide);
        }

        private void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            if (player == null || indicatorImage == null || trackRectTransform == null) return;

            float progress = Mathf.Clamp01((player.position.y - groundY) / (targetY - groundY));

            float trackHeight = trackRectTransform.rect.height;
            Vector2 pos = indicatorImage.anchoredPosition;
            pos.y = progress * trackHeight;
            indicatorImage.anchoredPosition = pos;
        }
    }
}
