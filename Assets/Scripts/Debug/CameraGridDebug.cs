using UnityEngine;

namespace Gameplay.Debugging
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraGridDebug : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color originColor = new Color(1f, 0f, 0f, 0.5f);
        [SerializeField] private bool showOriginLines = true;
        [SerializeField] private bool showReferenceSquares = true;
        [SerializeField] private Color referenceSquareColor = new Color(0f, 1f, 0f, 0.5f);

        [Header("Grid Alignment")]
        [Tooltip("Align grid to camera bottom edge so cells are complete from bottom")]
        [SerializeField] private bool alignToCamera = false;
        [SerializeField] private Vector2 manualOffset = Vector2.zero;

        [Header("Debug Info")]
        [SerializeField] private bool showDebugInfo = true;

        private Camera cam;
        private Material lineMaterial;

        private void OnEnable()
        {
            cam = GetComponent<Camera>();
            CreateLineMaterial();
        }

        private void CreateLineMaterial()
        {
            if (lineMaterial != null) return;

            // Unity's built-in shader for drawing simple colored lines
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            // Enable alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }

        private void OnPostRender()
        {
            if (!showGrid) return;
            DrawGrid();
        }

        private void DrawGrid()
        {
            if (cam == null) return;
            if (lineMaterial == null) CreateLineMaterial();

            // Calculate camera bounds in world units
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;

            Vector3 camPos = cam.transform.position;

            // Grid boundaries (exact camera bounds)
            float left = camPos.x - width / 2f;
            float right = camPos.x + width / 2f;
            float bottom = camPos.y - height / 2f;
            float top = camPos.y + height / 2f;

            // Calculate offset to align grid with camera edges
            Vector2 offset = manualOffset;
            if (alignToCamera)
            {
                // Offset so grid starts exactly at camera bottom-left
                offset.x = left - Mathf.Floor(left);
                offset.y = bottom - Mathf.Floor(bottom);
            }

            // Integer grid lines within view
            int gridLeft = Mathf.FloorToInt(left - offset.x);
            int gridRight = Mathf.CeilToInt(right - offset.x);
            int gridBottom = Mathf.FloorToInt(bottom - offset.y);
            int gridTop = Mathf.CeilToInt(top - offset.y);

            lineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.LoadProjectionMatrix(cam.projectionMatrix);
            GL.modelview = cam.worldToCameraMatrix;

            GL.Begin(GL.LINES);

            // Draw vertical lines
            GL.Color(gridColor);
            for (int x = gridLeft; x <= gridRight; x++)
            {
                float xPos = x + offset.x;

                // Origin line in different color (only when not using offset)
                if (x == 0 && showOriginLines && !alignToCamera && manualOffset == Vector2.zero)
                {
                    GL.Color(originColor);
                }
                else
                {
                    GL.Color(gridColor);
                }

                GL.Vertex3(xPos, bottom, 0);
                GL.Vertex3(xPos, top, 0);
            }

            // Draw horizontal lines
            for (int y = gridBottom; y <= gridTop; y++)
            {
                float yPos = y + offset.y;

                // Origin line in different color (only when not using offset)
                if (y == 0 && showOriginLines && !alignToCamera && manualOffset == Vector2.zero)
                {
                    GL.Color(originColor);
                }
                else
                {
                    GL.Color(gridColor);
                }

                GL.Vertex3(left, yPos, 0);
                GL.Vertex3(right, yPos, 0);
            }

            GL.End();

            // Draw reference squares (1x1 units) to verify scale
            if (showReferenceSquares)
            {
                GL.Begin(GL.LINES);
                GL.Color(referenceSquareColor);

                // Draw a 1x1 square at origin (0,0) to (1,1)
                // Bottom
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(1, 0, 0);
                // Right
                GL.Vertex3(1, 0, 0);
                GL.Vertex3(1, 1, 0);
                // Top
                GL.Vertex3(1, 1, 0);
                GL.Vertex3(0, 1, 0);
                // Left
                GL.Vertex3(0, 1, 0);
                GL.Vertex3(0, 0, 0);

                // Draw another at (2,0) to (3,1) for comparison
                GL.Vertex3(2, 0, 0);
                GL.Vertex3(3, 0, 0);
                GL.Vertex3(3, 0, 0);
                GL.Vertex3(3, 1, 0);
                GL.Vertex3(3, 1, 0);
                GL.Vertex3(2, 1, 0);
                GL.Vertex3(2, 1, 0);
                GL.Vertex3(2, 0, 0);

                GL.End();
            }

            GL.PopMatrix();
        }

        private void OnGUI()
        {
            if (!showDebugInfo || cam == null) return;

            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;

            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label($"<color=yellow><b>Camera Grid Debug</b></color>", new GUIStyle { richText = true, fontSize = 14 });
            GUILayout.Label($"Orthographic Size: {cam.orthographicSize}");
            GUILayout.Label($"Aspect Ratio: {cam.aspect:F3}");
            GUILayout.Label($"View Width: {width:F2} units");
            GUILayout.Label($"View Height: {height:F2} units");
            GUILayout.Label($"Screen: {Screen.width}x{Screen.height}");
            GUILayout.EndArea();
        }

        // Also draw in Scene view using Gizmos
        private void OnDrawGizmos()
        {
            if (!showGrid) return;
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) return;

            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;

            Vector3 camPos = cam.transform.position;

            float left = camPos.x - width / 2f;
            float right = camPos.x + width / 2f;
            float bottom = camPos.y - height / 2f;
            float top = camPos.y + height / 2f;

            // Calculate offset
            Vector2 offset = manualOffset;
            if (alignToCamera)
            {
                offset.x = left - Mathf.Floor(left);
                offset.y = bottom - Mathf.Floor(bottom);
            }

            int gridLeft = Mathf.FloorToInt(left - offset.x);
            int gridRight = Mathf.CeilToInt(right - offset.x);
            int gridBottom = Mathf.FloorToInt(bottom - offset.y);
            int gridTop = Mathf.CeilToInt(top - offset.y);

            // Draw vertical lines
            for (int x = gridLeft; x <= gridRight; x++)
            {
                float xPos = x + offset.x;
                bool isOrigin = x == 0 && showOriginLines && !alignToCamera && manualOffset == Vector2.zero;
                Gizmos.color = isOrigin ? originColor : gridColor;
                Gizmos.DrawLine(new Vector3(xPos, bottom, 0), new Vector3(xPos, top, 0));
            }

            // Draw horizontal lines
            for (int y = gridBottom; y <= gridTop; y++)
            {
                float yPos = y + offset.y;
                bool isOrigin = y == 0 && showOriginLines && !alignToCamera && manualOffset == Vector2.zero;
                Gizmos.color = isOrigin ? originColor : gridColor;
                Gizmos.DrawLine(new Vector3(left, yPos, 0), new Vector3(right, yPos, 0));
            }

            // Draw reference squares
            if (showReferenceSquares)
            {
                Gizmos.color = referenceSquareColor;
                // 1x1 square at origin
                Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
                Gizmos.DrawLine(new Vector3(1, 0, 0), new Vector3(1, 1, 0));
                Gizmos.DrawLine(new Vector3(1, 1, 0), new Vector3(0, 1, 0));
                Gizmos.DrawLine(new Vector3(0, 1, 0), new Vector3(0, 0, 0));
                // 1x1 square at (2,0)
                Gizmos.DrawLine(new Vector3(2, 0, 0), new Vector3(3, 0, 0));
                Gizmos.DrawLine(new Vector3(3, 0, 0), new Vector3(3, 1, 0));
                Gizmos.DrawLine(new Vector3(3, 1, 0), new Vector3(2, 1, 0));
                Gizmos.DrawLine(new Vector3(2, 1, 0), new Vector3(2, 0, 0));
            }
        }

        private void OnDisable()
        {
            if (lineMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(lineMaterial);
                else
                    DestroyImmediate(lineMaterial);
            }
        }
    }
}
