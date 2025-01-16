using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    public static CoroutineManager _instance;
    public static CoroutineManager Instance => _instance;

    private void Awake()
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

    public Coroutine StartManagedCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }

    public IEnumerator WaitForAllCoroutines(List<Coroutine> coroutines) 
    {
        foreach (Coroutine coroutine in coroutines)
        {
            yield return coroutine;
        }
    }
}
