using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace vrm
{
    // TODO: For now it focuses on Desktop. VR Integration will come at a later step, hopefully using the default input action assets from XR Interaction Toolkit
    public class ScatterRigidbodyChildren : MonoBehaviour
    {

        [SerializeField] private BoxCollider explosionBounds;

        void Start()
        {// TODO callback cleanup
            explosionBounds.enabled = false;
            GameManager.Instance.GameStartStarted += OnGameStarted;
            GameManager.Instance.GameStateChanged += OnGameStateChanged;
        }

        private void OnGameStarted()
        {
            InputAction explodeAction = GameManager.Instance.player.playerInput.currentActionMap["Explode"];
            explodeAction.performed += OnExplode;
            GameManager.Instance.player.MovementBreak += OnMovementBreak;
        }

        private void OnMovementBreak()
        {
            IList<Rigidbody> rigidbodies = Methods.GetChildRigidbodies(gameObject);
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
                rb.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                rb.isKinematic = false;
            }
            if (GameManager.Instance.GameState.HasFlag(GameState.TinkerableDecomposed))
                GameManager.Instance.GameState &= ~GameState.TinkerableDecomposed; // TinkerableInteractable removed by other component
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
        }


        private void OnExplode(CallbackContext ctx)
        {
            Debug.Log("OnExplode");
            Func<GameState, bool> isValidState = (_s) => { return (_s.HasFlag(GameState.TinkerableInteractable) || _s.HasFlag(GameState.TinkerableDecomposed)) && !_s.HasFlag(GameState.Paused); };
            if (!isValidState(GameManager.Instance.GameState))
                throw new SystemException("What");

            IList<Rigidbody> rigidbodies = Methods.GetChildRigidbodies(gameObject);
            if (GameManager.Instance.GameState.HasFlag(GameState.TinkerableInteractable))
            { // TODO Use specific layer for explosion, and move all components in there
                InputAction explodeAction = GameManager.Instance.player.playerInput.currentActionMap["Explode"];
                Debug.Log("Remove Input Action Event");
                new Task(processExplosion(3f, rigidbodies)).Finished += (manual) =>
                {
                    Debug.Log("Add Input Action Event");
                    explodeAction.performed += OnExplode;
                };
                explodeAction.performed -= OnExplode;
            }
            else
            {
                foreach (Rigidbody rb in rigidbodies)
                {
                    rb.isKinematic = true;
                    rb.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    rb.isKinematic = false;
                }
                GameManager.Instance.GameState &= ~GameState.TinkerableDecomposed;
            }
        }

        private IEnumerator processExplosion(float maxSeconds, IList<Rigidbody> rigidbodies)
        {
            Debug.Log("Process Explosion");
            explosionBounds.enabled = true;
            foreach (Rigidbody rb in rigidbodies)
            {
                Vector3 dir = UnityEngine.Random.insideUnitSphere;
                rb.isKinematic = false;
                rb.useGravity = false;
                //rb.AddExplosionForce(10f, rb.transform.position + dir, 10f);
                rb.AddForce(dir * 10f, ForceMode.Impulse);
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
            explosionBounds.enabled = false;
            GameManager.Instance.GameState |= GameState.TinkerableDecomposed;
        }
    }
}
