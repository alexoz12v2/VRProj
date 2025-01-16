using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class EventSystemLoading : MonoBehaviour
{
    private static EventSystemLoading _instance;

    public static EventSystemLoading Instance => _instance;
    //solution with Task
    //public event Func<object, EventArgs, Task> Shutdown;
    public delegate IEnumerator CoroutineEventHandler();
    public event CoroutineEventHandler DissolveAndWait;
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
    }
}
