using UnityEngine;
using System;

namespace Gameplay.Parallax
{
    /// <summary>
    /// Manages multiple parallax layers from a single component.
    /// Alternative to adding ParallaxLayer to each sprite individually.
    /// </summary>
    public class ParallaxManager : MonoBehaviour
    {
        [Serializable]
        public class ParallaxLayerData
        {
            public string name;
            public Transform layerTransform;

            [Range(0f, 1f)]
            [Tooltip("0 = fixed, 0.5 = half speed, 1 = follows camera")]
            public float parallaxFactor = 0.5f;

            [Tooltip("Enable for repeating backgrounds")]
            public bool infiniteScroll = false;

            [HideInInspector]
            public Vector3 startPosition;
        }

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;

        [Header("Layers (Back to Front)")]
        [SerializeField] private ParallaxLayerData[] layers;

        private Vector3 cameraStartPosition;

        private void Start()
        {
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }

            if (cameraTransform != null)
            {
                cameraStartPosition = cameraTransform.position;
            }

            // Store starting positions
            foreach (var layer in layers)
            {
                if (layer.layerTransform != null)
                {
                    layer.startPosition = layer.layerTransform.position;
                }
            }
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            Vector3 cameraDelta = cameraTransform.position - cameraStartPosition;

            foreach (var layer in layers)
            {
                if (layer.layerTransform == null) continue;

                // Only apply Y parallax for vertical runner
                float newY = layer.startPosition.y + (cameraDelta.y * layer.parallaxFactor);

                layer.layerTransform.position = new Vector3(
                    layer.startPosition.x,
                    newY,
                    layer.layerTransform.position.z
                );

                // Infinite scroll handling
                if (layer.infiniteScroll)
                {
                    HandleInfiniteScroll(layer);
                }
            }
        }

        private void HandleInfiniteScroll(ParallaxLayerData layer)
        {
            SpriteRenderer sr = layer.layerTransform.GetComponent<SpriteRenderer>();
            if (sr == null) return;

            float spriteHeight = sr.bounds.size.y;
            if (spriteHeight <= 0) return;

            float cameraY = cameraTransform.position.y;
            float layerY = layer.layerTransform.position.y;
            float relativeY = layerY - cameraY;

            if (relativeY < -spriteHeight * 0.5f)
            {
                layer.layerTransform.position += Vector3.up * spriteHeight;
                layer.startPosition += Vector3.up * spriteHeight;
            }
            else if (relativeY > spriteHeight * 0.5f)
            {
                layer.layerTransform.position -= Vector3.up * spriteHeight;
                layer.startPosition -= Vector3.up * spriteHeight;
            }
        }
    }
}
