using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrintDevices : MonoBehaviour
{
    private void OnGUI()
    {
        var list = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(list);
        foreach (var device in list)
        {
            /*GUILayout.Label($"XR Device: {device.GetType()} - {device.name}", EditorStyles.boldLabel);
            GUILayout.Space(10);*/
        }
    }
}
