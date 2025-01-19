using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
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

    // Input Actions Asset must contain these maps!
    const string actionMapPlayerControls = "Player Controls";

    private string _currentControlScheme;
    private string currentControlScheme { 
        get { return _currentControlScheme; } 
        set {
            _currentControlScheme = value;
            Debug.Log(" setting Control Scheme: "+_currentControlScheme);
        } 
    }

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
    }

    // Input Action Event Callbacks -----------------------------------------------------------------------------------
    public void OnMovement(InputAction.CallbackContext value)
    { // should be action of type Value and category vector 2 (see input action asset)
        Vector2 inputMovement = value.ReadValue<Vector2>();
        rawInputMovement = new(inputMovement.x, 0f, inputMovement.y);
    }

    // Input System Callbacks -----------------------------------------------------------------------------------------
    // Whenever the input controller changes (applicable only when the input is automatically changed)
    public void OnControlsChanged()
    {
        if (playerInput.currentControlScheme == currentControlScheme)
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
