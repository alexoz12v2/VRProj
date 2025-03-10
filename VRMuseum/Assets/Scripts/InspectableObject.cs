using FMOD;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace vrm
{
    public class InspectableObject : MonoBehaviour
    {
        [SerializeField] private GameObject m_Images = null;
        [SerializeField] private float m_MaxInteractionDistance = 4f;
        [SerializeField] private string m_InteractableName = null;

        private int m_StartRenderLayer;
        private bool m_Selected = false;

        public System.Action Inspected;

        private void Start()
        {
            PauseManager.Instance.OnPaused += OnPaused;
            PauseManager.Instance.OnUnpaused += OnUnpaused;
            RegisterInput();
            m_StartRenderLayer = gameObject.layer;
            if (m_Images == null)
                UnityEngine.Debug.LogError($"Expected at least one bundle on {gameObject.name}");
        }

        private void OnDestroy()
        {
            if (m_Images.TryGetComponent<NearestChildCollision>(out var comp))
                comp.Cleanup();
            InputAction action = null;
            if (GameManager.Exists)
            {
                if (m_Selected && (action = Actions.Deselect()) != null)
                {
                    action.performed -= OnDeselect;
                }
                else if ((action = Actions.Interact()) != null)
                {
                    action.performed -= OnInteract;
                }
            }

            if (PauseManager.Exists)
            {
                PauseManager.Instance.OnPaused -= OnPaused;
                PauseManager.Instance.OnUnpaused -= OnUnpaused;
            }
        }

        private void OnPaused()
        {
            if (m_Selected)
            {
                Actions.Deselect().performed -= OnDeselect;
            }
            else
            {
                Actions.Interact().performed -= OnInteract;
            }
        }

        private void OnUnpaused()
        {
            if (m_Selected)
            {
                Actions.Deselect().performed += OnDeselect;
            }
            else
            {
                Actions.Interact().performed += OnInteract;
            }
        }

        private void RegisterInput()
        {
            if (DeviceCheckAndSpawn.Instance.isXR)
            {
                throw new System.NotImplementedException();
            }
            else
            {// TODO Possibly: use a trigger collider to register the action only within the trigger collider
                Actions.Interact().performed += OnInteract;
            }
        }

        private void Update()
        {
            if (PauseManager.Instance.Paused)
                return;

            if (!DeviceCheckAndSpawn.Instance.isXR)
            {
                if (!m_Selected)
                {
                    Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
                    if (AtLeastOneColliderRaycast(ray, GetColliders()))
                    {
                        gameObject.SetLayerRecursively((int)Layers.OutlineObject);
                    }
                    else
                    {
                        gameObject.SetLayerRecursively(m_StartRenderLayer);
                    }
                }
                else if (Vector3.Distance(Camera.main.transform.position, transform.position) > m_MaxInteractionDistance)
                {
                    Deselect();
                }
            }
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
            if (AtLeastOneColliderRaycast(ray, GetColliders()))
            {
                // deselect other object, if any
                if (GameManager.Instance.SelectedObject != null)
                {
                    GameManager.Instance.SelectedObject.Deselect();
                    GameManager.Instance.SelectedObject = null;
                }
                DebugPrintXRDevices.Instance.AddMessage($"Interazione con {(m_InteractableName ?? gameObject.name)}");
                gameObject.SetLayerRecursively((int)Layers.OutlineObject);
                m_Images.SetActive(true);
                m_Selected = true;
                Actions.Deselect().performed += OnDeselect;
                Actions.Interact().performed -= OnInteract;
                Inspected?.Invoke();
                GameManager.Instance.SelectedObject = this;
            }
        }

        private void OnDeselect(InputAction.CallbackContext context)
        {
            Deselect();
        }

        private void Deselect()
        {
            if (m_Images.TryGetComponent<NearestChildCollision>(out var comp))
                comp.Cleanup();
            m_Images.SetActive(false);
            gameObject.SetLayerRecursively(m_StartRenderLayer);
            m_Selected = false;
            Actions.Interact().performed += OnInteract;
            Actions.Deselect().performed -= OnDeselect;
            GameManager.Instance.SelectedObject = null;
        }

        private IList<Collider> GetColliders()
        {
            if (gameObject.TryGetComponent<Collider>(out var collider))
                return new SingletonList<Collider>(collider);
            else
            {
                var list = new List<Collider>();
                Methods.ForEachChildWith(gameObject, child => child.TryGetComponent<Collider>(out var x) && child.TryGetComponent<MeshRenderer>(out var y), child => list.Add(child.GetComponent<Collider>()));
                return list;
            }
        }

        private bool AtLeastOneColliderRaycast(Ray ray, IList<Collider> list)
        {
            if (list.Count == 0)
                return false;
            return list.Aggregate(false, (acc, x) => acc || x.Raycast(ray, out RaycastHit hit, m_MaxInteractionDistance));
        }
    }
}
