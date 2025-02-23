using UnityEngine;

namespace vrm
{
    public class ScaleWithDistance : MonoBehaviour
    {
        [Header("Distance Settings")]
        [SerializeField] private float m_MinDistance = 10f;
        [SerializeField] private float m_MaxDistance = 50f;

        [Header("Scale Settings")]
        [SerializeField] private float m_ScaleAtMin = 0.2f;
        [SerializeField] private float m_ScaleAtMax = 2.5f;

        [Header("Custom Falloff Curve")]
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private Vector3 m_StartScale;

        private float m_MaxDistanceSquared { get => m_MaxDistance * m_MaxDistance; }
        private float m_MinDistanceSquared { get => m_MinDistance * m_MinDistance; }

        private void Start()
        {
            m_StartScale = transform.localScale;
        }

        private void Update()
        {
            float sqrDist = Vector3.SqrMagnitude(Camera.main.transform.position - transform.position);
            sqrDist = Mathf.Clamp(sqrDist, m_MinDistanceSquared, m_MaxDistanceSquared);

            // Normalize distance between 0 and 1
            float t = (sqrDist - m_MinDistanceSquared) / (m_MaxDistanceSquared - m_MinDistanceSquared);

            // Get the scale factor from the custom curve
            float curveValue = scaleCurve.Evaluate(t);

            // Apply scale based on curve result
            float scaleValue = Mathf.Lerp(m_ScaleAtMin, m_ScaleAtMax, curveValue);
            transform.localScale = m_StartScale * scaleValue;
        }
    }
}
