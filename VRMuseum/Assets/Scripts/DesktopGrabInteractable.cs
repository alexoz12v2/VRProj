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
    class DesktopGrabInteractable : MonoBehaviour
    {
        private Action OnGameStarted;
        private Action OnGameDestroy;

        private bool grabbed = false;

        private void Start()
        {
            OnGameStarted = () =>
            {
                GameManager.Instance.player.GrabCallbackEvent += OnInteract;
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
            var rigidBody = GetComponent<Rigidbody>();
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
                if (pressed)
                {
                    GameManager.Instance.player.playerInput.currentActionMap["Move"].Disable();
                    GameManager.Instance.player.playerInput.currentActionMap["Rotate"].performed += rotateFromDelta;
                    inInteraction = true;
                }
                else if (released)
                {
                    GameManager.Instance.player.playerInput.currentActionMap["Rotate"].performed -= rotateFromDelta;
                    GameManager.Instance.player.playerInput.currentActionMap["Move"].Enable();
                    inInteraction = false;
                }
            }
        }

        private bool inInteraction = false;
        private Task rotationTask = null;

        private void rotateFromDelta(CallbackContext ctx)
        {
            Debug.Log("RotateFromDelta");
            if (rotationTask == null || !rotationTask.Running)
            {
                Debug.Log("RotateFromDelta inside condtition");
                Vector4 rawInput = ctx.ReadValue<Vector2>();
                if (rawInput.y > rawInput.x)
                {
                    rawInput.z = rawInput.y;
                    rawInput.y = 0;
                }

                // for now use only the object in it of itself, then accuulate children with rigidbody
                var rigidBody = GetComponent<Rigidbody>();
                if (!rigidBody)
                    return;
                Vector3 worldSpaceDir = Camera.main.transform.localToWorldMatrix * rawInput;
                worldSpaceDir.Normalize();

                Vector3 pseudoVec = Vector3.Cross(worldSpaceDir, Camera.main.transform.forward);
                // DebugPrintXRDevices.Instance.AddMessage($"Torque: {pseudoVec}");

                // doesn't work on kinematic rigid bodies
                // rigidBody.AddTorque(pseudoVec, ForceMode.Impulse);
                rotationTask = new Task(rotateFromDelta(pseudoVec, rigidBody));
            }
        }

        private IEnumerator rotateFromDelta(Vector3 pseudoVec, Rigidbody rigidBody)
        {
            Debug.Log("Rotation Task Started");

            // Make Rigidbody non-kinematic temporarily to apply physics
            rigidBody.isKinematic = false;
            rigidBody.useGravity = false;

            // Optional: Lock position to prevent linear movement
            rigidBody.constraints = RigidbodyConstraints.FreezePosition; // Freeze all movement

            // Center of mass might need adjustment if it's off-center
            var oldCOM = rigidBody.centerOfMass;
            rigidBody.centerOfMass = Vector3.zero; // Make sure it's centered

            // Define the maximum angular velocity you want
            float maxAngularVelocity = 90f; // Max angular velocity (in degrees per second)

            // TODO: You can adjust torque strength if needed
            float torqueStrength = 10f;

            while (inInteraction)
            {
                // Apply torque based on pseudoVec direction
                rigidBody.AddTorque(pseudoVec * torqueStrength, ForceMode.Force);

                // Clamp angular velocity to max angular velocity to prevent infinite acceleration
                Vector3 clampedAngularVelocity = rigidBody.angularVelocity;

                // Limit the angular velocity to maxAngularVelocity
                clampedAngularVelocity = Vector3.ClampMagnitude(clampedAngularVelocity, Mathf.Deg2Rad * maxAngularVelocity);

                // Apply the clamped angular velocity back to the Rigidbody
                rigidBody.angularVelocity = clampedAngularVelocity;

                // Optional: Debug the angular velocity for tracking
                Debug.Log($"Current Angular Velocity: {rigidBody.angularVelocity}");

                yield return new WaitForFixedUpdate(); // Wait for the next fixed frame
            }

            // After interaction ends, reset Rigidbody settings
            rigidBody.isKinematic = true;
            rigidBody.useGravity = true;
            rigidBody.constraints = RigidbodyConstraints.None; // Release the position constraint
            rigidBody.centerOfMass = oldCOM;

            Debug.Log("Rotation Task Finished");
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
                rigidBody.AddRelativeForce(Vector3.forward, ForceMode.Impulse);

                GameManager.Instance.player.MovementBreak -= OnMovementBreak;
                GameManager.Instance.GameState &= ~GameState.TinkerableInteractable;
            }
        }
    }
}
