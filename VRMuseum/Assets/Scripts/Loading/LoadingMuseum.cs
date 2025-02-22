using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class LoadingMuseum : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // LoadingManager.Instance.PlayDissolving();
            LoadingManager.Instance.DissolveToMainMenu();
        }
    }
}
