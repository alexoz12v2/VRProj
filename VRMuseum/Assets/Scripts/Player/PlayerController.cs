using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using FMOD.Studio;
using vrm;

public class PlayerController : MonoBehaviour
{
    [Header("Sub Behaviours")]
    public PlayerMovementBehaviours playerMovementBehaviours;

    [Header("Input Settings")]
    public PlayerInput playerInput = null;
    public float movementSmoothingSpeed = 1f;
    private Vector3 rawInputMovement = Vector3.zero;
    private Vector3 smoothInputMovement = Vector3.zero;

    private EventInstance _playerFootsteps;

    // Input Actions Asset must contain these maps!
    const string actionMapPlayerControls = "Player Controls";

    private string _currentControlScheme;
    private string currentControlScheme
    {
        get { return _currentControlScheme; }
        set
        {
            _currentControlScheme = value;
            Debug.Log(" setting Control Scheme: " + _currentControlScheme);
        }
    }

    // Event for desktop grabbable
    public event Action<InputAction.CallbackContext> GrabCallbackEvent;
    public event Action MovementBreak;

    private Action<GameState, GameState> onGameModeChanged;
    private bool noMovement = false;

    // called from a game manager as part of the game setup
    public void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
    {
        switch (change)
        { // TODO maybe this is overkill
            case InputDeviceChange.Removed:
            //case InputDeviceChange.Disconnected:
            //case InputDeviceChange.Reconnected:
            //case InputDeviceChange.Enabled:
            //case InputDeviceChange.Disabled:
            //case InputDeviceChange.UsageChanged:
            //case InputDeviceChange.ConfigurationChanged:
            //case InputDeviceChange.SoftReset:
            //case InputDeviceChange.HardReset:
            case InputDeviceChange.Added:
                var XRDevices = InputSystem.devices.Where(device => device is XRController || device is XRHMD).ToArray();
                playerInput.SwitchCurrentControlScheme("XR", XRDevices);
                break;
        }
    }

    public void SetupPlayer()
    {
        if (DeviceCheckAndSpawn.Instance.isXR)
        {
            InputSystem.onDeviceChange += OnDeviceChange;
        }
        // default is assigned in the inspector, in the playerInput Component
        currentControlScheme = playerInput.currentControlScheme;

        // call all subbehaviours setup functions
        playerMovementBehaviours.SetupBehaviour();
        onGameModeChanged = (oldState, newState) =>
        {
            if (oldState == 0)
                noMovement = true;
            if (newState == 0)
                noMovement = false;
        };


        GameManager.Instance.GameStateChanged += onGameModeChanged;
        GameManager.Instance.GameDestroy += () =>
        {
            GameManager.Instance.GameStateChanged -= onGameModeChanged;
        };
        _playerFootsteps = AudioManager.Instance.CreateInstance(FMODEvents.Instance.PlayerFootsteps);
    }

    // Input Action Event Callbacks -----------------------------------------------------------------------------------
    public void OnMovement(InputAction.CallbackContext value)
    { // should be action of type Value and category vector 2 (see input action asset)
        if (noMovement)
        { 
            rawInputMovement = Vector3.zero;
            MovementBreak?.Invoke();
        }
        else
        {
            Vector2 inputMovement = value.ReadValue<Vector2>();
            rawInputMovement = new(inputMovement.x, 0f, inputMovement.y);
            
            
            if (!rawInputMovement.Equals(Vector3.zero))
            {
                Debug.Log($"Play sound");
                PLAYBACK_STATE playbackState;
                _playerFootsteps.getPlaybackState(out playbackState);
                if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    _playerFootsteps.start();
                }

            }
            else
            {
                _playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (DeviceCheckAndSpawn.Instance.isXR)
            throw new SystemException("This Input Event is for desktop only!");

        GrabCallbackEvent?.Invoke(ctx);
    }

    // Input System Callbacks -----------------------------------------------------------------------------------------
    // Whenever the input controller changes (applicable only when the input is automatically changed)
    public void OnControlsChanged()
    {
        if (playerInput.currentControlScheme != currentControlScheme)
        {
            currentControlScheme = playerInput.currentControlScheme;
            InputActionRebindingExtensions.RemoveAllBindingOverrides(playerInput.currentActionMap);
        }
    }

    // automatically called when the device is "lost" (eg disconnected, run out of batteries and so on...)
    public void OnDeviceLost()
    { // TODO Logic here ...
    }
    public void OnDeviceRegained()
    { // TODO possibly coroutine logic here
    }

    // MonoBehaviour Lifecycle ----------------------------------------------------------------------------------------
    private void Update()
    {
        computeSmoothMovmentInput();
        // update movement here, while orientation is dictated by the camera
        updatePlayerMovement();
        // TODO animations
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // Movement Private stuff -----------------------------------------------------------------------------------------
    private void computeSmoothMovmentInput()
    {
        smoothInputMovement = Vector3.Lerp(smoothInputMovement, rawInputMovement, Time.deltaTime * movementSmoothingSpeed);
    }
    private void updatePlayerMovement()
    {
        playerMovementBehaviours.MovementDirection = smoothInputMovement;
    }
}
