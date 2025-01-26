using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCharacterController : MonoBehaviour
{
    [SerializeField] private Transform _cameraT;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _mouseSensitivity = 100f;

    //[SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;
    //[SerializeField] private float _jumpHeight = 3f;

    private EventInstance _playerFootSteps;
    private CharacterController _characterController;
    private float cameraXRotation = 0f;
    private Vector3 _velocity;
    private Vector3 _move;
    private bool _isGrounded;

    void Start()
    {
        _playerFootSteps = AudioManager.Instance.CreateInstance(FMODEvents.Instance.playerFootsteps);
        _characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        UpdateCursor();

        if(Cursor.lockState == CursorLockMode.None)
            return;

        //Ground Check
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        //Compute direction According to Camera Orientation
        transform.Rotate(Vector3.up, mouseX);
        cameraXRotation -= mouseY;
        cameraXRotation = Mathf.Clamp(cameraXRotation, -90f, 90f);
        _cameraT.localRotation = Quaternion.Euler(cameraXRotation, 0f, 0f);


        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        _move = (transform.right * h + transform.forward * v).normalized;
        _characterController.Move(_move * _speed * Time.deltaTime);

        _characterController.Move(_velocity * Time.deltaTime);
        UpdateSound();
    }

    private void UpdateCursor()
    {
        if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(1))
            Cursor.lockState = CursorLockMode.Locked;

        if (Cursor.lockState == CursorLockMode.Locked && Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = CursorLockMode.None;
    }

    private void UpdateSound()
    {
        if (!_move.Equals(Vector3.zero))
        {
            Debug.Log("move is zero");
            PLAYBACK_STATE playbackState;
            _playerFootSteps.getPlaybackState(out playbackState);
            
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                Debug.Log("Play audio");
                _playerFootSteps.start();
            }
            

        }
        else
        {
            Debug.Log("Stop audio");
            _playerFootSteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }


}
