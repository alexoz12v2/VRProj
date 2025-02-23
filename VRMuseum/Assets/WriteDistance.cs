using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace vrm
{
    [RequireComponent(typeof(TextMeshPro))]
    public class WriteDistance : MonoBehaviour
    {
        [SerializeField] private string m_Prefix = "";
        [SerializeField] private string m_Suffix = " m";

        private TextMeshPro m_TMPro;

        private void Start()
        {
            m_TMPro = GetComponent<TextMeshPro>();
        }

        // Update is called once per frame
        private void Update()
        {
            string distance = Vector3.Distance(Camera.main.transform.position, transform.position).ToString("0.0");
            m_TMPro.SetText(m_Prefix + distance + m_Suffix);
        }
    }
}
