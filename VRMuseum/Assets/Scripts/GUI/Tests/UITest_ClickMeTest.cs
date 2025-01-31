using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace vrm
{
    public class UITest_ClickMeTest : MonoBehaviour
    {
        private GUIEvents _events = null;
        [SerializeField] private float _heightOffset = 10f;

        private static string cleanStr(string input)
        {
            // Step 1: Remove excess whitespace and literal newlines
            string cleaned = Regex.Replace(input, @"\s+", " ").Trim();
            // Step 2: Insert newlines wherever \n is present
            string result = cleaned.Replace(@"\n", Environment.NewLine);
            return result;
        }

        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/
        private CanvasData _canvas = new CanvasData
        {
            MaxWidth = 2,
            MaxHeight = 3,
            Title = "Fiat Revelli Mod. 1914",
            Paragraphs = new List<ParagraphData> {
                new(cleanStr(@"
                    La <b>Fiat-Revelli Mod. 1914<b> e' stata una mitragliatrice media,
                    adottata dal Regio Esercito Italiano nella prima guerra mondiale.
                "))
                {
                    Audio = "test.wav",
                    Title = "Introduzione"
                },
                new(cleanStr(@"
                    La <b>Fiat-Revelli Mod. 1914<b> e' stata una mitragliatrice media,
                    adottata dal Regio Esercito Italiano nella prima guerra mondiale.
                "))
                {
                    Audio = "test.wav",
                    Title = "Introduzione"
                }
            }
        };
        void Start()
        {
            _events = GUIEvents.Instance;
        }

        void Update()
        {
            // difference between this and isPressed is that this will be true only in the first 
            // frame after you pressed the button
            if (!DeviceCheckAndSpawn.Instance.isXR)
            {
                bool mouseDown = Mouse.current.leftButton.wasPressedThisFrame;
                if (mouseDown)
                {
                    var inputDevices = new List<UnityEngine.XR.InputDevice>();

                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Vector3 position = hit.transform.position + Vector3.up * _canvas.MaxHeight;
                        Quaternion rotation = Quaternion.Inverse(Camera.main.transform.rotation);
                        _events.OnGUIDisplayRequest.Invoke(_canvas, position, rotation);
                    }
                }
            }
        }

        public void OnActivateXR(SelectEnterEventArgs args)
        {
            Debug.Log("SHOTS FIRED");

            Ray ray = new Ray(args.interactorObject.transform.position, args.interactorObject.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 position = hit.transform.position + Vector3.up * _heightOffset * _canvas.MaxHeight;
                Quaternion rotation = Quaternion.Inverse(Camera.main.transform.rotation);
                _events.OnGUIDisplayRequest.Invoke(_canvas, position, rotation);
            }
        }
    }
}
