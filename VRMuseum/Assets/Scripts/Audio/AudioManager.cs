using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace vrm
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Volume")]
        
        [Range(0, 1)]
        public float MasterVolume = 1;
        [Range(0, 1)]
        public float AmbientVolume = 1;
        [Range(0, 1)]
        public float SFXVolume = 1;
        [Range(0, 1)]
        public float MusicVolume = 1;
        [Range(0, 1)]
        public float UIVolume = 1;


        private List<EventInstance> _eventInstances = new();
        private List<StudioEventEmitter> _eventEmitters = new();
        private EventInstance _background;
        private EventInstance _ambientEventInstance;

        private Bus _masterBus;
        private Bus _ambientBus;
        private Bus _sfxBus;
        private Bus _musicBus;
        private Bus _UIBus;

        public void Awake()
        {
            _masterBus = RuntimeManager.GetBus("bus:/");
            _ambientBus = RuntimeManager.GetBus("bus:/Ambient");
            _sfxBus = RuntimeManager.GetBus("bus:/SFX");
            _UIBus = RuntimeManager.GetBus("bus:/UI");
            _musicBus = RuntimeManager.GetBus("bus:/Music");
        }
        public void Initialize()
        {
            //InitializeBackground(FMODEvents.Instance.background);
            InitializeAmbient(FMODEvents.Instance.AmbientSound);
        }
        public void PlayOneShot(EventReference sound, Vector3 worldPos)
        {
            RuntimeManager.PlayOneShot(sound, worldPos);
        }

        public void PlayOneShot(EventReference sound)
        {
            RuntimeManager.PlayOneShot(sound);
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

        public void InitializeAmbient(EventReference ambient)
        {
            _ambientEventInstance = CreateInstance(ambient);
            _ambientEventInstance.start();
        }
        public void SetAmbientParameter(string parameterName, float parameterValue)
        {
            _ambientEventInstance.setParameterByName(parameterName, parameterValue);
        }
        public void SetBackgroundParameter(string parameterName, float parameterValue)
        {
            _background.setParameterByName(parameterName, parameterValue);
        }



        private void CleanUp()
        {
            foreach (EventInstance eventInstance in _eventInstances)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }

            foreach (StudioEventEmitter emitter in _eventEmitters)
            {
                emitter.Stop();
            }
        }
        private void Update()
        {
            _masterBus.setVolume(MasterVolume);
            _ambientBus.setVolume(AmbientVolume);
            _sfxBus.setVolume(SFXVolume);
            _UIBus.setVolume(UIVolume);
            _musicBus.setVolume(MusicVolume);
        }
        protected override void OnDestroyCallback()
        {
            CleanUp();
        }
    }
}