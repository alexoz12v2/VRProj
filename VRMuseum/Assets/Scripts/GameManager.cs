using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vrm;

public class GameManager : Singleton<GameManager> 
{
    [HideInInspector]
    public bool isPaused = false;

    public GameObject playerPrefab = null;

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
