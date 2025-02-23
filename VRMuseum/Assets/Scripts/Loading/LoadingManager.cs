using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;

namespace vrm
{
    public class LoadingManager : Singleton<LoadingManager>
    {
        public bool isBeenLoadScene = false;

        [SerializeField] private string _unloadSceneName;
        [SerializeField] private string _loadSceneName;
        [SerializeField] private float _intensityToBlank = 10;
        [SerializeField] private float _intensityToNormal = 0;
        [SerializeField] private float _timeToBlank = 1;
        public delegate IEnumerator cbDissolve();
        public event cbDissolve transactionEvent;
        private UnityEngine.SceneManagement.Scene _sceneToUnload;
        private UnityEngine.SceneManagement.Scene _sceneToLoad;
        private AsyncOperation _asyncLoadOperation;
        private Volume _volume;
        private Bloom _bloom;
        private float _refreshRate = 0.03f;
        private GameObject _player;
        private CharacterController _characterController;

        private float _rate;

        protected override void OnDestroyCallback()
        {

        }

        // Start is called before the first frame update
        void Start()
        {
            _rate = _intensityToBlank / (_timeToBlank / _refreshRate);
            _sceneToUnload = SceneManager.GetSceneByName(_unloadSceneName);
            //avoid tag 
            PostprocessingTarget PT = GameObject.FindObjectOfType<PostprocessingTarget>();
            _volume = PT.gameObject.GetComponent<Volume>();
            _volume.profile.TryGet(out _bloom);
            transactionEvent += Dissolve;
            //Get player prefab 
            PlayerController PCtarget = GameObject.FindObjectOfType<PlayerController>();
            _player = PCtarget.gameObject;
            _characterController = _player.GetComponent<CharacterController>();


        }

        public void PlayDissolving()
        {
            Debug.Log("Start dissolving process");
            if (transactionEvent != null)
                StartCoroutine(transactionEvent.Invoke());
            else Debug.Log("No listener for transaction event");
        }

        public void DissolveToMainMenu()
        {
            new Task(DissolveToMainMenuTask());
        }

        private IEnumerator DissolveToMainMenuTask()
        {
            Scene trenchScene = SceneManager.GetSceneByName("TrenchScene");
            Scene playerScene = SceneManager.GetSceneByName("PlayerScene");

            yield return blankScreen();
            if (trenchScene.IsValid() && trenchScene.isLoaded)
            {
                Debug.Log("Unloading TrenchScene");
                SceneManager.UnloadSceneAsync(trenchScene);
            }
            if (playerScene.IsValid() && playerScene.isLoaded)
            {
                Debug.Log("Unloading PlayerScene");
                SceneManager.UnloadSceneAsync(playerScene);
            }
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
            yield return restoreScreen();
        }

        public IEnumerator Dissolve()
        {
            _sceneToUnload = SceneManager.GetSceneByName(_unloadSceneName);

            if (!_sceneToUnload.IsValid() || !_sceneToUnload.isLoaded)
            {
                Debug.LogError("Scene to unload isn't valid");
            }

            yield return blankScreen();
            SceneManager.UnloadSceneAsync(_sceneToUnload);

            yield return SceneManager.LoadSceneAsync(_loadSceneName, LoadSceneMode.Additive);
            yield return null;
            GameObject spawnPoint;

            if (HUDManager.Exists)
                HUDManager.Instance.CleanHUDMessages();
            if (_loadSceneName.Equals("Museum"))
            {
                //AudioManager.Instance.StopForest();
                yield return new WaitForSeconds(0.1f);
                //AudioManager.Instance.StartAmbient();

                spawnPoint = GameObject.FindWithTag("SpawnPoint");
            }
            else
            {

                //AudioManager.Instance.StopAmbient();
                yield return new WaitForSeconds(0.1f);
                //AudioManager.Instance.StartForest();
                spawnPoint = GameObject.FindWithTag("SpawnPointTrench");
            }

            Debug.Log($"Dissolving Spawn: Position: {spawnPoint.transform.position} Rotation: {spawnPoint.transform.rotation}");
            if (_player == null)
            {
                Debug.Log("Player is Null");
            }

            //Disable the characterController
            _characterController.enabled = false;
            _player.transform.SetPositionAndRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
            Debug.Log($"Player moved to: {_player.transform.position}, Rotation: {_player.transform.rotation}");
            // Re-enable the CharacterController
            _characterController.enabled = true;

            yield return restoreScreen();

            SwapSceneToLoadAndUnload();
        }

        public IEnumerator blankScreen()
        {
            float intesity = 0;

            while (intesity <= _intensityToBlank)
            {
                intesity += _rate;
                _bloom.intensity.value = intesity;
                yield return new WaitForSeconds(_refreshRate);
            }

        }
        public IEnumerator restoreScreen()
        {
            float intesity = _intensityToBlank;

            while (intesity > _intensityToNormal)
            {
                intesity -= _rate;
                _bloom.intensity.value = intesity;
                yield return new WaitForSeconds(_refreshRate);
            }


        }

        private void SwapSceneToLoadAndUnload()
        {
            string tmp = _unloadSceneName;
            _unloadSceneName = _loadSceneName;
            _loadSceneName = tmp;
        }


    }
}
