using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class AlertOnFirstTriggerEnter : MonoBehaviour
    {
        private bool m_enteredOnce = false;

        private void OnTriggerEnter(Collider other)
        {
            if (m_enteredOnce)
                return;
            m_enteredOnce = true;

            HUDManager.Instance.AddHUDMessage("Cammina sulla trincea per entrarci");
        }
    }
}
