using UnityEngine;

namespace vrm
{
    [System.Serializable]    
    public struct MouseSensitivity
    {
        public float HorzSpeed;
        public float VertSpeed;
    }

    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings", order = 1)]
    public class PlayerSettingsScriptableObject : ScriptableObject
    {
        public MouseSensitivity MouseSensitivity;
        public float WalkingSpeed;
    }
}
