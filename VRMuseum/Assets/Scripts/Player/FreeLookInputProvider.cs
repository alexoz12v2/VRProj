using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeLookInputProvider : MonoBehaviour, Cinemachine.AxisState.IInputAxisProvider
{
    //[Header("Player Input Source")]
    //public PlayerInput playerInput;

    private Vector3 _movement = new(0,0,0);
    public Vector3 Movement { 
        set {
            //Debug.Log("Movmenet changeed" + value);
            _movement = value; 
        } 
    }

    // `Monobehaviour` Lifecycle --------------------------------------------------------------------------------------
    //void Awake()
    //{
    //    if (playerInput == null)
    //        Debug.LogError("Cannot Find Player Input Asset component");
    //    else
    //    {
    //        if (playerInput.notificationBehavior != PlayerNotifications.InvokeUnityEvents)
    //            throw new System.Exception("Shouldn't thsi be Unity Event?");
    //        // playerInput.onActionTriggered only for CsEvent
    //    }
    //}

    // `Cinemachine.AxisState.IsnputAxisProvider` interface------------------------------------------------------------
    public float GetAxisValue(int axis)
    {
        switch (axis) {
            case 0: // x
            case 1: // y
            case 2: // z
                return _movement[axis];
            default:
                throw new System.Exception("You shouldn't be here");
        }
    }
}
