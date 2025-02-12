using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : Singleton<FMODEvents>
{
    /*[field: Header("Dissolve SFX")]
    [field: SerializeField] public EventReference dissolve { get; private set; }
    */
    [field: Header("Player footsteps SFX")]
    [field: SerializeField] public EventReference PlayerFootsteps { get; private set; }
    [field: Header("TrinceaSFX")]
    [field: SerializeField] public EventReference Trincea { get; private set; }
    [field: Header("Ambient Sound")]
    [field: SerializeField] public EventReference AmbientSound { get; private set; }

    protected override void OnDestroyCallback()
    {
    }
}
