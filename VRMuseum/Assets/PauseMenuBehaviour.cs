using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace vrm
{
    public class PauseMenuBehaviour : MonoBehaviour
    {
        [SerializeField] private CanvasScaler m_Scaler = null;
        [SerializeField] private float m_Ratio = 1f;
        const float TargetWidth = 1920;

        private void Update()
        {
            m_Scaler.scaleFactor = (float)Screen.width / TargetWidth * m_Ratio;
        }

        public void OnClickResume()
        {
            PauseManager.Instance.TogglePause();
        }

        public void OnClickMainMenu()
        {
            Debug.Log("Menu");
            var op1 = SceneManager.UnloadSceneAsync(GetPlaygroundSceneName());
            GameManager.Instance.SelectedObject = null;
            PauseManager.Instance.TogglePause();
            var op2 = SceneManager.UnloadSceneAsync("PlayerScene");
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
        }

        private string GetPlaygroundSceneName()
        {
            if (SceneManager.GetSceneByName("TrenchScene").IsValid())
                return "TrenchScene";
            else
                return "Museum";
        }
    }
}
