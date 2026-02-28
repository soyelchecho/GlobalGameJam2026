using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Gameplay.Masks;
using Gameplay.Player;
using Gameplay.UI;

namespace Gameplay.Interactables
{
    /// <summary>
    /// Trigger that unlocks mask equipping when the player enters.
    /// </summary>
    public class MaskPickup : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private string playerTag = "Player";

        [Header("Mask Override")]
        [Tooltip("If set, equips this mask instead of the player's default TimeMask and updates swipe-down binding.")]
        [SerializeField] private MaskBase maskToEquip;

        [Header("Visuals")]
        [Tooltip("Object to hide/destroy when picked up (optional)")]
        [SerializeField] private GameObject objectToHide;
        [SerializeField] private bool destroyInsteadOfHide;

        [Header("Pickup Animation")]
        [Tooltip("Pause player and play equip animation on pickup")]
        [SerializeField] private bool equipOnPickup;
        [SerializeField] private float pauseDuration = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip pickupSound;

        [Header("UI")]
        [Tooltip("Show mask info panel after pickup")]
        [SerializeField] private bool showMaskInfoPanel;
        [Tooltip("Sprite to display in the mask info panel image when this mask is picked up")]
        [SerializeField] private Sprite maskInfoSprite;

        [Header("Events")]
        public UnityEvent OnPickedUp;

        private bool hasBeenPickedUp;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasBeenPickedUp) return;
            if (!other.CompareTag(playerTag)) return;

            var maskManager = other.GetComponent<MaskManager>();
            if (maskManager == null) return;

            hasBeenPickedUp = true;

            if (pickupSound != null)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0f;
                audioSource.PlayOneShot(pickupSound, Gameplay.Audio.VolumeManager.GetSFXVolume());
            }

            if (objectToHide != null)
            {
                // Copy sprite from pickup visual onto the player's mask renderer
                var pickupSprite = objectToHide.GetComponent<SpriteRenderer>();
                if (pickupSprite != null)
                {
                    Sprite maskSprite = pickupSprite.sprite;

                    // Update player mask renderer
                    Transform playerMaskRenderer = other.transform.Find("maskRendered");
                    if (playerMaskRenderer != null)
                    {
                        SpriteRenderer playerSpriteRenderer = playerMaskRenderer.GetComponent<SpriteRenderer>();
                        if (playerSpriteRenderer != null)
                            playerSpriteRenderer.sprite = maskSprite;
                    }

                    GameObject uiRoot = GameObject.Find("UI");
                    if (uiRoot != null)
                    {
                        // Update mask info panel image
                        if (maskInfoSprite != null)
                        {
                            Transform panelImage = uiRoot.transform.Find("TimeMaskPanel/Image");
                            if (panelImage != null)
                            {
                                Image image = panelImage.GetComponent<Image>();
                                if (image != null)
                                    image.sprite = maskInfoSprite;
                            }
                        }

                        // Update height indicator icon
                        Transform rawImageTransform = uiRoot.transform.Find("HeightIndicator/RawImage");
                        if (rawImageTransform != null)
                        {
                            RawImage rawImage = rawImageTransform.GetComponent<RawImage>();
                            if (rawImage != null)
                                rawImage.texture = maskSprite.texture;
                        }
                    }
                }

                if (destroyInsteadOfHide)
                    Destroy(objectToHide);
                else
                    objectToHide.SetActive(false);
            }

            if (equipOnPickup)
            {
                var playerController = other.GetComponent<PlayerController>();
                var playerMaskController = other.GetComponent<PlayerMaskController>();
                StartCoroutine(PickupSequence(playerController, maskManager, playerMaskController));
            }
            else
            {
                maskManager.UnlockMaskEquipping();

                if (showMaskInfoPanel && UIManager.Instance != null)
                    UIManager.Instance.ShowMaskInfoPanel();

                OnPickedUp?.Invoke();
            }
        }

        private IEnumerator PickupSequence(PlayerController player, MaskManager maskManager, PlayerMaskController playerMaskController)
        {
            // Freeze player
            if (player != null)
            {
                player.enabled = false;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.velocity = Vector2.zero;
            }

            // Unlock and equip mask (triggers animation)
            maskManager.UnlockMaskEquipping();
            MaskBase maskToUse = maskToEquip != null ? maskToEquip : playerMaskController?.TimeMask;
            if (maskToUse != null)
            {
                if (maskToEquip != null)
                    maskManager.CancelCooldown();
                maskManager.EquipMask(maskToUse);
                playerMaskController?.SetActiveMask(maskToUse);
            }

            // Pause timer while showing info panel
            maskManager.PauseTimer();

            yield return new WaitForSeconds(pauseDuration);

            // Show mask info panel and wait for dismiss
            if (showMaskInfoPanel && UIManager.Instance != null)
            {
                UIManager.Instance.ShowMaskInfoPanel();

                // Wait until panel is dismissed
                while (UIManager.Instance.CurrentPanel == ActivePanel.MaskInfo)
                {
                    yield return null;
                }
            }

            // Resume timer after panel dismissed
            maskManager.ResumeTimer();

            // Unfreeze player
            if (player != null)
                player.enabled = true;

            OnPickedUp?.Invoke();
        }
    }
}
