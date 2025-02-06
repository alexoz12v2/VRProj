using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace vrm
{
    [Flags]
    public enum Layers : int
    {
        Default = 0,
        Outlined = 8,
        Grabbed = 10,
        GrabbedOutline = 11,
    }

    public class Tags
    {
        public string Value { get; private set; }

        private Tags(string value) { Value = value; }
        public override string ToString() { return Value; }
        public static implicit operator string(Tags _taga) => _taga.Value;

        public static Tags UIContent = new("UIContent");
        public static Tags UIParagraphTitle = new("UIParagraphTitle");
        public static Tags UIParagraphButton = new("UIParagraphButton");
        public static Tags UICanvas = new("UICanvas");
        public static Tags CameraSocket = new("CameraSocket");
        public static Tags ExplosionBarrier = new("ExplosionBarrier");
        public static Tags Component = new("Component");
        public static Tags LeftXRController = new("LeftXRController");
        public static Tags RightXRController = new("RightXRController");
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

        // Meant for desktop only, radius is a percentage relative to the smaller screen dimension
        public static float CheckScreenCircleIntersection(GameObject target, float radius, bool lookSelf = false)
        {
            radius *= 0.01f * Math.Min(Screen.width, Screen.height);
            Vector2 screenCenter = new(Screen.width / 2, Screen.height / 2);
            Bounds bounds = lookSelf ? target.GetComponent<Renderer>().bounds : BoundsInChildren(target.transform);

            // Get all 8 corners of the bounds
            Vector3[] worldCorners = GetBoundsCorners(bounds);
            List<Vector2> screenCorners = new();

            bool isInFrontOfCamera = false;

            // Project corners to screen space
            foreach (var corner in worldCorners)
            {
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(corner);

                // Check if any point is in front of the camera
                if (screenPoint.z > 0)
                    isInFrontOfCamera = true;

                screenCorners.Add(new(screenPoint.x, screenPoint.y));
            }

            if (isInFrontOfCamera)
            {
                if (CircleIntersectsPolygon(screenCenter, radius, screenCorners))
                {
                    // Find the closest point on the polygon to the screen center
                    float minDistance = float.MaxValue;

                    foreach (var corner in screenCorners)
                    {
                        float distance = Vector2.Distance(screenCenter, corner);
                        minDistance = Mathf.Min(minDistance, distance);
                    }

                    return minDistance;
                }
            }

            return -1f;
        }
        // Get all 8 corners of the bounds
        public static Vector3[] GetBoundsCorners(Bounds bounds)
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
            // Step 1: Get the AABB (axis-aligned bounding box) from the polygon points
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            foreach (var point in polygon)
            {
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            // Step 2: Find the closest point on the rectangle to the circle's center
            Vector2 closestPoint = new Vector2(
                Mathf.Clamp(circleCenter.x, min.x, max.x),
                Mathf.Clamp(circleCenter.y, min.y, max.y)
            );

            // Step 3: Check if the distance between the circle's center and the closest point is within the radius
            float distanceSquared = (circleCenter - closestPoint).sqrMagnitude;

            return distanceSquared <= radius * radius;
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

        public static bool RayPlaneIntersection(Ray ray, Vector3 planeNormal, Vector3 planePoint, out Vector3 intersection)
        {
            intersection = Vector3.zero;
            float denom = Vector3.Dot(planeNormal, ray.direction);

            if (Mathf.Abs(denom) > 1e-6f) // Prevent division by zero for parallel rays
            {
                float t = Vector3.Dot(planePoint - ray.origin, planeNormal) / denom;
                if (t >= 0) // Ensure the intersection is in front of the camera
                {
                    intersection = ray.origin + t * ray.direction;
                    return true;
                }
            }
            return false;
        }

        // TODO maybe remove
        static public List<Rigidbody> GetChildRigidbodies(GameObject parent)
        {
            List<Rigidbody> rigidbodies = new List<Rigidbody>();

            // Get all rigidbodies in children, including inactive ones
            Rigidbody[] allRigidbodies = parent.GetComponentsInChildren<Rigidbody>(true);

            foreach (Rigidbody rb in allRigidbodies)
            {
                // Exclude the parent Rigidbody if it exists
                if (rb.gameObject != parent)
                {
                    rigidbodies.Add(rb);
                }
            }

            return rigidbodies;
        }

        static public void ForEachChildWith(GameObject parent, Predicate<GameObject> pred, Action<GameObject> func)
        {
            if (parent == null || pred == null || func == null)
                return;

            // Recursive helper function
            void Traverse(GameObject obj)
            {
                foreach (Transform child in obj.transform)
                {
                    GameObject childObj = child.gameObject;

                    if (pred(childObj))
                    {
                        func(childObj);
                    }

                    // Recursively check this child's children
                    Traverse(childObj);
                }
            }

            // Start traversal from the parent
            Traverse(parent);
        }

        static public void RemoveComponent<T>(GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component != null)
                UnityEngine.Object.Destroy(component);
        }

        public static GameObject FindFirstChildRecursive(GameObject parent, Predicate<GameObject> pred)
        {
            if (parent == null || pred == null)
                return null;

            foreach (Transform child in parent.transform)
            {
                GameObject childObj = child.gameObject;

                // Check the current child
                if (pred(childObj))
                {
                    return childObj;
                }

                // Recursively check the child's children
                GameObject result = FindFirstChildRecursive(childObj, pred);
                if (result != null)
                {
                    return result;  // Return as soon as a match is found in deeper levels
                }
            }

            return null;  // No match found
        }

        public static void ForEachComponent<T>(GameObject gameObject, Action<T> action) where T : Component
        {
            if (gameObject == null || action == null)
                return;

            T[] components = gameObject.GetComponents<T>();
            foreach (T component in components)
            {
                action(component);
            }
        }

        public static Quaternion QuaternionFromMouseDelta(Vector2 delta, float angularSpeed)
        {
            delta *= angularSpeed;
            delta.x *= -1;
            Quaternion xRot = Quaternion.AngleAxis(delta.x * Time.fixedDeltaTime, Camera.main.transform.up);
            Quaternion yRot = Quaternion.AngleAxis(delta.y * Time.fixedDeltaTime, Camera.main.transform.right);
            return xRot * yRot;
        }

        public delegate bool ObjectCallbackPredicate(GameObject obj, UnityEngine.InputSystem.InputAction.CallbackContext ctx);
        public static Action<UnityEngine.InputSystem.InputAction.CallbackContext> RotateFromDeltaCallback(GameObject gameObject, ObjectCallbackPredicate pred, float angularSpeed)
        {
            return ctx =>
            {
                Debug.Log($"Rotation Callback for {gameObject}, predicate...");
                if (pred(gameObject, ctx))
                {
                    Debug.Log($"Rotation Callback for {gameObject},After predicate"); 
                    Quaternion rot = QuaternionFromMouseDelta(ctx.ReadValue<Vector2>(), angularSpeed);
                    gameObject.transform.rotation = rot * gameObject.transform.rotation;
                }
            };
        }

        static public Action<UnityEngine.InputSystem.InputAction.CallbackContext> ParentMouseDeltaCallback(GameObject gameObject, float angleSpeed = 5f)
        {
            return RotateFromDeltaCallback(gameObject, (obj, ctx) => obj.GetComponent<Rigidbody>() != null && ctx.performed, angleSpeed);
        }

        static public Tuple<GameObject, GameObject> GetXRControllers(GameObject parent) {
            return new(FindFirstChildRecursive(parent, obj => obj.CompareTag(Tags.LeftXRController)), FindFirstChildRecursive(parent, obj => obj.CompareTag(Tags.RightXRController)));
        }
    }

    public class FollowTargetPosition : MonoBehaviour 
    {
        [Header("General")]
        public GameObject Target = null;
        public Vector3 Offset = Vector3.zero;
        public float SmoothSpeed = 1.0f;

        [Header("For Targets having a rigidbody")]
        public bool ForceKinematic = false;
        public bool ForceDisableGravity = true;
        public float StoppingDistance = 0.1f;
        public float DampingStrength = 10f;

        private void LateUpdate()
        {
            if (Target == null)
                return;
            var rigidbody = GetComponent<Rigidbody>();
            bool noForce = true;
            if (rigidbody != null)
            {
                if (ForceDisableGravity)
                    rigidbody.useGravity = false;
                if (ForceKinematic)
                    rigidbody.isKinematic = true;
                else
                    noForce = false;
            }

            if (noForce)
            {
                transform.position = Vector3.Lerp(transform.position, Target.transform.position + Offset, Time.deltaTime * SmoothSpeed);
            }
            else
            {
                Vector3 desiredPosition = Target.transform.position + Offset;
                Vector3 direction = desiredPosition - transform.position;
                Vector3 forceDirection = direction * SmoothSpeed;
                float distance = direction.magnitude;
                if (distance < StoppingDistance)
                {
                    rigidbody.velocity = Vector3.zero;
                }
                else
                {
                    // Compute velocity to reach the target smoothly (Critically damped motion)
                    Vector3 targetVelocity = direction.normalized * SmoothSpeed;
                    Vector3 velocityError = targetVelocity - rigidbody.velocity; 
                    rigidbody.AddForce((forceDirection) + velocityError * DampingStrength, ForceMode.Acceleration);
                }
            }
        }
    }

}
