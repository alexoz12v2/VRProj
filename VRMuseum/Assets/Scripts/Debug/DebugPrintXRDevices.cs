using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DebugPrintXRDevices : MonoBehaviour
{
    public float NotificationShowSeconds = 5f;
    private List<string> _devMsgs = new();
    private bool _devShowMsg = false;

    // MonoBehaviour Lifecycle ----------------------------------------------------------------------------------------
    private void Start()
    {
        UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
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

    private void OnDestroy()
    {
        UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // Private stuff --------------------------------------------------------------------------------------------------
    private IEnumerator waitFor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    private void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, UnityEngine.InputSystem.InputDeviceChange change)
    {
        _devMsgs.Add("");
        switch (change)
        {
            case UnityEngine.InputSystem.InputDeviceChange.Added:
                _devMsgs[_devMsgs.Count-1] = "Added Device: ";
                break;
            case UnityEngine.InputSystem.InputDeviceChange.Removed:
                _devMsgs[_devMsgs.Count-1] = "Removed Device; ";
                break;
            case UnityEngine.InputSystem.InputDeviceChange.Disconnected:
                _devMsgs[_devMsgs.Count-1] = "Disconnected Device; ";
                break;
            case UnityEngine.InputSystem.InputDeviceChange.Reconnected:
                _devMsgs[_devMsgs.Count-1] = "Reconnected Device: ";
                break;
            case UnityEngine.InputSystem.InputDeviceChange.Enabled:
                _devMsgs[_devMsgs.Count-1] = "Enabled Device: ";
                break;
            case UnityEngine.InputSystem.InputDeviceChange.Disabled:
                _devMsgs[_devMsgs.Count-1] = "Disabled Device: ";
                break;
            case UnityEngine.InputSystem.InputDeviceChange.UsageChanged:
                _devMsgs[_devMsgs.Count-1] = "UsageChanged Device: ";
                break;
            case UnityEngine.InputSystem.InputDeviceChange.ConfigurationChanged:
                _devMsgs[_devMsgs.Count-1] = "ConfigurationChanged Device: ";
                break;
            case UnityEngine.InputSystem.InputDeviceChange.SoftReset:
                _devMsgs[_devMsgs.Count-1] = "SoftReset Device: ";
                break;
            case UnityEngine.InputSystem.InputDeviceChange.HardReset:
                _devMsgs[_devMsgs.Count-1] = "HardReset Device: ";
                break;
        }
        _devMsgs[_devMsgs.Count-1] += device.displayName;
        if (!_devShowMsg)
        {
            new vrm.Task(waitFor(NotificationShowSeconds)).Finished += (manual) =>
            {
                _devShowMsg = false;
                _devMsgs.Clear();
            };
        }
        _devShowMsg = true;
    }
}
