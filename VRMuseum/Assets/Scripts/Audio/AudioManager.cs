using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    private List<EventInstance> _eventInstances;
    private List<StudioEventEmitter> _eventEmitters;
    private EventInstance _background;
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;

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

        _eventInstances = new List<EventInstance>();
        _eventEmitters = new List<StudioEventEmitter>();
    }


    private void Start()
    {
        InitializeBackground(FMODEvents.Instance.background);
    }
    public void PlayOneShot(EventReference sound, Vector3 worldPos) 
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateInstance(EventReference eventReference) 
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        _eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        _eventEmitters.Add(emitter);

        return emitter;
    }

    public void InitializeBackground(EventReference background) 
    {
        _background = CreateInstance(background);
        _background.start();
    }

    private void SetBackgroundParameter(string parameterName, float parameterValue) 
    {
        _background.setParameterByName(parameterName, parameterValue);
    }

    private void CleanUp()
    {
        foreach(EventInstance eventInstance in _eventInstances) 
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        foreach (StudioEventEmitter emitter in _eventEmitters)
        {
            emitter.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
