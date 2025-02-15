using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using System;
using FMODUnity;
using UnityEngine.SceneManagement;

namespace vrm
{
    public class DesktopManualCursorManagement : MonoBehaviour
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;    // X position of upper-left corner
            public int Top;     // Y position of upper-left corner
            public int Right;   // X position of lower-right corner
            public int Bottom;  // Y position of lower-right corner
        }

        private bool isFocused = true;

        void OnApplicationFocus(bool hasFocus)
        {
            isFocused = hasFocus;

            if (hasFocus)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.None;  // We handle centering manually
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        void LateUpdate()
        {
            if (isFocused)
            {
                //CenterCursorInGameWindow();
            }
        }

        private void CenterCursorInGameWindow()
        {
            IntPtr windowHandle = GetActiveWindow();

            if (GetWindowRect(windowHandle, out RECT rect)) // not client area, but whatever
            {
                // Calculate the center of the game window
                int centerX = (rect.Left + rect.Right) / 2;
                int centerY = (rect.Top + rect.Bottom) / 2;

                SetCursorPos(centerX, centerY);
            }
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
        public GameObject virtualCamera = null;
        public Vector3 startPosition = new(0, 0, 0);
        public Quaternion startRotation = Quaternion.identity;

        [HideInInspector]
        public GameObject inScenePlayer = null;
        [HideInInspector]
        public PlayerController player = null;

        public event Action GameStartStarted;
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

        void Start()
        {
            isPaused = false;
            spawnPlayer();
            setupPlayer();
            AudioManager.Instance.Initialize();

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
            var virtualCamera = this.virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCameraBase>();

            var socket = Methods.FindChildWithTag(inScenePlayer, "CameraSocket");

            virtualCamera.Follow = socket.transform;
            Debug.Log($"Spawned player in position : x ={inScenePlayer.transform.position.x}, y = {inScenePlayer.transform.position.y}, z = {inScenePlayer.transform.position.z}");
            //virtualCamera.LookAt = t;// hard lock to taget doesn't require that 
            StudioListener listenerAudio = Camera.main.GetComponent<StudioListener>();
            var field = typeof(StudioListener).GetField("attenuationObject", System.Reflection.BindingFlags.Instance |System.Reflection.BindingFlags.NonPublic);
            field.SetValue(listenerAudio, socket);
            GameStartStarted?.Invoke();
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
            GameDestroy?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureSceneIsLoaded()
        {
            string sceneName = "Shared";

            if (!IsSceneLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    return true; // Scene is already loaded
                }
            }
            return false; // Scene is not loaded
        }
    }
}
