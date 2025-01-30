using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using vrm;

public class GameManager : Singleton<GameManager> 
{
    [HideInInspector]
    public bool isPaused = false;

    public GameObject playerPrefab = null;
    public GameObject virtualCamera = null;

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
        virtualCamera.Follow = inScenePlayer.transform;
        virtualCamera.LookAt = t;
    }

    private void spawnPlayer()
    {
        inScenePlayer = Instantiate(playerPrefab);
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
