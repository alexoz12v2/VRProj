using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using vrm;

public class GameManager : Singleton<GameManager> 
{
    [HideInInspector]
    public bool isPaused = false;

    public GameObject playerPrefab = null;
    public GameObject virtualCamera = null;
    public Vector3 startPosition = new(0, 0, 0);
    public Quaternion startRotation = Quaternion.identity;

    [HideInInspector]
    public GameObject inScenePlayer = null;
    [HideInInspector]
    public PlayerController player = null;

    void Start()
    {
        DeviceCheckAndSpawn.Instance.Initialize();
        isPaused = false;
        spawnPlayer();
        setupPlayer();

        Transform t = null;
        if (DeviceCheckAndSpawn.Instance.isXR)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (GameObject obj in objects) 
            {
                var component = obj.GetComponent<TrackedPoseDriver>();
                if (component != null)
                    t = obj.transform;
            }
        }
        else
        {
            t = inScenePlayer.transform;
        }
        var virtualCamera = this.virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCameraBase>();

        var socket = Methods.FindChildWithTag(inScenePlayer, "CameraSocket");

        virtualCamera.Follow = socket.transform;
        Debug.Log($"Spawned player in position : x ={inScenePlayer.transform.position.x}, y = {inScenePlayer.transform.position.y}, z = {inScenePlayer.transform.position.z}");
        //virtualCamera.LookAt = t;// hard lock to taget doesn't require that 
    }

    private void OnGUI()
    {
        var rect = new Rect(Screen.width / 3, 10, Screen.width / 2, Screen.height / 10);
        GUI.TextField(rect, $"Player Position: x ={inScenePlayer.transform.position.x}, y = {inScenePlayer.transform.position.y}, z = {inScenePlayer.transform.position.z}");
    }

    private void spawnPlayer()
    {
        inScenePlayer = Instantiate(playerPrefab);
        var controller = inScenePlayer.GetComponent<CharacterController>();
        if (controller)
            controller.enabled = false;
        inScenePlayer.transform.position = startPosition;
        inScenePlayer.transform.rotation = startRotation;
        if (controller)
            controller.enabled = true;
        Debug.Log($"Spawned player in position : x ={inScenePlayer.transform.position.x}, y = {inScenePlayer.transform.position.y}, z = {inScenePlayer.transform.position.z}");
        player = inScenePlayer.GetComponent<PlayerController>();
    }

    private void setupPlayer()
    {
        player.SetupPlayer();
    }

    protected override void OnDestroyCallback()
    {
    }
}
