using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using System;
using FMODUnity;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Linq;

namespace vrm
{
    public class DesktopManualCursorManagement : MonoBehaviour
    {
        private void Start()
        {
            OnApplicationFocus(true);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Methods.SetCursorFPSBehaviour();
            }
            else
            {
                Methods.ResetCursor();
            }
        }

        private void OnDestroy()
        {
            Methods.ResetCursor();
        }
    }

    [Flags]
    public enum GameState : int
    {
        Paused = 0x0000_0002,
        UIInteractable = 0x0000_0004,
        TinkerableInteractable = 0x0000_0008,
        TinkerableDecomposed = 0x0000_0010
    }

    public class GameManager : Singleton<GameManager>
    {
        [HideInInspector]
        public bool isPaused = false;

        public GameObject playerPrefab = null;
        public Cinemachine.CinemachineVirtualCamera VirtualCamera = null;
        public Transform StartTransform = null;
        [SerializeField] private string m_PlayerSceneName;

        [HideInInspector]
        public GameObject inScenePlayer = null;
        [HideInInspector]
        public PlayerController player = null;
        [HideInInspector]
        public InspectableObject SelectedObject = null;

        public event Action GameDestroy;
        public event Action<GameState, GameState> GameStateChanged;

        private GameState gameState;
        public GameState GameState
        {
            get { return gameState; }
            set
            {
                if (gameState == value)
                    throw new SystemException("GameState Shouldn't be changed with itself");
                GameStateChanged?.Invoke(gameState, value);
                gameState = value;
            }
        }

        void Awake()
        {
            isPaused = false;
            spawnPlayer();
            setupPlayer();
            AudioManager.Instance.Initialize();
            PauseManager.Instance.Register();

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
                // TODO hookup pause and unpause events
                gameObject.AddComponent<DesktopManualCursorManagement>();
                t = inScenePlayer.transform;
            }
            var socket = Methods.FindChildWithTag(inScenePlayer, "CameraSocket");

            VirtualCamera.Follow = socket.transform;
            Debug.Log($"Spawned player in position : x ={inScenePlayer.transform.position.x}, y = {inScenePlayer.transform.position.y}, z = {inScenePlayer.transform.position.z}");
            //virtualCamera.LookAt = t;// hard lock to taget doesn't require that 
            StudioListener listenerAudio = Camera.main.GetComponent<StudioListener>();
            var field = typeof(StudioListener).GetField("attenuationObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field.SetValue(listenerAudio, socket);

            PlayerSettingsManager.Instance.PlayerSettingsChanged += OnPlayerSettingsChanged;
            OnPlayerSettingsChanged(PlayerSettingsManager.Instance.PlayerSettings);
        }

        private void OnPlayerSettingsChanged(PlayerSettingsScriptableObject settings)
        {
            var pov = VirtualCamera.GetCinemachineComponent<Cinemachine.CinemachinePOV>();
            if (pov != null)
            {
                pov.m_HorizontalAxis.m_MaxSpeed = settings.MouseSensitivity.HorzSpeed;
                pov.m_VerticalAxis.m_MaxSpeed = settings.MouseSensitivity.VertSpeed;
            }

            player.playerMovementBehaviours.movementSpeed = settings.WalkingSpeed;
        }

        private void spawnPlayer()
        {
            if (StartTransform == null)
                throw new System.SystemException("Null Transform");
            inScenePlayer = Instantiate(playerPrefab);
            var controller = inScenePlayer.GetComponent<CharacterController>();
            if (controller)
                controller.enabled = false;

            if (Methods.IsSceneLoaded("Museum", out Scene scene))
            {
                Transform t = scene.GetRootGameObjects().Where(obj => obj.CompareTag("SpawnPoint")).First().transform;
                inScenePlayer.transform.SetPositionAndRotation(t.position, t.rotation);
            }
            else if (Methods.IsSceneLoaded("TrenchScene", out scene))
            { 
                Transform t = scene.GetRootGameObjects().Where(obj => obj.CompareTag("SpawnPointTrench")).First().transform;
                inScenePlayer.transform.SetPositionAndRotation(t.position, t.rotation);
            }

            if (!Methods.IsSceneLoaded(m_PlayerSceneName))
                throw new System.SystemException("shfsaf");
            Scene playerScene = SceneManager.GetSceneByName(m_PlayerSceneName);

            SceneManager.MoveGameObjectToScene(inScenePlayer, playerScene);
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
            if (PlayerSettingsManager.Exists)
                PlayerSettingsManager.Instance.PlayerSettingsChanged -= OnPlayerSettingsChanged;
            GameDestroy?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureSceneIsLoaded()
        {
            string sceneName = "Shared";

            if (!Methods.IsSceneLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }
        }
    }
}
