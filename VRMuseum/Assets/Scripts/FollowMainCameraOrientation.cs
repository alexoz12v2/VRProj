using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMainCameraOrientation : MonoBehaviour
{
    // https://simple.wikipedia.org/wiki/Pitch,_yaw,_and_roll
    [Header("Follow Axes (Typical Config: Yaw Only or Yaw+Pitch)")]
    [SerializeField] bool _followYaw = true;
    [SerializeField] bool _followPitch = false;
    [SerializeField] bool _followRoll = false;

    private Vector3 _lastRotation = new(0, 0, 0);

    void Update()
    {
        if (Camera.main == null) return;

        // Get the camera's world rotation
        Vector3 targetRotation = Camera.main.transform.rotation.eulerAngles;

        // Only rotate if the rotation difference is significant enough
        if (Vector3.Distance(targetRotation, _lastRotation) > 0.1f)
        {
            transform.localEulerAngles = withDisabledAxes(targetRotation);
        }
    }

    private Vector3 withDisabledAxes(Vector3 angles)
    {
        return new(_followPitch ? angles.x : 0, _followYaw ? angles.y : 0, _followRoll ? angles.z : 0);
    }
}
