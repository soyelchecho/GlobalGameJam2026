using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Parallax
{
    /// <summary>
    /// Infinite scrolling parallax layer that automatically creates and manages tile copies.
    /// Ensures seamless vertical scrolling without gaps.
    /// </summary>
    public class InfiniteParallaxLayer : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Tooltip("0 = fixed, 0.5 = half speed, 1 = follows camera exactly")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxFactorY = 0.5f;

        [Header("Tile Settings")]
        [Tooltip("The sprite to tile. If empty, uses this object's SpriteRenderer.")]
        [SerializeField] private Sprite tileSprite;

        [Tooltip("Height of one tile. Auto-detects from sprite if 0.")]
        [SerializeField] private float tileHeight = 0f;

        [Tooltip("How many extra tiles above and below to ensure coverage.")]
        [SerializeField] private int bufferTiles = 2;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private Camera cam;
        private Vector3 cameraStartPosition;
        private float startY;
        private float calculatedHeight;
        private List<Transform> tiles = new List<Transform>();
        private SpriteRenderer templateRenderer;

        private void Awake()
        {
            templateRenderer = GetComponent<SpriteRenderer>();

            // Calculate tile height
            if (tileHeight <= 0)
            {
                if (tileSprite != null)
                {
                    calculatedHeight = tileSprite.bounds.size.y;
                }
                else if (templateRenderer != null && templateRenderer.sprite != null)
                {
                    calculatedHeight = templateRenderer.bounds.size.y;
                    tileSprite = templateRenderer.sprite;
                }
                else
                {
                    calculatedHeight = 20f;
                    Debug.LogWarning($"[InfiniteParallaxLayer] Could not detect tile height on {name}, using default 20");
                }
            }
            else
            {
                calculatedHeight = tileHeight;
            }
        }

        private void Start()
        {
            if (cameraTransform == null)
            {
                cam = Camera.main;
                cameraTransform = cam?.transform;
            }
            else
            {
                cam = cameraTransform.GetComponent<Camera>();
            }

            if (cameraTransform != null)
            {
                cameraStartPosition = cameraTransform.position;
            }

            startY = transform.position.y;

            CreateTiles();
        }

        private void CreateTiles()
        {
            if (cam == null || calculatedHeight <= 0) return;

            float viewHeight = cam.orthographicSize * 2f;

            // Calculate how many tiles we need to cover the view + buffer
            int tilesNeeded = Mathf.CeilToInt(viewHeight / calculatedHeight) + (bufferTiles * 2) + 1;

            // Clear existing tiles (except first one if it's this object)
            foreach (var tile in tiles)
            {
                if (tile != null && tile != transform)
                {
                    Destroy(tile.gameObject);
                }
            }
            tiles.Clear();

            // If this object has a SpriteRenderer, use it as the first tile
            if (templateRenderer != null)
            {
                tiles.Add(transform);
            }

            // Create additional tiles as children
            int startIndex = tiles.Count;
            for (int i = startIndex; i < tilesNeeded; i++)
            {
                GameObject tileObj = new GameObject($"{name}_Tile_{i}");
                tileObj.transform.SetParent(transform.parent);
                tileObj.transform.localScale = transform.localScale;

                SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sprite = tileSprite;
                sr.sortingLayerID = templateRenderer != null ? templateRenderer.sortingLayerID : 0;
                sr.sortingOrder = templateRenderer != null ? templateRenderer.sortingOrder : 0;
                sr.color = templateRenderer != null ? templateRenderer.color : Color.white;

                tiles.Add(tileObj.transform);
            }

            // Position all tiles initially
            RepositionTiles();
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            // Calculate parallax offset
            float cameraDeltaY = cameraTransform.position.y - cameraStartPosition.y;
            float parallaxOffset = cameraDeltaY * parallaxFactorY;

            // Apply parallax to base position
            float baseY = startY + parallaxOffset;

            // Reposition tiles around camera
            RepositionTilesAroundCamera(baseY);
        }

        private void RepositionTiles()
        {
            if (tiles.Count == 0 || cam == null) return;

            float cameraY = cameraTransform != null ? cameraTransform.position.y : 0;

            // Position tiles to cover the view
            int halfCount = tiles.Count / 2;

            for (int i = 0; i < tiles.Count; i++)
            {
                int offset = i - halfCount;
                float y = cameraY + (offset * calculatedHeight);

                tiles[i].position = new Vector3(
                    transform.position.x,
                    y,
                    transform.position.z
                );
            }
        }

        private void RepositionTilesAroundCamera(float baseY)
        {
            if (tiles.Count == 0 || cam == null) return;

            float cameraY = cameraTransform.position.y;
            float viewHeight = cam.orthographicSize * 2f;

            // Calculate the "anchor" tile position - where the base tile should be
            // relative to its parallax movement
            float anchorY = baseY;

            // Wrap anchor to be near camera
            float tileOffsetFromCamera = anchorY - cameraY;

            // Normalize to within one tile height of camera
            while (tileOffsetFromCamera > calculatedHeight / 2f)
            {
                tileOffsetFromCamera -= calculatedHeight;
            }
            while (tileOffsetFromCamera < -calculatedHeight / 2f)
            {
                tileOffsetFromCamera += calculatedHeight;
            }

            anchorY = cameraY + tileOffsetFromCamera;

            // Position all tiles relative to anchor
            int halfCount = tiles.Count / 2;

            for (int i = 0; i < tiles.Count; i++)
            {
                int offset = i - halfCount;
                float y = anchorY + (offset * calculatedHeight);

                tiles[i].position = new Vector3(
                    transform.position.x,
                    y,
                    transform.position.z
                );
            }
        }

        private void OnDestroy()
        {
            // Clean up created tiles
            foreach (var tile in tiles)
            {
                if (tile != null && tile != transform)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(tile.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(tile.gameObject);
                    }
                }
            }
            tiles.Clear();
        }

        [ContextMenu("Recalculate Tiles")]
        private void RecalculateTiles()
        {
            if (Application.isPlaying)
            {
                CreateTiles();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Show tile coverage area
            float height = tileHeight > 0 ? tileHeight : 20f;

            if (Application.isPlaying && calculatedHeight > 0)
            {
                height = calculatedHeight;
            }

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireCube(
                transform.position,
                new Vector3(10f, height, 0)
            );
        }
#endif
    }
}
