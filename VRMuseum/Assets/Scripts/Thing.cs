using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(StudioEventEmitter))]
public class Thing : MonoBehaviour
{
    private StudioEventEmitter emitter;
    // Start is called before the first frame update
    void Start()
    {
        emitter = AudioManager.Instance.InitializeEventEmitter(FMODEvents.Instance.thingIdle, this.gameObject);
        emitter.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        emitter.Stop();
    }
}
