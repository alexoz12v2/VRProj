using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class BindsOnMuseumLoaded : MonoBehaviour
    {
        [SerializeField] private float m_InterMsgSeconds = 1f;

        private void Start()
        {
            new Task(Tutorial());
        }

        private IEnumerator Tutorial()
        {
            yield return HUDManager.Instance.AddHUDMessage("Benvenuto Nel VR Museum!");
            yield return HUDManager.Instance.AddHUDMessage("Premere 'M' Per mostrare/nascondere oggetti interagibili");
        }
    }
}
