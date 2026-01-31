using UnityEngine;

/// <summary>
/// Utilidad para obtener los límites visibles de una cámara ortográfica
/// </summary>
public static class CameraBounds
{
    /// <summary>
    /// Obtiene la altura visible en unidades del mundo
    /// </summary>
    public static float GetVisibleHeight(Camera cam)
    {
        return cam.orthographicSize * 2f;
    }

    /// <summary>
    /// Obtiene el ancho visible en unidades del mundo
    /// </summary>
    public static float GetVisibleWidth(Camera cam)
    {
        return cam.orthographicSize * 2f * cam.aspect;
    }

    /// <summary>
    /// Obtiene los límites del mundo visible por la cámara
    /// </summary>
    public static Bounds GetWorldBounds(Camera cam)
    {
        float height = GetVisibleHeight(cam);
        float width = GetVisibleWidth(cam);
        Vector3 center = cam.transform.position;

        return new Bounds(
            new Vector3(center.x, center.y, 0),
            new Vector3(width, height, 0)
        );
    }

    /// <summary>
    /// Obtiene los bordes del área visible
    /// </summary>
    public static void GetEdges(Camera cam, out float left, out float right, out float top, out float bottom)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 pos = cam.transform.position;

        left = pos.x - halfWidth;
        right = pos.x + halfWidth;
        top = pos.y + halfHeight;
        bottom = pos.y - halfHeight;
    }

    /// <summary>
    /// Verifica si un punto está dentro del área visible
    /// </summary>
    public static bool IsPointVisible(Camera cam, Vector2 point)
    {
        GetEdges(cam, out float left, out float right, out float top, out float bottom);
        return point.x >= left && point.x <= right && point.y >= bottom && point.y <= top;
    }

    /// <summary>
    /// Calcula el Size necesario para mostrar X unidades de altura
    /// </summary>
    public static float SizeForHeight(float desiredHeight)
    {
        return desiredHeight / 2f;
    }

    /// <summary>
    /// Calcula el Size necesario para mostrar X unidades de ancho
    /// </summary>
    public static float SizeForWidth(float desiredWidth, float aspectRatio)
    {
        return desiredWidth / (2f * aspectRatio);
    }
}
