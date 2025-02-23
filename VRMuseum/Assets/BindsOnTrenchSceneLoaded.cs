using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class BindsOnTrenchSceneLoaded : MonoBehaviour
    {
        private EventInstance m_Ambient;

        private void Start()
        {
            m_Ambient = AudioManager.Instance.PlaySound2D(FMODEvents.Instance.AmbientForest);
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
        }
    }
}
