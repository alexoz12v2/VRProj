using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

using UnityEngine.SceneManagement;

public class EventSystemLoading : MonoBehaviour
{
    private static EventSystemLoading _instance;
    public static EventSystemLoading Instance => _instance;
    //solution with Task
    //public event Func<object, EventArgs, Task> Shutdown;
    public delegate IEnumerator CoroutineEventHandler();
    public event CoroutineEventHandler DissolveAndWait;
    public bool isBeenLoadScene = false;
    
    [SerializeField] private string _unloadSceneName;
    [SerializeField] private string _loadSceneName;
    private Scene _sceneToUnload;
    private Scene _sceneToLoad;
    private AsyncOperation _asyncLoadOperation;
    

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        
        _sceneToUnload = SceneManager.GetSceneByName(_unloadSceneName);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (DissolveAndWait != null)
            {
                Debug.Log("Lunch the event");
                StartCoroutine(Dissolver());
            }
        }
    }
    /*
    public async Task OnShutdown()
    {
        Func<object, EventArgs, Task> handler = Shutdown;

        //no subs
        if (handler == null)
        {
            return; 
        }

        Delegate[] invocationList = handler.GetInvocationList(); 
        Task[] handlerTask = new Task[invocationList.Length];

        for (int i = 0; i < invocationList.Length; i++)
        {
            handlerTask[i] = ((Func<object, EventArgs, Task>)invocationList[i])(this, EventArgs.Empty);
        }

        await Task.WhenAll(handlerTask);
    }
    */

    public IEnumerator Dissolver()
    {
        if (DissolveAndWait == null)
        {
            Debug.Log("No listerners");
            yield break;
        }

        List<Coroutine> activeCoroutines = new List<Coroutine>();

        foreach (CoroutineEventHandler listener in DissolveAndWait.GetInvocationList())
        {
            Coroutine coroutine = CoroutineManager.Instance.StartManagedCoroutine(listener());
            activeCoroutines.Add(coroutine);
        }

        yield return CoroutineManager.Instance.WaitForAllCoroutines(activeCoroutines);

        Debug.Log("dissolve done");
        Debug.Log($"Scene to load: {_loadSceneName}");
        Debug.Log($"Scene to unload: {_unloadSceneName}");
        _sceneToUnload = SceneManager.GetSceneByName(_unloadSceneName);
        //now can unload the current scene 
        if (!_sceneToUnload.IsValid() || !_sceneToUnload.isLoaded)
        {
            Debug.LogError("Scene to unload isn't valid");
        }

        //StartCoroutine(UnloadScene(_sceneToUnload));
        Debug.Log("Start unloading scene");
        SceneManager.UnloadSceneAsync(_sceneToUnload);
        StartCoroutine(LoadScene(_loadSceneName));
        SwapSceneToLoadAndUnload();
        
    }

    private IEnumerator LoadScene(string sceneToLoadName)
    {
        _sceneToLoad = SceneManager.GetSceneByName(sceneToLoadName);

        if (_sceneToLoad.IsValid() && _sceneToLoad.isLoaded)
        {
            Debug.LogError("Scene not vaild");
            yield break;
        }

        Debug.Log("Run async Loading");
        isBeenLoadScene = true;
        _asyncLoadOperation = SceneManager.LoadSceneAsync(sceneToLoadName, LoadSceneMode.Additive);
        while (!_asyncLoadOperation.isDone)
        { 
            yield return null;
        }
        _asyncLoadOperation = null;
        Scene loadedScene = SceneManager.GetSceneByName(sceneToLoadName);
        SceneManager.SetActiveScene(loadedScene);
    }

    private void SwapSceneToLoadAndUnload()
    {
        string tmp = _unloadSceneName;
        _unloadSceneName = _loadSceneName;
        _loadSceneName = tmp;
    }
}
