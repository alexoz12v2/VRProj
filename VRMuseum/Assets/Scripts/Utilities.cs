using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace vrm
{
    [Flags]
    public enum Layers : int
    {
        Default = 0,
        Outlined = 8,
    }
    public class SingletonList<T> : IList<T>
    {
        private readonly T _item;

        public SingletonList(T item)
        {
            _item = item;
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return _item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotSupportedException("Add not supported.");
        }

        public void Clear()
        {
            throw new NotSupportedException("Clear not supported.");
        }

        public bool Contains(T item)
        {
            if (item == null) return _item == null;

            return item.Equals(_item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");

            array[arrayIndex] = _item;
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException("Remove not supported.");
        }

        public int Count
        {
            get { return 1; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(T item)
        {
            return Contains(item) ? 0 : -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Insert not supported.");
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException("RemoveAt not supported.");
        }

        public T this[int index]
        {
            get
            {
                if (index == 0) return _item;

                throw new IndexOutOfRangeException();
            }
            set { throw new NotSupportedException("Set not supported."); }
        }
    }

    public class Methods
    {
        private Methods() { }

        // Helper method to find a child GameObject by name
        public static GameObject FindChildByName(GameObject parent, string name)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == name)
                    return child.gameObject;
            }
            return null;
        }

        // Helper method to find a child GameObject by tag
        public static GameObject FindChildWithTag(GameObject parent, string tag)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.CompareTag(tag))
                    return child.gameObject;
            }

            return null;
        }

        // Meant for desktop only
        public static int CheckScreenCircleIntersection(IList<GameObject> targets, float radius)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

            for (int i = 0; i < targets.Count; i++)
            {
                GameObject obj = targets[i];
                Bounds bounds = BoundsInChildren(obj.transform);

                // Get all 8 corners of the bounds
                Vector3[] worldCorners = GetBoundsCorners(bounds);
                List<Vector2> screenCorners = new List<Vector2>();

                bool isInFrontOfCamera = false;

                // Project corners to screen space
                foreach (var corner in worldCorners)
                {
                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(corner);

                    // Check if any point is in front of the camera
                    if (screenPoint.z > 0)
                        isInFrontOfCamera = true;

                    screenCorners.Add(new Vector2(screenPoint.x, screenPoint.y));
                }

                if (isInFrontOfCamera)
                {
                    if (CircleIntersectsPolygon(screenCenter, radius, screenCorners))
                        return i;
                }
            }

            return -1;
        }

        // Get all 8 corners of the bounds
        private static Vector3[] GetBoundsCorners(Bounds bounds)
        {
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            return new Vector3[]
            {
            center + new Vector3( extents.x,  extents.y,  extents.z),
            center + new Vector3( extents.x,  extents.y, -extents.z),
            center + new Vector3( extents.x, -extents.y,  extents.z),
            center + new Vector3( extents.x, -extents.y, -extents.z),
            center + new Vector3(-extents.x,  extents.y,  extents.z),
            center + new Vector3(-extents.x,  extents.y, -extents.z),
            center + new Vector3(-extents.x, -extents.y,  extents.z),
            center + new Vector3(-extents.x, -extents.y, -extents.z)
            };
        }

        // Check if a circle intersects with the projected quad
        private static bool CircleIntersectsPolygon(Vector2 circleCenter, float radius, List<Vector2> polygon)
        {
            // Check if any point of the polygon is inside the circle
            foreach (var point in polygon)
            {
                if (Vector2.Distance(circleCenter, point) <= radius)
                    return true;
            }

            // Check if any edge of the polygon intersects with the circle
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 p1 = polygon[i];
                Vector2 p2 = polygon[(i + 1) % polygon.Count];

                if (LineIntersectsCircle(p1, p2, circleCenter, radius))
                    return true;
            }

            // Check if circle is entirely within the polygon
            if (IsPointInPolygon(circleCenter, polygon))
                return true;

            return false;
        }

        // Check if a line segment intersects with a circle
        private static bool LineIntersectsCircle(Vector2 p1, Vector2 p2, Vector2 circleCenter, float radius)
        {
            Vector2 d = p2 - p1;
            Vector2 f = p1 - circleCenter;

            float a = Vector2.Dot(d, d);
            float b = 2 * Vector2.Dot(f, d);
            float c = Vector2.Dot(f, f) - radius * radius;

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                // No intersection
                return false;
            }
            else
            {
                discriminant = Mathf.Sqrt(discriminant);

                float t1 = (-b - discriminant) / (2 * a);
                float t2 = (-b + discriminant) / (2 * a);

                if ((t1 >= 0 && t1 <= 1) || (t2 >= 0 && t2 <= 1))
                    return true;
            }

            return false;
        }

        // Check if a point is inside a polygon using ray casting
        private static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            bool inside = false;
            int count = polygon.Count;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }


        public static Bounds BoundsInChildren(Transform paremt)
        {
            Renderer[] rr = paremt.GetComponentsInChildren<Renderer>();
            Bounds b = rr[0].bounds;
            foreach (Renderer r in rr) { b.Encapsulate(r.bounds); }
            return b;
        }
    }
}
