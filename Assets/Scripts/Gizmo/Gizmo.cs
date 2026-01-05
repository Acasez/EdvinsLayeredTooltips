using UnityEngine;

public class Gizmo : MonoBehaviour
{
    public RectTransform canvasRect;
    public RectTransform tooltipRect;
    public Vector2 clampedPosition;

    private void OnDrawGizmos()
    {
        if (canvasRect == null || tooltipRect == null) return;

        // Draw canvas bounds
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(canvasRect.position, canvasRect.rect.size);

        // Draw tooltip bounds at clamped position
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(clampedPosition, tooltipRect.rect.size);
    }
}
