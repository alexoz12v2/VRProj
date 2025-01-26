using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Dissolve SFX")]
    [field: SerializeField] public EventReference dissolve { get; private set; }
    [field: Header("Player footsteps SFX")]
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: Header("Thing Idle SFX")]
    [field: SerializeField] public EventReference thingIdle { get; private set; }
    [field: Header("Background Music")]
    [field: SerializeField] public EventReference background { get; private set; }

    private static FMODEvents _instance;
    public static FMODEvents Instance => _instance;

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
}
