using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace vrm
{
    public class DebugPrintXRDevices : Singleton<DebugPrintXRDevices>
    {
        public float NotificationShowSeconds = 5f;
        private List<string> _devMsgs = new();
        private bool _devShowMsg = false;

        private Task clearMessages;

        // MonoBehaviour Lifecycle ----------------------------------------------------------------------------------------
        private void Start()
        {
            UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
            clearMessages = new Task(ClearArrayRoutine());
        }

        private void OnGUI()
        {
            var list = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevices(list);
            foreach (var device in list)
            {
                GUILayout.Label($"XR Device: {device.GetType()} - {device.name}", EditorStyles.boldLabel);
                GUILayout.Space(10);
            }
            if (_devShowMsg)
            {
                float offset = 0f;
                foreach (var msg in _devMsgs)
                {
                    var style = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleRight,
                        fontSize = 10,
                        fontStyle = FontStyle.Bold,
                        normal = { textColor = Color.white }
                    };
                    var rect = new Rect(Screen.width - 500, 10 + offset, 290, 25); // Adjust width and position
                    GUI.Label(rect, msg, style);
                    offset += 12f;
                }
            }
        }

        // Public stuff --------------------------------------------------------------------------------------------------
        public void AddMessage(string message)
        {
            _devMsgs.Add(message);
            _devShowMsg = true;
        }

        // Private stuff --------------------------------------------------------------------------------------------------
        IEnumerator ClearArrayRoutine()
        {
            while (true) // Run infinitely
            {
                yield return new WaitForSeconds(5f); // Wait for 5 seconds

                // Clear the array
                _devMsgs.Clear();
                _devShowMsg = false;
            }
        }

        private void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, UnityEngine.InputSystem.InputDeviceChange change)
        {
            string msg = change switch
            {
                UnityEngine.InputSystem.InputDeviceChange.Added => "Added Device: " + device.displayName,
                UnityEngine.InputSystem.InputDeviceChange.Removed => "Removed Device, " + device.displayName,
                UnityEngine.InputSystem.InputDeviceChange.Disconnected => "Disconnected Device, " + device.displayName,
                UnityEngine.InputSystem.InputDeviceChange.Reconnected => "Reconnected Device: " + device.displayName,
                UnityEngine.InputSystem.InputDeviceChange.Enabled => "Enabled Device: " + device.displayName,
                UnityEngine.InputSystem.InputDeviceChange.Disabled => "Disabled Device: " + device.displayName,
                UnityEngine.InputSystem.InputDeviceChange.UsageChanged => "UsageChanged Device: " + device.displayName,
                UnityEngine.InputSystem.InputDeviceChange.ConfigurationChanged => "ConfigurationChanged Device: " + device.displayName,
                UnityEngine.InputSystem.InputDeviceChange.SoftReset => "SoftReset Device: " + device.displayName,
                UnityEngine.InputSystem.InputDeviceChange.HardReset => "HardReset Device: " + device.displayName,
                _ => throw new SystemException("Unexpected device change argument"),
            };
            AddMessage(msg);
        }

        protected override void OnDestroyCallback()
        {
            UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
        }
    }
}
