using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class BindsOnTrenchSceneLoaded : MonoBehaviour
    {
        [SerializeField] private Material m_SkyboxMaterial = null;
        private EventInstance m_Ambient;
        private Material m_OriginalSkybox;

        private void Start()
        {
            m_Ambient = AudioManager.Instance.PlaySound2D(FMODEvents.Instance.AmbientForest);
            m_OriginalSkybox = RenderSettings.skybox;
            if (m_SkyboxMaterial != null)
            {
                RenderSettings.skybox = m_SkyboxMaterial;
                DynamicGI.UpdateEnvironment();
            }
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

            RenderSettings.skybox = m_OriginalSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }
}
