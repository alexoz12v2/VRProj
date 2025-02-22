using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace vrm
{
    public class HUDManager : Singleton<HUDManager>
    {
        [SerializeField] private GameObject m_MessagePrefab;
        [SerializeField] private GameObject m_MessageContainer;

        protected override void OnDestroyCallback()
        {
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var o = GameObject.Instantiate(m_MessagePrefab, m_MessageContainer.transform);
                if (o.TryGetComponent<MessageBehaviour>(out var msg))
                {
                    msg.TargetText = "TEST";
                }
            }
        }
    }
}
