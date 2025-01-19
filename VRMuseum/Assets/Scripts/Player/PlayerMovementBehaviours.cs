using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Instead of using directly `RigidBody`, use a `CharacterComtroller` componwnt, which manipulates it for you
public class PlayerMovementBehaviours : MonoBehaviour
{
    [Header("Component References")]
    public Rigidbody playerRigidbody;

    [Header("Movement Settings")]
    public float movementSpeed = 3f;
    public float turnSpeed = 0.1f;

    private Camera mainCamera;
    private Vector3 movementDirection;

    // movement direction is updated by the master player controller
    public Vector3 MovementDirection { set { movementDirection = value; } }

    // called by the master player controller
    public void SetupBehaviour()
    { // TODO Choose main camera
    }

    // MonoBehaviour Lifecycle ----------------------------------------------------------------------------------------
    // notice how movement/physics code belong to `FixedUpdate` and not `Update`
    private void FixedUpdate()
    {
        Vector3 worldSpaceDirection = cameraDirection(movementDirection);
        MovePlayer(worldSpaceDirection);
        RotatePlayer(worldSpaceDirection);
    }

    // Movement Private stuff -----------------------------------------------------------------------------------------
    void MovePlayer(Vector3 wsDirection)
    {
        Vector3 movement = movementSpeed * Time.fixedDeltaTime * wsDirection;
        // works only if rigid body is in the parent of the player hierarchy
        playerRigidbody.MovePosition(transform.position + movement);
    }

    void RotatePlayer(Vector3 wsDirection)
    { 
        if (movementDirection.sqrMagnitude > 0.01f)
        {
            Quaternion rotation = Quaternion.Slerp(
                playerRigidbody.rotation, 
                Quaternion.LookRotation(wsDirection),
                turnSpeed);
            playerRigidbody.MoveRotation(rotation);
        }
    }

    // computations of a world space movement direction from a camera space movement direction
    private Vector3 cameraDirection(Vector3 movementDirection)
    { // TODO when camera is properly setup
        return movementDirection;
    }
}
