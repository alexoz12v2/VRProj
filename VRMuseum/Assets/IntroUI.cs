using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace vrm
{
    public class IntroUI : MonoBehaviour
    {
        [SerializeField] private UnityEvent m_OnBackClick;
        [SerializeField] private UnityEvent m_OnEnterClick;

        public void OnEnterButton()
        {
            m_OnEnterClick?.Invoke();
        }

        public void OnBackButton()
        {
            m_OnBackClick?.Invoke();
        }
    }
}
