using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class DisappearAfterSeconds : MonoBehaviour
    {
        [SerializeField] private float m_Seconds = 1.5f;

        private float elapsedTime = 0f;
        private bool isActive = false;

        private void OnEnable()
        {
            elapsedTime = 0f;
            isActive = true;
        }

        private void Update()
        {
            if (!isActive) 
                return;

            elapsedTime += Time.unscaledDeltaTime; // Tracks time ignoring timeScale
            if (elapsedTime >= m_Seconds)
            {
                gameObject.SetActive(false);
                isActive = false;
            }
        }
    }
}
