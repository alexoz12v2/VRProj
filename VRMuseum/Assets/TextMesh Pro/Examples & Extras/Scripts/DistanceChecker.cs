using UnityEngine;
using System.Collections;

namespace vrm
{
    class DistanceChecker : MonoBehaviour
    {
        [SerializeField] private float _distance = 3f;


        private void Update()
        {
             if(Vector3.Distance(transform.position, Camera.main.transform.position) > _distance)
            {
                DestroyImmediate(gameObject);
            }

        }
    }
}