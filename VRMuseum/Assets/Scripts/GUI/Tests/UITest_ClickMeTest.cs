using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace vrm
{
    public class UITest_ClickMeTest : MonoBehaviour
    {
        private GUIEvents _events = null;
        private InputAction _attackAction = null;

        private static string cleanStr(string input)
        {
            // Step 1: Remove excess whitespace and literal newlines
            string cleaned = Regex.Replace(input, @"\s+", " ").Trim();
            // Step 2: Insert newlines wherever \n is present
            string result = cleaned.Replace(@"\n", Environment.NewLine);
            return result;
        }

        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/
        private CanvasData _canvas = new CanvasData{
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
            var actionMap = InputSystem.actions.FindActionMap("Player", true);
            actionMap.Enable();
            _attackAction = actionMap.FindAction("Attack", true);
            _attackAction.Enable();
        }

        void Update()
        {
            // difference between this and isPressed is that this will be true only in the first 
            // frame after you pressed the button
            bool mouseDown = _attackAction.WasPressedThisFrame();
            if (mouseDown)
            {
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
}
