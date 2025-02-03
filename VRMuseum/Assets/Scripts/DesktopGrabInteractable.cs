using Cinemachine;
using System;
using System.Collections;
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

        private void Start()
        {
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
                if (Methods.CheckScreenCircleIntersection(new SingletonList<GameObject>(gameObject), 10f) >= 0)
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
            var rigidBody = gameObject.GetComponent<Rigidbody>();
            if (rigidBody == null)
                return;

            bool pressed = ctx.started;
            bool released = ctx.canceled;
            if (!grabbed && pressed && Methods.CheckScreenCircleIntersection(new SingletonList<GameObject>(gameObject), 10f) >= 0)
            {
                if (!GameManager.Instance.GameState.HasFlag(GameState.TinkerableInteractable) && !GameManager.Instance.GameState.HasFlag(GameState.Paused))
                {
                    grabbed = true;
                    if (rigidBody)
                        rigidBody.isKinematic = true;
                    gameObject.SetLayerRecursively((int)Layers.Grabbed);
                    Vector3 offset = Camera.main.transform.forward * 1f + Camera.main.transform.up * -0.2f;
                    transform.SetPositionAndRotation(Camera.main.transform.position + offset, Quaternion.identity);

                    GameManager.Instance.GameState |= GameState.TinkerableInteractable;
                    GameManager.Instance.player.MovementBreak += OnMovementBreak;
                }
            }
            if (grabbed)
            {
                var pov = getCinemachineCameraPOV();
                if (pressed)
                {
                    if (pov)
                    {
                        pov.m_HorizontalAxis.m_MaxSpeed = 0f;
                        pov.m_VerticalAxis.m_MaxSpeed = 0f;
                    }

                    // Make Rigidbody non-kinematic temporarily to apply physics
                    // rigidBody.isKinematic = false;
                    // rigidBody.useGravity = false;

                    // Optional: Lock position to prevent linear movement
                    rigidBody.constraints = RigidbodyConstraints.FreezePosition; // Freeze all movement

                    GameManager.Instance.player.playerInput.currentActionMap["Move"].Disable();
                    GameManager.Instance.player.playerInput.currentActionMap["Rotate"].performed += RotateFromDelta;
                    inInteraction = true;
                }
                else if (released)
                {
                    if (pov)
                    {  // TODO mouse sensitivity custom
                        pov.m_HorizontalAxis.m_MaxSpeed = 100f;
                        pov.m_VerticalAxis.m_MaxSpeed = 100f;
                    }
                    rigidBody.isKinematic = true;
                    rigidBody.useGravity = true;
                    rigidBody.constraints = RigidbodyConstraints.None; // Release the position constraint

                    GameManager.Instance.player.playerInput.currentActionMap["Rotate"].performed -= RotateFromDelta;
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

        public void RotateFromDelta(InputAction.CallbackContext ctx)
        {
            var rigidBody = gameObject.GetComponent<Rigidbody>();
            if (rigidBody == null)
                return;
            float maxAngularVelocity = 5f;
            float torqueStrength = 50f;
            float sensitivity = 0.1f;
            if (ctx.performed && rigidBody != null)
            {
                Vector2 mouseDelta = ctx.ReadValue<Vector2>() * sensitivity;
                mouseDelta.x *= -1;

                Quaternion xRot = Quaternion.AngleAxis(mouseDelta.x * torqueStrength * Time.fixedDeltaTime, Camera.main.transform.up);
                Quaternion yRot = Quaternion.AngleAxis(mouseDelta.y * torqueStrength * Time.fixedDeltaTime, Camera.main.transform.right);
                transform.rotation = xRot * yRot * transform.rotation;
            }
        }

        private void OnMovementBreak()
        {
            var rigidBody = gameObject.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                GetComponent<ScatterRigidbodyChildren>().FromCompOnMovementBreak();
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
