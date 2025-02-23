using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vrm
{
    public class CheckCollisionAndTeleport : MonoBehaviour
    {
        [SerializeField] float m_Seconds = 2f;

        private Task m_Task;

        private void OnTriggerEnter(Collider other)
        {
            m_Task = new(WaitAndTransition());
            Methods.RemoveComponent<ImGUIProgressBar>(gameObject);
            var comp = gameObject.AddComponent<ImGUIProgressBar>();
            m_Task.Finished += manual =>
            {
                if (!manual)
                    LoadingManager.Instance.PlayDissolving();
            };
        }

        private void OnTriggerExit(Collider other)
        {
            Methods.RemoveComponent<ImGUIProgressBar>(gameObject);
            m_Task.Stop();
        }
        private void Update()
        {
            if (PauseManager.Instance.Paused || !TryGetComponent<ImGUIProgressBar>(out var progressBar))
                return;

            progressBar.Progress += Time.deltaTime / m_Seconds;
            if (progressBar.Progress > 1f)
                Methods.RemoveComponent<ImGUIProgressBar>(gameObject);
        }

        private IEnumerator WaitAndTransition()
        {
            yield return new WaitForSeconds(m_Seconds);
        }
    }
}
