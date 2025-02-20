using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace vrm
{
    [RequireComponent(typeof(Collider))]
    public class SceneTransitionOnTrigger : MonoBehaviour
    {
        [SerializeField] private float m_Range = 4f;

        private Collider m_Collider;

        private void Start()
        {
            if (!TryGetComponent<Collider>(out m_Collider))
                Debug.LogError("Collider not found");
        }

        private void OnEnable()
        {
            Actions.Interact().performed += OnInteract;
            PauseManager.Instance.OnPaused += OnPaused;
            PauseManager.Instance.OnUnpaused += OnUnpaused;
        }

        private void OnDisable()
        {
            if (GameManager.Exists && GameManager.Instance.player != null)
                Actions.Interact().performed += OnInteract;
            if (GameManager.Exists && PauseManager.Exists)
            {
                PauseManager.Instance.OnPaused -= OnPaused;
                PauseManager.Instance.OnUnpaused -= OnUnpaused;
            }
        }

        private void OnPaused()
        {
            Actions.Interact().performed -= OnInteract;
        }

        private void OnUnpaused()
        {
            Actions.Interact().performed += OnInteract;
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
            if (m_Collider.Raycast(ray, out RaycastHit hit, m_Range))
            {
            }
        }
    }
}
