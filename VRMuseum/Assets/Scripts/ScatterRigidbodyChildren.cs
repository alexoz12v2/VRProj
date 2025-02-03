using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace vrm
{
    // TODO: For now it focuses on Desktop. VR Integration will come at a later step, hopefully using the default input action assets from XR Interaction Toolkit
    public class ScatterRigidbodyChildren : MonoBehaviour
    {
        private Task m_ExplosionTask;

        void Start()
        {// TODO callback cleanup
            GameManager.Instance.GameStartStarted += OnGameStarted;
            GameManager.Instance.GameStateChanged += OnGameStateChanged;
        }

        private void OnGameStarted()
        {
            InputAction explodeAction = GameManager.Instance.player.playerInput.currentActionMap["Explode"];
            explodeAction.performed += OnExplode;
            //GameManager.Instance.player.MovementBreak += OnMovementBreak; // handled by desktopgrabinteratable
        }

        private void MakeChildrenTransformable()
        {
            var component = GetComponent<DesktopGrabInteractable>();
            if (component == null)
                return;

            IDictionary<GameObject, Action<CallbackContext>> callbacks = Methods.GetChildRigidbodies(gameObject)
                .Select(r => r.gameObject)
                .Select(obj => new Tuple<GameObject, Action<CallbackContext>>(obj, new Action<CallbackContext>((CallbackContext ctx) => Methods.ParentMouseDeltaCallback(obj))))
                .ToDictionary(t => t.Item1, t => t.Item2);
            component.AddAllMouseDeltaCallbacks(callbacks);
        }

        private void RestoreTransformableObjects()
        {
            var component = GetComponent<DesktopGrabInteractable>();
            if (component == null)
                return;
            component.ResetMouseDeltaCallbacks();
        }

        public void FromCompOnMovementBreak()
        {
            if (m_ExplosionTask != null && m_ExplosionTask.Running)
            {
                m_ExplosionTask.Stop();
            }
            HandleExplosionTermination();
        }

        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            InputAction explodeAction = GameManager.Instance.player.playerInput.currentActionMap["Explode"];
            if (newState.HasFlag(GameState.Paused))
                explodeAction.Disable();
            else if (newState.HasFlag(GameState.TinkerableInteractable) || newState.HasFlag(GameState.TinkerableDecomposed))
                explodeAction.Enable();
            else
                explodeAction.Disable();

            if (!newState.HasFlag(GameState.Paused) && oldState.HasFlag(GameState.TinkerableInteractable) && newState.HasFlag(GameState.TinkerableInteractable))
            {
                if (newState.HasFlag(GameState.TinkerableDecomposed))
                    MakeChildrenTransformable();
                else
                    RestoreTransformableObjects();
            }
        }


        private void OnExplode(CallbackContext ctx)
        {
            Debug.Log("OnExplode");
            Func<GameState, bool> isValidState = (_s) => { return (_s.HasFlag(GameState.TinkerableInteractable) || _s.HasFlag(GameState.TinkerableDecomposed)) && !_s.HasFlag(GameState.Paused); };
            if (!isValidState(GameManager.Instance.GameState))
                throw new SystemException("What");

            if (!GameManager.Instance.GameState.HasFlag(GameState.TinkerableDecomposed))
            { // TODO Use specific layer for explosion, and move all components in there
                Methods.ForEachChildWith(gameObject, (child) => child.CompareTag("Component"), (child) => child.AddComponent<Rigidbody>());
                Methods.RemoveComponent<Rigidbody>(gameObject); // Unity readds it if there are any RequireComponent[Rigidbody]
                IList<Rigidbody> rigidbodies = Methods.GetChildRigidbodies(gameObject);

                InputAction explodeAction = GameManager.Instance.player.playerInput.currentActionMap["Explode"];
                var barrier = Methods.FindFirstChildRecursive(gameObject, (child) => child.CompareTag("ExplosionBarrier"));
                Methods.ForEachComponent<Collider>(barrier, (collider) => collider.enabled = true);
                Debug.Log("Remove Input Action Event");
                m_ExplosionTask = new Task(processExplosion(0.3f, rigidbodies));
                m_ExplosionTask.Finished += (manual) =>
                {
                    Debug.Log("Add Input Action Event");
                    explodeAction.performed += OnExplode;
                    Methods.ForEachComponent<Collider>(barrier, (collider) => collider.enabled = false);
                };
                explodeAction.performed -= OnExplode;
            }
            else
            {
                HandleExplosionTermination();
            }
        }

        private void HandleExplosionTermination()
        {
            Methods.ForEachChildWith(gameObject, (child) => child.CompareTag("Component"), (child) =>
            {
                Methods.RemoveComponent<Rigidbody>(child);
                child.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            });
            var parentBody = gameObject.AddComponent<Rigidbody>();
            parentBody.isKinematic = true;
            GameManager.Instance.GameState &= ~GameState.TinkerableDecomposed;
        }

        private IEnumerator processExplosion(float maxSeconds, IList<Rigidbody> rigidbodies)
        {
            Debug.Log("Process Explosion");
            float depthIntensity = 0.1f; // TODO parameter
            float explosionIntensity = 1f;
            foreach (Rigidbody rb in rigidbodies)
            {
                Vector3 dir = UnityEngine.Random.insideUnitCircle;
                dir.z = UnityEngine.Random.value * depthIntensity;
                dir.Normalize();
                rb.isKinematic = false;
                rb.useGravity = false;
                //rb.AddExplosionForce(10f, rb.transform.position + dir, 10f);
                rb.AddRelativeForce(dir * explosionIntensity, ForceMode.Impulse);
            }

            float start = Time.time;
            while (Time.time - start < maxSeconds)
            {
                Debug.Log("Waiting for explosion...");
                yield return new WaitForFixedUpdate();
            }

            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
            }
            Debug.Log("Explosion finished, setting state");
            GameManager.Instance.GameState |= GameState.TinkerableDecomposed;
        }
    }
}
