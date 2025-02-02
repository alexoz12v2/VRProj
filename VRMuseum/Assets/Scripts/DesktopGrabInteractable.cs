using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace vrm
{
    class DesktopGrabInteractable : MonoBehaviour
    {
        private Action<CallbackContext> _f;
        private Action OnGameStarted;
        private Action OnGameDestroy;

        private bool grabbed = false;

        private void Start()
        {
            _f = (CallbackContext ctx) => { OnInteract(gameObject, ctx); };
            OnGameStarted = () =>
            {
                GameManager.Instance.player.GrabCallbackEvent += _f;
            };
            OnGameDestroy = () =>
            {
                GameManager.Instance.player.GrabCallbackEvent -= _f;
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

        public static void OnInteract(GameObject self, CallbackContext ctx)
        {
            var component = self.GetComponent<DesktopGrabInteractable>();
            var rigidBody = self.GetComponent<Rigidbody>();
            bool pressed = ctx.started;
            bool released = ctx.canceled;
            if (component && !component.grabbed && pressed && Methods.CheckScreenCircleIntersection(new SingletonList<GameObject>(self), 10f) >= 0)
            {
                if (!GameManager.Instance.GameState.HasFlag(GameState.TinkerableInteractable) && !GameManager.Instance.GameState.HasFlag(GameState.Paused))
                {
                    component.grabbed = true;
                    if (rigidBody)
                        rigidBody.isKinematic = true;
                    self.SetLayerRecursively((int)Layers.Grabbed);
                    Debug.Log("GRABBED");
                    Vector3 offset = Camera.main.transform.forward * 1f + Camera.main.transform.up * -0.2f; 
                    self.transform.SetPositionAndRotation(Camera.main.transform.position + offset, Quaternion.identity);

                    GameManager.Instance.GameState |= GameState.TinkerableInteractable;
                    GameManager.Instance.player.MovementBreak += component.OnMovementBreak;
                }
            }
            if (component && component.grabbed)
            {
                if (pressed)
                {
                    GameManager.Instance.player.playerInput.currentActionMap["Move"].Disable();
                }
                else if (released)
                {
                    GameManager.Instance.player.playerInput.currentActionMap["Move"].Enable();
                }
            }
        }

        private void OnMovementBreak()
        {
            var rigidBody = gameObject.GetComponent<Rigidbody>();
            if (!GameManager.Instance.GameState.HasFlag(GameState.Paused))
            {
                grabbed = false;
                if (rigidBody)
                    rigidBody.isKinematic = false;
                gameObject.SetLayerRecursively((int)Layers.Default);
                Debug.Log("UNGRABBED");
                rigidBody.AddRelativeForce(Vector3.forward, ForceMode.Impulse);

                GameManager.Instance.player.MovementBreak -= OnMovementBreak;
                GameManager.Instance.GameState &= ~GameState.TinkerableInteractable;
            }
        }
    }
}
