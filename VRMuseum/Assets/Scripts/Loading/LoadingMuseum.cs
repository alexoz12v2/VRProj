using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingMuseum : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        LoadingManager.Instance.PlayDissolving();
    }
}
