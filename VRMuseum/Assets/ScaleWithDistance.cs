using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class ScaleWithDistance : MonoBehaviour
    {
        [Header("Minimum Distance/Max Scale")]
        [SerializeField] private float m_MinDistance = 10f;
        [SerializeField] private float m_ScaleAtMin = 0.2f;

        [Header("Maximum Distance/Min Scale")]
        [SerializeField] private float m_MaxDistance = 50f;
        [SerializeField] private float m_ScaleAtMax = 2.5f;

        private float m_MaxDistanceSquared { get => m_MaxDistance * m_MaxDistance; }
        private float m_MinDistanceSquared { get => m_MinDistance * m_MinDistance; }


        void Update()
        {
            var sqrDist = Mathf.Clamp(Vector3.SqrMagnitude(Camera.main.transform.position - transform.position), m_MinDistanceSquared, m_MaxDistanceSquared);
            var scaleValue = Mathf.Lerp(m_ScaleAtMin, m_ScaleAtMax, sqrDist / m_MaxDistanceSquared);
            transform.localScale = new(scaleValue, scaleValue, scaleValue);
        }
    }
}
