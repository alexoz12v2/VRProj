using System.Collections;
using UnityEngine;


namespace vrm
{
    [RequireComponent(typeof(BoxCollider))]
    public class AutomaticRotationDoor : MonoBehaviour
    {

        private Quaternion m_StartRotation;
        private Task m_RotationTask = null;

        [SerializeField] private float m_RotationTime = 1.5f;

        void Start()
        {
            m_StartRotation = transform.rotation;
            bool foundCollider = false;
            // store a reference to the first trigger box collider
            foreach (var collider in gameObject.GetComponents<BoxCollider>())
            {
                if (collider.enabled && collider.isTrigger)
                {
                    foundCollider = true;
                    break;
                }
            }

            if (!foundCollider)
            {
                throw new System.SystemException($"Couldn't find enabled trigger collider for {gameObject.name}");
            }
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log("A collider has entered the DoorObject trigger");

            if (m_RotationTask != null && m_RotationTask.Running)
                m_RotationTask.Stop();

            float currentZRotation = transform.eulerAngles.z;
            float remainingTime = CalculateRemainingTime(currentZRotation, 90f);

            m_RotationTask = new Task(RotateToTarget(90f, remainingTime));
        }

        void OnTriggerExit(Collider other)
        {
            Debug.Log("A collider has exited the DoorObject trigger");

            if (m_RotationTask != null && m_RotationTask.Running)
                m_RotationTask.Stop();

            float currentZRotation = transform.eulerAngles.z;
            float remainingTime = CalculateRemainingTime(currentZRotation, 0f);

            m_RotationTask = new Task(RotateToTarget(0f, remainingTime));
        }

        private float CalculateRemainingTime(float currentZRotation, float targetZRotation)
        {
            float rotationDifference = Mathf.Abs(targetZRotation - currentZRotation);
            float remainingTime = (rotationDifference / 90f) * m_RotationTime;
            return Mathf.Max(remainingTime, 0.1f); // Prevent near-zero or zero duration
        }

        private IEnumerator RotateToTarget(float targetZRot, float seconds)
        {
            float elapsed = 0;
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, targetZRot);

            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / seconds; // Normalized time
                transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                yield return new WaitForFixedUpdate();
            }

            transform.rotation = targetRotation; // Ensure final rotation is set precisely
        }
    }
}
