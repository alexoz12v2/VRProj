using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace vrm
{
    public class BindsOnMuseumLoaded : MonoBehaviour
    {
        [SerializeField] private float m_InterMsgSeconds = 1f;
        [SerializeField] private GameObject m_Minimap;

        private bool m_InteractedOnce = false;
        private Task m_TutorialTask = null;
        private EventInstance m_Ambient;

        private void Start()
        {
            m_Ambient = AudioManager.Instance.PlaySound2D(FMODEvents.Instance.AmbientSound);
            PauseManager.Instance.OnPaused += OnPaused;
            PauseManager.Instance.OnUnpaused += OnUnpaused;
            m_TutorialTask = new Task(Tutorial());
            Actions.Map().performed += OnMap;
            foreach (var inspectable in FindObjectsOfType<InspectableObject>())
            {
                inspectable.Inspected += OnInteractableObject;
            }
        }

        private void OnInteractableObject()
        {
            if (m_InteractedOnce)
                return;
            m_InteractedOnce = true;
            HUDManager.Instance.AddHUDMessage("Cliccare una immagine per sentirne la spiegazione");
            HUDManager.Instance.AddHUDMessage("Premere 'X' o allontanarsi per chiudere l'interazione");
        }

        private void OnUnpaused()
        {
            Actions.Map().performed += OnMap;
        }

        private void OnPaused()
        {
            Actions.Map().performed -= OnMap;
        }

        private void OnDisable()
        {
            if (m_Ambient.isValid())
            {
                m_Ambient.getPlaybackState(out PLAYBACK_STATE state);
                if (state == PLAYBACK_STATE.PLAYING)
                {
                    m_Ambient.stop(STOP_MODE.IMMEDIATE);
                    m_Ambient.release();
                }
            }
            if (PauseManager.Exists)
            {
                PauseManager.Instance.OnPaused += OnPaused;
                PauseManager.Instance.OnUnpaused += OnUnpaused;
            }
            if (GameManager.Exists)
                Actions.Map().performed -= OnMap;
        }

        private void OnMap(InputAction.CallbackContext context)
        {
            m_Minimap.SetActive(!m_Minimap.activeSelf);
        }

        private IEnumerator Tutorial()
        {
            yield return HUDManager.Instance.AddHUDMessage("Benvenuto Nel VR Museum!");
            yield return HUDManager.Instance.AddHUDMessage("Premere 'M' Per mostrare/nascondere oggetti interagibili");
        }
    }
}
