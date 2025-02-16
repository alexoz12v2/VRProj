using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace vrm
{

    [System.Serializable]
    public class ImageBundle
    {
        public Collider Collider;
        public EventReference AudioEventRef;
        private bool m_Selected = false;

        public bool Selected { get { return m_Selected; } set { m_Selected = value; } }
    }

    // Warning: Works only if child list doesn't change and if colliders are not removed
    public class NearestChildCollision : MonoBehaviour
    {

        [SerializeField] private List<ImageBundle> m_ImageBundles = new();
        [SerializeField] private float m_MaxDistance = 4f;

        private EventInstance? m_audioInstance;

        // Warning: Works only if the object starts as disabled (GameManager player initialization already happened)
        private void OnEnable()
        {
            Actions.Interact().performed += OnInteract;
            PauseManager.Instance.OnPaused += OnPaused;
            PauseManager.Instance.OnUnpaused += OnUnpaused;
        }

        private void OnDisable()
        {
            Actions.Interact().performed -= OnInteract;
            PauseManager.Instance.OnPaused -= OnPaused;
            PauseManager.Instance.OnUnpaused -= OnUnpaused;
        }

        private void OnPaused()
        {
            Actions.Interact().performed -= OnInteract;
        }

        private void OnUnpaused()
        {
            Actions.Interact().performed += OnInteract;
        }

        private void Update()
        {
            if (m_ImageBundles.Count == 0 || PauseManager.Instance.Paused)
                return;

            Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
            var list = m_ImageBundles
                .Select(bundle =>
                {
                    if (bundle.Collider.Raycast(ray, out RaycastHit hit, m_MaxDistance))
                        return (hit.distance, bundle);
                    else
                        return (float.PositiveInfinity, bundle);
                })
                .OrderBy(tuple => tuple.Item1)
                .Select(tuple => tuple.Item2)
                .ToList();
            if (list.Count > 0)
            {
                GameObject go = list[0].Collider.gameObject;
                go.layer = (int)Layers.OutlineObject;
                list[0].Selected = true;
                for (int i = 1; i < list.Count; ++i)
                {
                    go = list[i].Collider.gameObject;
                    go.layer = (int)Layers.Default;
                    list[i].Selected = false;
                }
            }
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            var bundles = m_ImageBundles.Where(bundle => bundle.Selected).ToList();
            if (bundles.Count == 0)
                return;
            var bundle = bundles[0];

            if (m_audioInstance.HasValue)
            {
                AudioManager.Instance.StopSound(m_audioInstance.Value);
                m_audioInstance = null;
            }

            m_audioInstance = AudioManager.Instance.PlaySound2D(bundle.AudioEventRef);
        }
    }
}
