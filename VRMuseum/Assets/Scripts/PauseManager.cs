using UnityEngine;
using UnityEngine.InputSystem;

namespace vrm
{
    class PauseManager : Singleton<PauseManager>
    {
        [SerializeField] private GameObject pauseMenu = null;


        private bool m_Paused = false;

        public bool Paused { get { return m_Paused; } }

        public System.Action OnPaused;
        public System.Action OnUnpaused;

        public void Register()
        {
            Actions.Pause().performed += OnPause;
        }

        public void TogglePause()
        {
            m_Paused = !m_Paused;
            Time.timeScale = m_Paused ? 0 : 1;

            if (m_Paused)
                OnPaused?.Invoke();
            else
                OnUnpaused?.Invoke();

            if (pauseMenu != null)
                pauseMenu.SetActive(m_Paused);
            Debug.Log($"Pause {(m_Paused ? "Entered" : "Exited")}");
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        protected override void OnDestroyCallback()
        {
        }
    }
}

