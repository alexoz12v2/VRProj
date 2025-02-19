using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace vrm
{
    public class PauseMenuBehaviour : MonoBehaviour
    {
        [SerializeField] private CanvasScaler m_Scaler = null;
        [SerializeField] private float m_Ratio = 1f;
        [Header("Audio Settings")]
        [SerializeField] private Slider m_AudioVolumeSlider = null;
        [SerializeField] private TextMeshProUGUI m_AudioVolumeSliderText = null;
        [Header("Player Settings")]
        [SerializeField] private Slider m_MouseSensitivitySlider = null;
        [SerializeField] private Slider m_WalkSpeedSlider = null;

        const float TargetWidth = 1920;

        private bool m_OverlayMenu = false;
        private HashSet<GameObject> m_Overlays = new();
        private PlayerSettingsScriptableObject m_InFlightSettingsShallowCopy = null;

        private void Start()
        {
            if (m_AudioVolumeSlider)
            {
                m_AudioVolumeSlider.value = AudioManager.Instance.MasterVolume;
                m_AudioVolumeSliderText.text = m_AudioVolumeSlider.value.ToString("0.0");
            }

            if (m_MouseSensitivitySlider)
            {
                m_MouseSensitivitySlider.value = PlayerSettingsManager.Instance.PlayerSettings.MouseSensitivity.HorzSpeed;
            }

            if (m_WalkSpeedSlider)
            {
                m_WalkSpeedSlider.value = PlayerSettingsManager.Instance.PlayerSettings.WalkingSpeed;
            }

            m_InFlightSettingsShallowCopy = PlayerSettingsManager.Instance.PlayerSettings;
        }

        private void Update()
        {
            m_Scaler.scaleFactor = (float)Screen.width / TargetWidth * m_Ratio;
        }

        private void OnDisable()
        {
            foreach (var overlay in m_Overlays)
            {
                overlay.SetActive(false);
            }
            m_Overlays.Clear();
            Methods.SetCursorFPSBehaviour();
        }

        public void OnClickResume()
        {
            if (!m_OverlayMenu)
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ClickUI);
                PauseManager.Instance.TogglePause();
            }
        }

        public void OnClickMainMenu()
        {
            if (!m_OverlayMenu)
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ClickUI);
                Debug.Log("Menu");
                var op1 = SceneManager.UnloadSceneAsync(GetPlaygroundSceneName());
                GameManager.Instance.SelectedObject = null;
                PauseManager.Instance.TogglePause();
                var op2 = SceneManager.UnloadSceneAsync("PlayerScene");
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
            }
        }

        public void ResetOverlay(GameObject obj)
        {
            m_OverlayMenu = false;
            m_Overlays.Remove(obj);
            obj.SetActive(false);
        }

        public void OpenOverlay(GameObject obj)
        {
            m_OverlayMenu = true;
            m_Overlays.Add(obj);
            obj.SetActive(true);
        }

        private string GetPlaygroundSceneName()
        {
            if (SceneManager.GetSceneByName("TrenchScene").IsValid())
                return "TrenchScene";
            else
                return "Museum";
        }

        public void OnMouseSensitivityChange(float sensitivity)
        {
            var settings = PlayerSettingsManager.Instance.PlayerSettings;
            settings.MouseSensitivity.HorzSpeed = sensitivity;
            settings.MouseSensitivity.VertSpeed = sensitivity;
            m_InFlightSettingsShallowCopy = settings;
        }

        public void OnWalkSpeedChange(float speed)
        {
            var settings = PlayerSettingsManager.Instance.PlayerSettings;
            settings.WalkingSpeed = speed;
            m_InFlightSettingsShallowCopy = settings;
        }

        public void OnApplyPlayerSettings()
        {
            PlayerSettingsManager.Instance.PlayerSettings = m_InFlightSettingsShallowCopy;
        }
    }
}
