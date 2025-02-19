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
            if (GameManager.Instance != null && GameManager.Instance.player != null)
                Actions.Interact().performed -= OnInteract;
            PauseManager.Instance.OnPaused -= OnPaused;
            PauseManager.Instance.OnUnpaused -= OnUnpaused;
        }

        private void OnPaused()
        {
            Actions.Interact().performed -= OnInteract;
            if (m_audioInstance.HasValue && m_audioInstance.Value.isValid())
            {
                m_audioInstance.Value.getPlaybackState(out PLAYBACK_STATE state);
                if (state == PLAYBACK_STATE.PLAYING)
                    m_audioInstance.Value.setPaused(true);
            }
        }

        private void OnUnpaused()
        {
            Actions.Interact().performed += OnInteract;
            if (m_audioInstance.HasValue && m_audioInstance.Value.isValid())
                m_audioInstance.Value.setPaused(false);
        }

        private void Update()
        {
            if (m_ImageBundles.Count == 0 || PauseManager.Instance.Paused)
                return;

            UpdateSelectedBundle();

            if (m_audioInstance.HasValue && m_audioInstance.Value.isValid())
            {
                float perc = AudioManager.Instance.GetPlaybackPercentage(m_audioInstance.Value);
                // TODO REMOVE
                var comp = gameObject.GetComponent<ImGUIProgressBar>();
                if (comp != null)
                {
                    comp.Progress = perc;
                    if (perc >= 1f)
                        Methods.RemoveComponent<ImGUIProgressBar>(gameObject);
                }
            }
        }

        private void UpdateSelectedBundle()
        {
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
                .ToList();
            if (float.IsFinite(list[0].Item1))
            {
                GameObject go = list[0].bundle.Collider.gameObject;
                go.layer = (int)Layers.OutlineObject;
                list[0].bundle.Selected = true;
                for (int i = 1; i < list.Count; ++i)
                {
                    go = list[i].bundle.Collider.gameObject;
                    go.layer = (int)Layers.Default;
                    list[i].bundle.Selected = false;
                }
            }
            else 
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    GameObject go = list[i].bundle.Collider.gameObject;
                    go.layer = (int)Layers.Default;
                    list[i].bundle.Selected = false;
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
                // TODO REMOVE
                Methods.RemoveComponent<ImGUIProgressBar>(gameObject);
            }

            m_audioInstance = AudioManager.Instance.PlaySound2D(bundle.AudioEventRef);
            // TODO REMOVE
            gameObject.AddComponent<ImGUIProgressBar>();
        }

        // called by InsepctableObject or by any other logic which needs the audio to be stopped
        public void Cleanup()
        {
            if (m_audioInstance.HasValue && m_audioInstance.Value.isValid())
            {
                Debug.Log($"Stopping sound {m_audioInstance.Value}");
                AudioManager.Instance.StopSound(m_audioInstance.Value);
            }
            // TODO REMOVE
            Methods.RemoveComponent<ImGUIProgressBar>(gameObject);
        }
    }
}
