using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vrm;

[RequireComponent(typeof(StudioEventEmitter))]
public class Emitter : MonoBehaviour
{

    private StudioEventEmitter emitter;
    // Start is called before the first frame update
    void Start()
    {
        emitter = AudioManager.Instance.InitializeEventEmitter(FMODEvents.Instance.Trincea, this.gameObject);
        emitter.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
