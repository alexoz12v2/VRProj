using UnityEngine;

namespace vrm
{
    /// <summary>
    /// Attach this to a gameobject while in playmode to visualize the bounds of its mesh in the scene editor
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(Renderer))]
    class DebugDrawBounds : MonoBehaviour
    {
        private Bounds m_Bounds;
        public bool DrawScreenProjected = false;

        private void OnDrawGizmos()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null)
                return;
            m_Bounds = renderer.bounds;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(m_Bounds.center, m_Bounds.size);
        }

        private void OnGUI()
        {
            if (!DrawScreenProjected || Camera.main == null) return;

            Vector3[] worldCorners = Methods.GetBoundsCorners(m_Bounds);

            // Project corners to screen space
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            foreach (var corner in worldCorners)
            {
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(corner);

                // Ignore points behind the camera
                if (screenPoint.z < 0) continue;

                // Flip Y-axis because GUI coordinates start from the top-left
                screenPoint.y = Screen.height - screenPoint.y;

                min = Vector2.Min(min, screenPoint);
                max = Vector2.Max(max, screenPoint);
            }

            // Draw the bounding box if it's valid
            if (min.x < max.x && min.y < max.y)
            {
                Rect screenRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

                // Draw the bounding box
                DrawBoundingBox(screenRect, Color.red);
            }
        }

        private void DrawBoundingBox(Rect rect, Color color)
        {
            Color prevColor = GUI.color;
            GUI.color = color;

            // Draw 4 lines to create a rectangle
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1), Texture2D.whiteTexture); // Top
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1), Texture2D.whiteTexture); // Bottom
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1, rect.height), Texture2D.whiteTexture); // Left
            GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1, rect.height), Texture2D.whiteTexture); // Right

            GUI.color = prevColor;
        }
    }
}
