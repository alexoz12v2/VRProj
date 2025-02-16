using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace vrm
{
    public class InspectableObject : MonoBehaviour
    {
        [SerializeField] private GameObject m_Images = null;
        [SerializeField] private float m_MaxInteractionDistance = 4f;
        private int m_StartRenderLayer;
        private bool m_Selected = false;
        private Transform m_Parent = null;

        private void Start()
        {
            PauseManager.Instance.OnPaused += OnPaused;
            PauseManager.Instance.OnUnpaused += OnUnpaused;
            GameManager.Instance.GameStartStarted += Registar;
            m_Parent = gameObject.transform.parent;
            m_StartRenderLayer = gameObject.layer;
            if (m_Images == null)
                Debug.LogError($"Expected at least one bundle on {gameObject.name}");
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

        private void Registar()
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
                    Collider collider = GetComponent<Collider>();
                    if (collider.Raycast(ray, out RaycastHit hit, m_MaxInteractionDistance))
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
            Collider collider = GetComponent<Collider>();
            if (collider.Raycast(ray, out RaycastHit hit, m_MaxInteractionDistance))
            {
                DebugPrintXRDevices.Instance.AddMessage($"Collider intersection with {gameObject.name}");
                gameObject.SetLayerRecursively((int)Layers.OutlineObject);
                m_Images.SetActive(true);

                // reparent and reset local rotation transforms
                m_Images.transform.parent = Camera.main.transform;
                foreach (Transform imageT in m_Images.transform)
                {
                    var imageO = imageT.gameObject;
                    //imageO.transform.localPosition = Vector3.zero + 0.2f * Vector3.forward;

                    Transform planeTransform = imageO.transform;
                    Transform camTransform = Camera.main.transform;

                    // Make the plane look at the camera
                    planeTransform.LookAt(camTransform.position);
                }

                m_Selected = true;
                Actions.Deselect().performed += OnDeselect;
                Actions.Interact().performed -= OnInteract;
            }
        }

        private void OnDeselect(InputAction.CallbackContext context)
        {
            Deselect();
        }

        private void Deselect()
        {
            m_Images.SetActive(false);
            gameObject.SetLayerRecursively(m_StartRenderLayer);
            gameObject.transform.parent = m_Parent;
            m_Selected = false;
            Actions.Interact().performed += OnInteract;
            Actions.Deselect().performed -= OnDeselect;
        }
    }
}
