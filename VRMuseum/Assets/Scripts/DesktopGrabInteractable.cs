using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.InputAction;

namespace vrm
{
    // TODO handle both tinkerable and non
    class DesktopGrabInteractable : MonoBehaviour
    {
        private Action OnGameStarted;
        private Action OnGameDestroy;

        private bool grabbed = false;
        private bool inInteraction = false;
        private GameObject m_Active;
        private IDictionary<GameObject, Action<CallbackContext>> m_MouseDeltaCallbacks = new Dictionary<GameObject, Action<CallbackContext>>();

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

            float currentDistance = Mathf.Infinity;
            int index = -1;
            IList<GameObject> list = GetChildComponents();
            foreach (GameObject gameObject in list)
            {
                int current = 0;
                float screenSpaceDist = Methods.CheckScreenCircleIntersection(gameObject, 10f);
                if (screenSpaceDist >= 0 && screenSpaceDist < currentDistance)
                {
                    index = current;
                    currentDistance = screenSpaceDist;
                }
            }

            Debug.Log($"Multiple: Active Rotator is {(index == -1 ? "Nobody" : list[index])}");
            if (index == -1)
                return null;
            else
                return list[index];
        }

        private void Start()
        {
            m_MouseDeltaCallbacks.Add(gameObject, Methods.ParentMouseDeltaCallback(gameObject));
            OnGameStarted = () =>
            {
                GameManager.Instance.player.GrabCallbackEvent += OnInteract;
                var rigidBody = gameObject.AddComponent<Rigidbody>(); // TODO mass
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

        private void Update()
        {
            if (!grabbed)
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
            else if (grabbed)
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
                        inInteraction = true;
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
                        rigidBody.isKinematic = true;
                        rigidBody.useGravity = true;
                        rigidBody.constraints = RigidbodyConstraints.None; // Release the position constraint
                        GameManager.Instance.player.playerInput.currentActionMap["Rotate"].performed -= m_MouseDeltaCallbacks[m_Active];
                        m_Active = null;
                    }

                    GameManager.Instance.player.playerInput.currentActionMap["Move"].Enable();
                    inInteraction = false;
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
