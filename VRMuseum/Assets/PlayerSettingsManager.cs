using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class PlayerSettingsManager : Singleton<PlayerSettingsManager>
    {
        private PlayerSettingsScriptableObject m_PlayerSettings;

        public Action<PlayerSettingsScriptableObject> PlayerSettingsChanged = null;

        public PlayerSettingsScriptableObject PlayerSettings
        {
            get { return m_PlayerSettings; }
            set
            {
                PlayerSettingsChanged?.Invoke(value);
                m_PlayerSettings = value;
            }
        }

        private void Awake()
        {
            // TODO load JSON from disk?
            m_PlayerSettings = ScriptableObject.CreateInstance<PlayerSettingsScriptableObject>();
            PlayerSettingsChanged = null;
            m_PlayerSettings.MouseSensitivity.VertSpeed = 100f;
            m_PlayerSettings.MouseSensitivity.HorzSpeed = 100f;
            m_PlayerSettings.WalkingSpeed = 3f;
        }

        protected override void OnDestroyCallback()
        {
        }
    }
}
