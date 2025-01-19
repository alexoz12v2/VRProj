using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.tvOS;

// https://discussions.unity.com/t/how-to-detect-if-headset-is-available-and-initialize-xr-only-if-true/799147/2
namespace vrm
{
    public class FailedXRLoaderInitializationException : Exception
    {
        public FailedXRLoaderInitializationException() : base("Couldn't Initialize XR Loader (OpenXR)") { }
        public FailedXRLoaderInitializationException(string message) : base(message) { }
        public FailedXRLoaderInitializationException(string message, Exception inner) : base(message, inner) { }
    }

    public class XRLoaderManager {
        private bool _xrSupported = false;
        private bool _xrSubsystemActive = false;

        public bool XRSupported { get { return _xrSupported; } }
        public bool XRSubSystemActive { get { return _xrSubsystemActive; } }

        public XRLoaderManager() 
        {
            initializeXRSubsystem();
        }

        // we need to make sure that XR is properly shut down, and bot `OnDestroy` and `OnApplicationExit`
        // hwve proven to be unreliable. Hence, G#'s garbage collector is a far better choice. It can be
        // used only on pure C# classes though
        ~XRLoaderManager()
        {
            Cleanup();
        }

        public void Cleanup()
        {
            // XR management plugin logic
            StopXR();
            Debug.Log("Destroying XR Loader");
            if (_xrSupported && UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader != null)
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }

        public void StartXR()
        {
            if (_xrSubsystemActive)
                return;
            // try again (maybe SteamVR was opened in a later moment)
            if (!_xrSupported)
            {
                initializeXRSubsystem();
                if (!_xrSupported)
                    throw new FailedXRLoaderInitializationException();
            }
            UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StartSubsystems();
            _xrSubsystemActive = true;
            // set XR controllers as current input system device
        }

        public void StopXR()
        {
            if (UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader != null &&_xrSubsystemActive)
            {
                UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StopSubsystems();
            }
        }

        private void initializeXRSubsystem()
        {
            var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
            if (xrManagerSettings.activeLoader != null)
                //throw new FailedXRLoaderInitializationException("XR Loader initialization already done. but it shouldn't be");
                xrManagerSettings.DeinitializeLoader();

            xrManagerSettings.InitializeLoaderSync();
            if (!xrManagerSettings.isInitializationComplete)
                throw new FailedXRLoaderInitializationException();

            _xrSupported = xrManagerSettings.activeLoader != null;
        }
    }

    // This object has to persist between scenes, and be instanciated once at application startup
    public class DeviceCheckAndSpawn : Singleton<DeviceCheckAndSpawn>
    {
        [Header("Prefabs to spawn")]
        public GameObject _xrControllerPrefab;  // XR Controller Prefab to spawn if OpenXR device is detected
        public GameObject _fallbackPrefab;      // Fallback Prefab to spawn if no OpenXR device is found

        [SerializeField] private bool _forceDesktop = false;
        private bool _startRun = false;


        private XRLoaderManager _XRManager = null;

        public bool isXR { get { return _XRManager != null && _XRManager.XRSupported && _XRManager.XRSubSystemActive; } }

        public void Initialize()
        {
            if (_startRun)
                return;
            _startRun = true;
            //  Set up XR environment to run manually (has to be on start as it depends on the graphics initialization)
            if (!_forceDesktop)
                _XRManager = new();

            if (!_forceDesktop && _XRManager.XRSupported)
            {
                _XRManager.StartXR();
                Debug.Log("XR Started with device .. TDO COMPLETE LOG");
                // spawn XR controller prefab and set main camera to it, enable XR bindings from InputSystem
            }
            else
            {
                // spawn Desktop controller prefab and set main camera to it, eneable Desktop bindings from Input System
                Debug.Log("There's no XR device here");
            }

            // Device debuggin
            InputSystem.onDeviceChange += (InputDevice device, InputDeviceChange change) =>
            {
                switch (change)
                {
                    case InputDeviceChange.Added:
                        {
                            Debug.Log("Added " + device);
                            var inputDevices = new List<UnityEngine.XR.InputDevice>();
                            UnityEngine.XR.InputDevices.GetDevices(inputDevices);
                            if (inputDevices.Count == 0)
                                Debug.LogError("Fdklfdjsalkfjdsalkfjskdlafjdsaklfjdsa");
                            foreach (var dev in inputDevices)
                            {
                                Debug.Log("Actual XR Devices: " + dev.name);
                            }
                            var XRDevices = InputSystem.devices.Where(device => 
                                device is UnityEngine.InputSystem.XR.XRController 
                                || device is UnityEngine.InputSystem.XR.XRController);
                            foreach (var dev in XRDevices)
                            {
                                Debug.Log("Input System XR devices: " + dev.name);
                            }
                        }
                        break;
                    case InputDeviceChange.Removed: Debug.Log("Removed "+device); break;
                    case InputDeviceChange.Disconnected: Debug.Log("Disconnected "+device); break;
                    case InputDeviceChange.Reconnected: Debug.Log("Reconnected "+device); break;
                    case InputDeviceChange.Enabled: Debug.Log("Enabled "+device); break;
                    case InputDeviceChange.Disabled: Debug.Log("Disabled "+device); break;
                    case InputDeviceChange.UsageChanged: Debug.Log("UsageChanged "+device); break;
                    case InputDeviceChange.ConfigurationChanged: Debug.Log("ConfigurationChanged "+device); break;
                    case InputDeviceChange.SoftReset: Debug.Log("SoftReset "+device); break;
                    case InputDeviceChange.HardReset: Debug.Log("HardReset "+device); break;
                }
            };
        }

        override protected void OnDestroyCallback() 
        {
            if (_XRManager != null)
            {
                _XRManager.Cleanup();
                _XRManager = null;
            }
        }

        void SpawnPrefab(GameObject prefab)
        {
            if (prefab != null)
            {
                // Instantiate prefab at the origin, or you can modify this to spawn at a custom position
                Instantiate(prefab, Vector3.zero, Quaternion.identity);
            }
            else
            {
                Debug.LogError("Prefab to spawn is not assigned!");
            }
        }
    }
}
