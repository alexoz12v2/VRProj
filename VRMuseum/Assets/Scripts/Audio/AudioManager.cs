using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

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
        [Range(0, 1)]
        public float VoiceVolume = 1;

        private HashSet<EventInstance> _eventInstances = new();
        private List<StudioEventEmitter> _eventEmitters = new();
        private EventInstance _background;
        private EventInstance _ambientEventInstance;
        private EventInstance _forestEventInstance;

        private Bus _masterBus;
        private Bus _ambientBus;
        private Bus _sfxBus;
        private Bus _musicBus;
        private Bus _UIBus;
        private Bus _VoiceBus;

        // Unity MonoBehaviour Lifecycle ------------------------------------------------------------------------------
        private void Awake()
        {
            // get ha handle to all buses
            _masterBus = RuntimeManager.GetBus("bus:/");
            _ambientBus = RuntimeManager.GetBus("bus:/Ambient");
            _sfxBus = RuntimeManager.GetBus("bus:/SFX");
            _UIBus = RuntimeManager.GetBus("bus:/UI");
            _musicBus = RuntimeManager.GetBus("bus:/Music");
            _VoiceBus = RuntimeManager.GetBus("bus:/VoiceGroup");
        }

        private void Update()
        {
            // set current volume
            _masterBus.setVolume(MasterVolume);
            _ambientBus.setVolume(AmbientVolume);
            _sfxBus.setVolume(SFXVolume);
            _UIBus.setVolume(UIVolume);
            _musicBus.setVolume(MusicVolume);
            _VoiceBus.setVolume(VoiceVolume);

            // check for in flight event instances which should be terminated
            List<EventInstance> toRemove = new();
            foreach (var eventInstance in _eventInstances)
            {
                if (!eventInstance.isValid())
                {
                    toRemove.Add(eventInstance);
                }
                else
                {
                    eventInstance.getPlaybackState(out PLAYBACK_STATE state);
                    if (state == PLAYBACK_STATE.STOPPED)
                        toRemove.Add(eventInstance);
                }
            }

            foreach (var eventInstance in toRemove)
            {
                _eventInstances.Remove(eventInstance);
                eventInstance.release();
            }
        }

        // public Methods ---------------------------------------------------------------------------------------------
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
        public void StopAmbient()
        {

            var result =  _ambientEventInstance.stop(STOP_MODE.IMMEDIATE);
            if (result != FMOD.RESULT.OK)
            {
                Debug.Log($"Failed to stop event with result: {result}");
            }
            _ambientEventInstance.release();
            /*
            result = musicEventInstance.release();
            if (result != FMOD.RESULT.OK)
            {
                Debug.Log($"Failed to stop event with result: {result}");
            }*/
        }

        public void StartAmbient()
        {

            InitializeAmbient(FMODEvents.Instance.AmbientSound);

        }

        public void InitializeForest(EventReference ambient)
        {
            _forestEventInstance = CreateInstance(ambient);

            if (!_forestEventInstance.isValid())
            {
                Debug.LogError("Forest event instance is not valid.");
            }
            _forestEventInstance.start();

        }

        public void StartForest()
        {
            InitializeForest(FMODEvents.Instance.AmbientForest);

        }

        public void StopForest()
        {

            var result = _forestEventInstance.stop(STOP_MODE.IMMEDIATE);
            if (result != FMOD.RESULT.OK)
            {
                Debug.Log($"Failed to stop event with result: {result}");
            }
            _forestEventInstance.release();
        }

        public void SetAmbientParameter(string parameterName, float parameterValue)
        {
            _ambientEventInstance.setParameterByName(parameterName, parameterValue);
        }
        public void SetBackgroundParameter(string parameterName, float parameterValue)
        {
            _background.setParameterByName(parameterName, parameterValue);
        }

        public EventInstance PlaySound3D(EventReference sound, Vector3 position)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
            eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            eventInstance.start();

            _eventInstances.Add(eventInstance);

            return eventInstance;
        }

        public EventInstance PlaySound2D(EventReference sound)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(sound);

            eventInstance.set3DAttributes(new FMOD.ATTRIBUTES_3D { position = new FMOD.VECTOR { x = 0, y = 0, z = 0 } });
            eventInstance.start();
            _eventInstances.Add(eventInstance);

            return eventInstance;
        }

        public void StopSound(EventInstance eventInstance)
        {
            if (_eventInstances.Contains(eventInstance))
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                eventInstance.release();
                _eventInstances.Remove(eventInstance);
            }
        }

        public float GetPlaybackPercentage(EventInstance eventInstance)
        {
            if (!eventInstance.isValid())
                return 0f;

            // Get current position in milliseconds
            eventInstance.getTimelinePosition(out int currentPositionMs);

            // Get event description
            eventInstance.getDescription(out EventDescription eventDescription);

            // Get total length in milliseconds
            eventDescription.getLength(out int totalLengthMs);

            if (totalLengthMs > 0)
            {
                return (float)currentPositionMs / totalLengthMs;
            }

            return 0f;
        }

        // Private/Protected Methods ----------------------------------------------------------------------------------

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

        protected override void OnDestroyCallback()
        {
            CleanUp();
        }


    }
}