using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace vrm
{
    public class CreatePrefab : MonoBehaviour
    {
      /*  [MenuItem("Extras/Create Prefab From Selection")]
        static void DoCreatePrefab()
        {
            Transform[] transforms = Selection.transforms;
            foreach (Transform t in transforms)
            {
                GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(t.gameObject, "Assets/Prefabs/" + t.gameObject.name + ".prefab", InteractionMode.UserAction);
            }
        }*/
    }

    public class HandInteractors
    {
        public HandInteractors(XRRayInteractor rayInteractor, XRDirectInteractor directInteractor)
        {
            RayInteractor = rayInteractor;
            DirectInteractor = directInteractor;
        }

        public XRRayInteractor RayInteractor;
        public XRDirectInteractor DirectInteractor;
    }

    [Flags]
    public enum Layers : int
    {
        Default = 0,
        Outlined = 8,
        Grabbed = 10,
        GrabbedOutline = 11,
        Hidden = 12,
        Object = 13,
        OutlineObject = 14,
    }

    [Flags]
    public enum InteractionLayers : int
    {
        Default = 0,
        Component = 1 << 29,
        Grabbed = 1 << 30,
        Teleport = 1 << 31
    }

    public enum Hand
    {
        Left = 0,
        Right = 1,
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
        public static Tags LeftXRControllerChild = new("LeftXRControllerChild");
        public static Tags RightXRControllerChild = new("RightXRControllerChild");
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

        static public bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    return true; // Scene is already loaded
                }
            }
            return false; // Scene is not loaded
        }

        static public bool IsSceneLoaded(string sceneName, out Scene outScene)
        {
            outScene = SceneManager.GetSceneAt(0);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    outScene = scene;
                    return true; // Scene is already loaded
                }
            }

            return false; // Scene is not loaded
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

        public IList<Scene> GetLoadedScenesWith(Predicate<Scene> predicate)
        {
            var list = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (predicate(scene))
                    list.Add(scene);
            }
            return list;
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

        public delegate bool ObjectCallbackPredicate(GameObject obj, InputAction.CallbackContext ctx);
        public static Action<InputAction.CallbackContext> RotateFromDeltaCallback(GameObject gameObject, ObjectCallbackPredicate pred, float angularSpeed)
        {
            return ctx =>
            {
                Debug.Log($"Rotation Callback for {gameObject}, predicate...");
                if (pred(gameObject, ctx))
                {
                    Debug.Log($"Rotation Callback for {gameObject},After predicate");
                    Quaternion targetRotation = QuaternionFromMouseDelta(ctx.ReadValue<Vector2>(), angularSpeed) * gameObject.transform.rotation;
                    var rigidbody = gameObject.GetComponent<Rigidbody>();
                    if (rigidbody != null && !rigidbody.isKinematic)
                        ApplyTorqueToRotation(rigidbody, targetRotation, angularSpeed);
                    else
                        gameObject.transform.rotation = targetRotation;
                }
            };
        }

        private static void ApplyTorqueToRotation(Rigidbody rb, Quaternion targetRotation, float angularSpeed)
        {
            float torqueForce = 5f;  // Increase for stronger input response
            float dampingFactor = 0.7f;  // Lower = stronger damping (e.g., 0.7 = 30% reduction per frame)
            float velocityThreshold = 0.1f;  // Minimum speed before stopping
            float maxAngularSpeed = 5f;  // Prevents excessive spinning

            // Compute the necessary torque
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(rb.rotation);
            deltaRotation.ToAngleAxis(out float rotationAngle, out Vector3 rotationAxis);

            if (rotationAngle > 180f) rotationAngle -= 360f;

            // If rotation difference is small, stop applying torque
            if (Mathf.Abs(rotationAngle) < velocityThreshold)
            {
                rb.angularVelocity = Vector3.zero;  // **Immediate stop for better control**
                return;
            }

            // Apply torque only if below max speed
            Vector3 torque = rotationAxis.normalized * (rotationAngle * torqueForce);
            if (rb.angularVelocity.magnitude < maxAngularSpeed)
                rb.AddTorque(torque, ForceMode.VelocityChange);

            // **Stronger Damping**: Slows down rotation more aggressively
            rb.angularVelocity *= dampingFactor;

            // **Extra Stability**: If angular velocity is too low, force a full stop
            if (rb.angularVelocity.magnitude < velocityThreshold)
                rb.angularVelocity = Vector3.zero;
        }

        static public Action<InputAction.CallbackContext> ParentMouseDeltaCallback(GameObject gameObject, float angleSpeed = 5f)
        {
            return RotateFromDeltaCallback(gameObject, (obj, ctx) => obj.GetComponent<Rigidbody>() != null && ctx.performed, angleSpeed);
        }

        static public Tuple<HandInteractors, HandInteractors> GetXRControllers(GameObject parent)
        {
            return new(
                InteractorsFromGameObject(FindFirstChildRecursive(parent, obj => obj.CompareTag(Tags.LeftXRController))),
                InteractorsFromGameObject(FindFirstChildRecursive(parent, obj => obj.CompareTag(Tags.RightXRController)))
            );
        }

        static public void DisableRayEnableDirectOnLayer(HandInteractors interactors, InteractionLayers directLayer, InteractionLayers rayLayer)
        {
            interactors.DirectInteractor.interactionLayers |= (int)directLayer;
            interactors.RayInteractor.interactionLayers &= ~(int)rayLayer;
            var lineRenderer = interactors.RayInteractor.gameObject.GetComponent<LineRenderer>();
            if (lineRenderer != null)
                lineRenderer.renderingLayerMask = 0;
        }

        static public void EnableRayDisableDirectOnLayer(HandInteractors interactors, InteractionLayers directLayer, InteractionLayers rayLayer)
        {
            interactors.DirectInteractor.interactionLayers &= ~(int)directLayer;
            interactors.RayInteractor.interactionLayers |= (int)rayLayer;
            var lineRenderer = interactors.RayInteractor.gameObject.GetComponent<LineRenderer>();
            if (lineRenderer != null)
                lineRenderer.renderingLayerMask = 1;
        }

        /// @warning If you modify the prefab, eg adding more interactors, modify this too!
        private static HandInteractors InteractorsFromGameObject(GameObject handObject)
        {
            var rayList = handObject.GetComponentsInChildren<XRRayInteractor>();
            var directList = handObject.GetComponentsInChildren<XRDirectInteractor>();
            if (rayList.Length == 0 || directList.Length == 0)
                throw new SystemException("What");
            return new(rayList[0], directList[0]);
        }

        static public void DebugPrintPredicate(UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputBinding> bindings, Predicate<InputBinding> pred)
        {
            foreach (InputBinding binding in bindings)
            {
                if (pred(binding))
                {
                    string msg = $"Binding: {binding.path} ";
                    if (binding.overridePath != null)
                        msg += $"override {binding.overridePath}";
                    Debug.Log(msg);
                }
            }
        }

        public static void SetCursorFPSBehaviour()
        {
            Cursor.visible = PauseManager.Exists && PauseManager.Instance.Paused;
            Cursor.lockState = CursorLockMode.Confined;
        }

        // To be used with startsWith
        static public string PathRegexFromTag(GameObject gameObject)
        {
            if (gameObject.CompareTag(Tags.LeftXRControllerChild))
            {
                return "<XRController>{LeftHand}";
            }
            else if (gameObject.CompareTag(Tags.RightXRControllerChild))
            {
                return "<XRController>{RightHand}";
            }
            else
                throw new SystemException(
                    $"Couldn't find controller game object. Name: {gameObject.name} " +
                    $"Unexpected tag: {gameObject.tag}");
        }

        static public void DisableBinding(InputAction inputAction, Predicate<InputBinding> predicate)
        {
            int bindingIndex = inputAction.bindings.IndexOf(predicate);
            if (bindingIndex >= 0)
            {
                inputAction.ApplyBindingOverride(bindingIndex, "");
            }
        }

        static public void EnableAllBindingsWith(InputAction inputAction, Predicate<InputBinding> predicate)
        {
            for (int i = 0; i < inputAction.bindings.Count; ++i)
            {
                if (predicate(inputAction.bindings[i]))
                {
                    inputAction.RemoveBindingOverride(i);
                }
            }
        }

        static public void ResetCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        static public Hand HandFromInteractor(GameObject interactor)
        {
            if (interactor.CompareTag(Tags.LeftXRControllerChild))
                return Hand.Left;
            else if (interactor.CompareTag(Tags.RightXRControllerChild))
                return Hand.Right;
            else
                throw new SystemException("Unexpected gameobject");
        }

        static public IEnumerator WaitFor(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
        static public string XRPrimaryAxisPathHand(GameObject interactor)
        {
            return "<XRController>{" + (interactor.CompareTag(Tags.LeftXRControllerChild) ? "Left" : "Right") + "Hand}/{Primary2DAxis}";
        }
    }

    public class Actions
    {
        private Actions() { }

        public static InputAction GetInputAction(string actionName)
        {
            var player = GameManager.Exists ? GameManager.Instance.player : null;
            var actionMap = (player != null && player.playerInput != null) ? player.playerInput.currentActionMap : null;

            if (actionMap != null)
            {
                return actionMap.FindAction(actionName);
            }

            return null;
        }

        public static InputAction Interact() => GetInputAction("Interact");
        public static InputAction Deselect() => GetInputAction("Deselect");
        public static InputAction Pause() => GetInputAction("Pause");
        public static InputAction Map() => GetInputAction("Map");
    }

    public class FollowTargetPosition : MonoBehaviour
    {
        [Header("General")]
        public GameObject Target = null;
        public Vector3 Offset = Vector3.zero;
        public float SmoothSpeed = 1.0f;

        [Header("For Targets having a Rigidbody")]
        public bool ForceKinematic = false;
        public bool ForceDisableGravity = true;
        public float StoppingDistance = 0.1f;
        public float DampingStrength = 10f;
        public float ForceStrength = 1f;

        private Rigidbody m_RigidBody;
        private bool m_HasAttached = false;
        private Transform m_OriginalParent;
        private XRBaseInteractor m_CurrentInteractor;

        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_OriginalParent = transform.parent;
        }

        private void LateUpdate()
        {
            if (Target == null || m_HasAttached)
                return;

            bool useForces = m_RigidBody != null && !ForceKinematic;
            if (m_RigidBody != null)
            {
                if (ForceDisableGravity)
                    m_RigidBody.useGravity = false;
                if (ForceKinematic)
                    m_RigidBody.isKinematic = true;
            }

            Vector3 desiredPosition = Target.transform.position + Offset;
            float distance = Vector3.Distance(transform.position, desiredPosition);

            if (distance < StoppingDistance)
            {
                AttachToInteractor();
            }
            else if (useForces)
            {
                ApplyForces(desiredPosition);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * SmoothSpeed);
            }
        }

        private void ApplyForces(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            Vector3 forceDirection = direction * SmoothSpeed;

            Vector3 targetVelocity = direction.normalized * SmoothSpeed;
            Vector3 velocityError = targetVelocity - m_RigidBody.velocity;

            m_RigidBody.AddForce(forceDirection * ForceStrength + velocityError * DampingStrength, ForceMode.Acceleration);
        }

        private void AttachToInteractor()
        {
            if (Target.TryGetComponent(out XRBaseInteractor interactor))
            {
                Debug.Log("Object reached target, attaching to interactor.");
                m_HasAttached = true;

                // Parent the object to the interactor to follow its movement
                transform.SetParent(interactor.transform, true);

                // If it has a Rigidbody, make it kinematic to follow smoothly
                if (m_RigidBody != null)
                {
                    m_RigidBody.isKinematic = true;
                    m_RigidBody.velocity = Vector3.zero;
                    m_RigidBody.angularVelocity = Vector3.zero;
                }

                m_CurrentInteractor = interactor;
            }
        }

        public void OnSelectExit(XRBaseInteractor interactor)
        {
            if (interactor == m_CurrentInteractor)
            {
                Debug.Log("Interactor released object, restoring parent.");
                Destroy(this); // Component is removed, OnDestroy restores original parent
            }
        }

        private void OnDestroy()
        {
            // Restore original parent when the object is released
            transform.SetParent(m_OriginalParent, true);

            if (m_RigidBody != null)
            {
                m_RigidBody.isKinematic = false;  // Restore original physics settings
                m_RigidBody.useGravity = true;
            }
        }
    }

    public struct ProgressBarSpec
    {
        public ProgressBarSpec(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float X;
        public float Y;
        public float Width;
        public float Height;
    }

    public class ImGUIProgressBar : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float Progress = 0f;
        public ProgressBarSpec? Spec;

        private void OnGUI()
        {
            // Set progress bar position and size
            float barWidth = Spec.HasValue ? Spec.Value.Width : 300f;
            float barHeight = Spec.HasValue ? Spec.Value.Height : 25f;
            float barX = Spec.HasValue ? Spec.Value.X : (Screen.width - barWidth) / 2;
            float barY = Spec.HasValue ? Spec.Value.Y : Screen.height - 50f;

            // Draw background bar
            GUI.Box(new Rect(barX, barY, barWidth, barHeight), "");

            // Draw progress bar
            GUI.Box(new Rect(barX, barY, barWidth * Progress, barHeight), "Progress");
        }
    }
}
