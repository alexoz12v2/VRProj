using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class RigidbodyGroup : MonoBehaviour
    {
        public float followStrength = 10f; // Strength of force keeping children in place
        public bool autoConfigureJoints = true; // Automatically create joints if missing

        private List<Rigidbody> childRigidbodies = new List<Rigidbody>();
        private Rigidbody rootRigidbody;

        void Start()
        {
            rootRigidbody = GetComponent<Rigidbody>();

            if (rootRigidbody == null)
            {
                Debug.LogError("[RigidbodyGroup] No Rigidbody found on the root object!");
                return;
            }

            FindChildRigidbodies(transform);
            SetupJoints();
        }

        void FindChildRigidbodies(Transform parent)
        {
            foreach (Transform child in parent)
            {
                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    childRigidbodies.Add(rb);
                }

                // Recursively search children
                FindChildRigidbodies(child);
            }
        }

        void SetupJoints()
        {
            foreach (Rigidbody childRb in childRigidbodies)
            {
                if (autoConfigureJoints && childRb.GetComponent<Joint>() == null)
                {
                    ConfigurableJoint joint = childRb.gameObject.AddComponent<ConfigurableJoint>();
                    joint.connectedBody = rootRigidbody;

                    // Lock position while allowing slight flexibility
                    joint.xMotion = ConfigurableJointMotion.Limited;
                    joint.yMotion = ConfigurableJointMotion.Limited;
                    joint.zMotion = ConfigurableJointMotion.Limited;

                    // Lock rotation
                    joint.angularXMotion = ConfigurableJointMotion.Limited;
                    joint.angularYMotion = ConfigurableJointMotion.Limited;
                    joint.angularZMotion = ConfigurableJointMotion.Limited;

                    // Configure joint strength
                    JointDrive drive = new JointDrive
                    {
                        positionSpring = followStrength,
                        positionDamper = 1f,
                        maximumForce = float.MaxValue
                    };
                    joint.xDrive = drive;
                    joint.yDrive = drive;
                    joint.zDrive = drive;
                }
            }
        }

        public void ApplyForce(Vector3 force, ForceMode forceMode)
        {
            rootRigidbody.AddForce(force, forceMode);
            foreach (var rb in childRigidbodies)
            {
                rb.AddForce(force * 0.5f, forceMode); // Apply half the force to children
            }
        }

        public void ApplyTorque(Vector3 torque, ForceMode forceMode)
        {
            rootRigidbody.AddTorque(torque, forceMode);
            foreach (var rb in childRigidbodies)
            {
                rb.AddTorque(torque * 0.5f, forceMode);
            }
        }
    }
}