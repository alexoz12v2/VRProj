using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace vrm
{
    public class UITest_ClickMeTest : MonoBehaviour
    {
        [SerializeField] private float _heightOffset = 10f;

        [Header("Raycast Configuration")]
        [SerializeField] private float _centerRadius = 100f;  // Radius of the circle around the screen center where raycast is valid
        [SerializeField] private bool showDebugLines = true;  // Flag to enable/disable debug line rendering
        [SerializeField] private string _key;

        private bool _uiSelected = false;

        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/
 

        void Update()
        {
            // Only process if not in XR mode
            if (!DeviceCheckAndSpawn.Instance.isXR)
            {
                bool mouseDown = Mouse.current.leftButton.wasPressedThisFrame;

                // Get the object position in world space
                Vector3 objectWorldPosition = transform.position;

                // Project the object position to screen space
                Vector3 objectScreenPosition = Camera.main.WorldToScreenPoint(objectWorldPosition);

                // Get the center of the screen
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

                // Check if the object's screen position is within the center region (radius) from the screen center
                Ray ray = Camera.main.ScreenPointToRay(screenCenter);
                if (Vector2.Distance(new Vector2(objectScreenPosition.x, objectScreenPosition.y), screenCenter) <= _centerRadius)
                {
                    Func<RaycastHit, Vector3> position = hit => hit.transform.position + Vector3.up;
                    Func<RaycastHit, Quaternion> rotation = hit => Quaternion.Inverse(Camera.main.transform.rotation);
                    castRayAndDispatchCanvasEvent(mouseDown, ray, position, rotation);

                }
                else if (!_uiSelected)
                {
                    gameObject.SetLayerRecursively((int)Layers.Default);
                }
            }
        }

        // TODO: (Desktop) Somehow the raycast hits something when the camera points in the opposite direction
        private void castRayAndDispatchCanvasEvent(bool mouseDown, Ray ray, Func<RaycastHit, Vector3> positionFromHit, Func<RaycastHit, Quaternion> rotationFromHit)
        {
            
            // Raycast from mouse position on the screen
            if (Physics.Raycast(ray, out RaycastHit hit, 30))
            {
                Vector3 position = positionFromHit(hit);
                Quaternion rotation = rotationFromHit(hit);

                gameObject.SetLayerRecursively((int)Layers.Outlined);

                if (mouseDown)
                {
                    GUIEvents.Instance.OnGUIDisplayRequest.Invoke(GUIDictionary.Instance.GetCanvasDataByKey(_key), position, rotation);
                    _uiSelected = true;
                }
            }
            else if (!_uiSelected)
            {
                gameObject.SetLayerRecursively((int)Layers.Default);
            }

            // Render the debug ray if the flag is set
            if (showDebugLines)
            {
                Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 0.1f); // Draw a red ray for 0.1 seconds
            }
        }

        /*
        public void OnBeginHover(HoverEnterEventArgs args)
        {
            Ray ray = new Ray(args.interactorObject.transform.position, args.interactorObject.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                gameObject.SetLayerRecursively((int)Layers.Outlined);
            }
        }

        public void OnExitHover(HoverExitEventArgs args)
        {
            Ray ray = new Ray(args.interactorObject.transform.position, args.interactorObject.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                gameObject.SetLayerRecursively((int)Layers.Default);
            }
        }
        */

        public void OnActivateXR(SelectEnterEventArgs args)
        {
            Debug.Log("SHOTS FIRED");

            Ray ray = new Ray(args.interactorObject.transform.position, args.interactorObject.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 position = hit.transform.position + Vector3.up * _heightOffset;
                Quaternion rotation = Quaternion.Inverse(Camera.main.transform.rotation);
                GUIEvents.Instance.OnGUIDisplayRequest.Invoke(GUIDictionary.Instance.GetCanvasDataByKey(_key), position, rotation);
            }
        }
    }
}
