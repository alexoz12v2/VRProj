using FMOD.Studio;
using FMODUnity;
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
        [SerializeField] private EventReference m_Benvenuto;

        private EventInstance m_Instance;

        private void OnEnable()
        {
            m_Instance = AudioManager.Instance.PlaySound2D(m_Benvenuto);
        }
        private void OnDisable()
        {
            if (AudioManager.Exists)
                AudioManager.Instance.StopSound(m_Instance);
        }

        public void TogglePauseBenvenuto()
        {
            m_Instance.getPaused(out bool paused);
            m_Instance.setPaused(!paused);
        }

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
