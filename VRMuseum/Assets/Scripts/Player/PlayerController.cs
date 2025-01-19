using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private string currentControlScheme;

    // called from a game manager as part of the game setup
    public void SetupPlayer()
    {
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
