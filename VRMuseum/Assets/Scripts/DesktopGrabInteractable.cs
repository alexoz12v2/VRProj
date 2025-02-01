using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace vrm
{
    class DesktopGrabInteractable : MonoBehaviour
    {
        private System.Action<InputAction.CallbackContext> _f;
        private System.Action OnGameStarted;
        private System.Action OnGameDestroy;

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

        public static void OnInteract(GameObject self, CallbackContext ctx)
        {
            if (Methods.CheckScreenCircleIntersection(new SingletonList<GameObject>(self), 10f) >= 0)
            {
                Debug.Log("SHOTS FIRED");
                Vector4 d4 = Camera.main.transform.worldToLocalMatrix * new Vector4(0, -0.2f, 0.3f, 0f);
                Vector3 d3 = new(d4.x, d4.y, d4.z);
                self.transform.position = Camera.main.transform.position + d3;
                self.transform.rotation = Quaternion.identity;
            }
        }
    }
}
