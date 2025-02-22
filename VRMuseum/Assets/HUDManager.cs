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

        //private void Update()
        //{
        //    if (Mouse.current.leftButton.wasPressedThisFrame)
        //        AddHUDMessage("TEST");
        //}

        public IEnumerator AddHUDMessage(string msgText)
        {
            var o = GameObject.Instantiate(m_MessagePrefab, m_MessageContainer.transform);
            if (!o.TryGetComponent<MessageBehaviour>(out var msg))
                throw new System.Exception("dfsdfds");

            msg.TargetText = msgText;
            return new WaitUntil(() => msg.Written);
        }
    }
}
