using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.InputSystem.InputAction;

namespace vrm
{
    // TODO handle both tinkerable and non
    class DesktopGrabInteractable : MonoBehaviour
    {
        private Action OnGameStarted;
        private Action OnGameDestroy;

        private bool grabbed = false;
        private GameObject m_Active;
        private IDictionary<GameObject, Action<CallbackContext>> m_MouseDeltaCallbacks = new Dictionary<GameObject, Action<CallbackContext>>();

        [SerializeField] private bool m_DebugDraw = false;
        [Header("Desktop Settings")]
        [SerializeField] private float m_ScreenDistanceThreshold = 1f;
        [SerializeField] private float m_CenterDistanceThreshold = 1f;

        private GameObject m_Left = null;
        private GameObject m_Right = null;

        public void ResetMouseDeltaCallbacks()
        {
            InputAction rotateAction = GameManager.Instance.player.playerInput.currentActionMap["Rotate"];
            if (m_MouseDeltaCallbacks.Count > 1)
            {
                GameObject originalKey = null;
                foreach (var pair in m_MouseDeltaCallbacks)
                {
                    if (pair.Key == gameObject)
                    {
                        originalKey = pair.Key;
                        continue;
                    }
                    if (m_Active == pair.Key)
                    {
                        m_Active = null;
                    }
                    rotateAction.performed -= pair.Value;
                }
                m_MouseDeltaCallbacks.Clear();
                if (originalKey == null)
                    throw new SystemException("WHAT");
                m_MouseDeltaCallbacks.Add(originalKey, Methods.ParentMouseDeltaCallback(gameObject));
            }
        }

        public void AddAllMouseDeltaCallbacks(IDictionary<GameObject, Action<CallbackContext>> actions)
        {
            foreach (var action in actions)
            {
                m_MouseDeltaCallbacks.Add(action);
            }
        }

        private IList<GameObject> GetChildComponents()
        {
            List<GameObject> list = new();
            Methods.ForEachChildWith(gameObject, (child) => child.CompareTag("Component"), (child) => list.Add(child));
            return list;
        }

        private GameObject GrabActiveRotateGameObject()
        {
            Debug.Log($"Requesting active rotator, count: {m_MouseDeltaCallbacks.Count}");
            if (m_MouseDeltaCallbacks.Count == 1)
                return gameObject;

            IList<Tuple<GameObject, float>> list = GetChildComponents()
                .Select(obj => new Tuple<GameObject, float>(obj, Methods.CheckScreenCircleIntersection(obj, 10f, true)))
                .Select(tup => { Debug.Log($"INTERSECTION: {tup.Item1} at {tup.Item2}"); return tup; }) // TODO comment when you are done
                .Where(tup => tup.Item2 > 0f)
                .ToList();
            if (list.Count > 0)
            {
                GameObject selected = list.Aggregate((acc, current) =>
                {
                    Vector3 accCenter = acc.Item1.GetComponent<Renderer>().bounds.center;
                    Vector3 currCenter = current.Item1.GetComponent<Renderer>().bounds.center;

                    // World-space distance from the camera center
                    float accCenterDist = Vector3.Distance(Camera.main.transform.position, accCenter);
                    float currCenterDist = Vector3.Distance(Camera.main.transform.position, currCenter);

                    // Hybrid score combining screen-space and center distance
                    float accScore = (acc.Item2 * m_ScreenDistanceThreshold) - (accCenterDist * m_CenterDistanceThreshold);
                    float currScore = (current.Item2 * m_ScreenDistanceThreshold) - (currCenterDist * m_CenterDistanceThreshold);

                    return (currScore > accScore) ? current : acc;
                }).Item1;

                Debug.Log($"Selected: {selected}");

                return selected;
            }
            else
                return null;
        }

        private void Start()
        {
            m_MouseDeltaCallbacks.Add(gameObject, Methods.ParentMouseDeltaCallback(gameObject));
            OnGameStarted = () =>
            {
                if (m_DebugDraw)
                {
                    foreach (var obj in GetChildComponents())
                    {
                        var comp = obj.AddComponent<DebugDrawBounds>();
                        comp.DrawScreenProjected = true;
                    }
                }
                if (!DeviceCheckAndSpawn.Instance.isXR)
                {
                    GameManager.Instance.player.GrabCallbackEvent += OnInteract;
                }
                else
                {
                    (m_Left, m_Right) = Methods.GetXRControllers(Camera.main.gameObject.transform.parent.gameObject);
                    if (m_Left == null || m_Right == null)
                        throw new SystemException("You Should set left and right as the gameobjects containing a hierarchy of XRController components and interactors");
                    ConfigureXRSimpleInteractable(gameObject.AddComponent<XRSimpleInteractable>());
                }
                Rigidbody rigidBody = null;
                while (rigidBody == null)
                    rigidBody = gameObject.AddComponent<Rigidbody>(); // TODO mass
                rigidBody.isKinematic = false;
                rigidBody.useGravity = true;
            };
            OnGameDestroy = () =>
            {
                GameManager.Instance.player.GrabCallbackEvent -= OnInteract;
                GameManager.Instance.GameStartStarted -= OnGameStarted;
            };

            GameManager.Instance.GameStartStarted += OnGameStarted;
            GameManager.Instance.GameDestroy += OnGameDestroy;
        }

        // TODO maybe: any optional logic to a nested component?
        private void Update()
        {
            if (!grabbed && !DeviceCheckAndSpawn.Instance.isXR)
            {
                if (Methods.CheckScreenCircleIntersection(gameObject, 10f) >= 0)
                {
                    gameObject.SetLayerRecursively((int)Layers.Outlined);
                }
                else
                {
                    gameObject.SetLayerRecursively((int)Layers.Default);
                }
            }
        }

        private void ConfigureXRSimpleInteractable(XRSimpleInteractable interactable)
        {
            // set the layer such that Direct interactor cannot see you
            interactable.interactionLayers = (int)InteractionLayers.Grabbed;

            // allow to select multiple objects at the same time
            interactable.selectMode = InteractableSelectMode.Multiple;

            // configure interactable events
            interactable.hoverEntered.AddListener(OnHoverEnter);
            interactable.hoverExited.AddListener(OnHoverExit);
            interactable.selectEntered.AddListener(OnSelectEnter);
            interactable.selectExited.AddListener(OnSelectExit);
            interactable.activated.AddListener(OnActivate);
            interactable.deactivated.AddListener(OnDeactivate);
        }

        private IXRSelectInteractor m_XRLastSelectInteractor = null;
        private void OnHoverEnter(HoverEnterEventArgs args)
        {
            Debug.Log("hoverEntered");
            gameObject.SetLayerRecursively((int)Layers.Outlined);
        }

        private void OnHoverExit(HoverExitEventArgs args)
        {
            Debug.Log("hoverExited");
            gameObject.SetLayerRecursively((int)Layers.Default);
        }

        // TODO Fix the fact that the direcct interactor is preferred over the ray interactor??
        private void OnSelectEnter(SelectEnterEventArgs args)
        {
            GameObject interactorObj = args.interactorObject.transform.gameObject;
            InputAction moveAction = GameManager.Instance.player.playerInput.currentActionMap["Move"];
            if (moveAction == null)
                throw new SystemException("Couldn't find move action");
            Debug.Log($"eelectEntered Interactor: {interactorObj.name}");

            string bindingPathRegex = Methods.PathRegexFromTag(interactorObj);
            if (m_XRLastSelectInteractor != null)
            {
                Methods.RemoveComponent<FollowTargetPosition>(args.interactableObject.transform.gameObject);
                Methods.EnableAllBindingsWith(moveAction, b => b.path.StartsWith("<XRController>"));
            }

            if (m_XRLastSelectInteractor == null || m_XRLastSelectInteractor.transform.gameObject != args.interactorObject.transform.gameObject)
            {
                var component = args.interactableObject.transform.gameObject.AddComponent<FollowTargetPosition>();
                component.Offset = Vector3.up * 0.1f; // TODO configurable
                component.DampingStrength = 10f; 
                component.ForceStrength = 30f;
                component.Target = args.interactorObject.transform.gameObject;
                m_XRLastSelectInteractor = args.interactorObject;
                Methods.DisableBinding(moveAction, b => b.path.StartsWith(bindingPathRegex));
                Methods.DebugPrintPredicate(moveAction.bindings, b => b.overridePath != null);
            }
            else if (m_XRLastSelectInteractor.transform.gameObject == args.interactorObject.transform.gameObject)
            {
                m_XRLastSelectInteractor = null;
                Debug.LogWarning("Exiting");
                var rigidbody = args.interactableObject.transform.gameObject.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.isKinematic = false;
                    rigidbody.useGravity = true;
                    rigidbody.AddRelativeForce(Vector3.forward, ForceMode.Impulse);
                }
                Methods.DebugPrintPredicate(moveAction.bindings, b => b.overridePath != null);
            }
        }

        private void OnSelectExit(SelectExitEventArgs args)
        {
            Debug.Log("selectExited");
        }

        private void OnActivate(ActivateEventArgs args)
        {
            Debug.Log("activated");
        }

        private void OnDeactivate(DeactivateEventArgs args)
        {
            Debug.Log("deactivated");
        }

        private void OnInteract(CallbackContext ctx)
        {

            bool pressed = ctx.started;
            bool released = ctx.canceled;
            if (!grabbed && pressed && Methods.CheckScreenCircleIntersection(gameObject, 10f) >= 0)
            {
                if (!GameManager.Instance.GameState.HasFlag(GameState.TinkerableInteractable) && !GameManager.Instance.GameState.HasFlag(GameState.Paused))
                {
                    grabbed = true;
                    var rigidBody = gameObject.GetComponent<Rigidbody>(); // shouldn't be null
                    if (rigidBody)
                        rigidBody.isKinematic = true;
                    gameObject.SetLayerRecursively((int)Layers.Grabbed);
                    Vector3 offset = Camera.main.transform.forward * 1f + Camera.main.transform.up * -0.2f;
                    transform.SetPositionAndRotation(Camera.main.transform.position + offset, Quaternion.identity);

                    GameManager.Instance.GameState |= GameState.TinkerableInteractable;
                    GameManager.Instance.player.MovementBreak += OnMovementBreak;
                }
            }
            else if (grabbed && !GameManager.Instance.GameState.HasFlag(GameState.Paused))
            {
                var pov = getCinemachineCameraPOV();
                if (pressed)
                {
                    Debug.Log("PRESSED---------------");
                    if (pov)
                    {
                        pov.m_HorizontalAxis.m_MaxSpeed = 0f;
                        pov.m_VerticalAxis.m_MaxSpeed = 0f;
                    }

                    // TODO register only the callback for the intersected object within the screen space circle and store it for later removal
                    // if store, and removal is alao propagated
                    GameManager.Instance.player.playerInput.currentActionMap["Move"].Disable();
                    m_Active = GrabActiveRotateGameObject();
                    if (m_Active != null)
                    {
                        GameManager.Instance.player.playerInput.currentActionMap["Rotate"].performed += m_MouseDeltaCallbacks[m_Active];
                        var rigidBody = m_Active.GetComponent<Rigidbody>(); // shouldn't be null
                        rigidBody.constraints = RigidbodyConstraints.FreezePosition; // Freeze all movement
                    }
                }
                else if (released)
                {
                    Debug.Log("RELEASED---------------");
                    if (pov)
                    {  // TODO mouse sensitivity custom
                        pov.m_HorizontalAxis.m_MaxSpeed = 100f;
                        pov.m_VerticalAxis.m_MaxSpeed = 100f;
                    }

                    if (m_Active)
                    {
                        var rigidBody = m_Active.GetComponent<Rigidbody>(); // shouldn't be null
                        //rigidBody.isKinematic = true;
                        rigidBody.useGravity = true;
                        rigidBody.constraints = RigidbodyConstraints.None; // Release the position constraint
                        GameManager.Instance.player.playerInput.currentActionMap["Rotate"].performed -= m_MouseDeltaCallbacks[m_Active];
                        m_Active = null;
                    }

                    GameManager.Instance.player.playerInput.currentActionMap["Move"].Enable();
                }
            }
        }

        private static CinemachinePOV getCinemachineCameraPOV()
        {

            var camera = GameManager.Instance.virtualCamera.GetComponent<CinemachineVirtualCamera>();
            if (camera)
                return camera.GetCinemachineComponent<CinemachinePOV>();
            return null;
        }

        // TODO: 1) If this is called while the object is being destructured, exceptions
        // TODO: 2) Reposition object to its starting transform 
        private void OnMovementBreak()
        {
            var rigidBody = gameObject.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                GetComponent<ScatterRigidbodyChildren>().FromCompOnMovementBreak();
                rigidBody = gameObject.GetComponent<Rigidbody>();
                if (rigidBody == null)
                    throw new SystemException("aaaa");
            }

            if (!GameManager.Instance.GameState.HasFlag(GameState.Paused))
            {
                grabbed = false;
                if (rigidBody)
                    rigidBody.isKinematic = false;
                gameObject.SetLayerRecursively((int)Layers.Default);
                rigidBody.AddRelativeForce(Vector3.forward, ForceMode.Impulse);

                GameManager.Instance.player.MovementBreak -= OnMovementBreak;
                GameManager.Instance.GameState &= ~GameState.TinkerableInteractable;
            }
        }
    }
}
