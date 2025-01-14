using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLauncher : MonoBehaviour
{
    private static EventLauncher _instance;
    public static EventLauncher Instance => _instance;
    public event EventHandler EventToDissolve;
    // Start is called before the first frame update

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (EventToDissolve != null)
            {
                Debug.Log("Lunch Dissolvance Event");
            }
        }
    }


}
