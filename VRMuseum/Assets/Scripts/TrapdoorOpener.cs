using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapdoorOpener : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private BoxCollider _portal;
    private bool _isOpen = false;
    // Start is called before the first frame update
    void Start()
    {
        //_animator = GetComponent<Animator>();

        if (_animator == null)
        {
            Debug.LogError("Error Animator not found");
        }
        _portal.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        Open();
    }

    private void OnTriggerExit(Collider collision)
    {
        Close();
    }

    public void Open()
    {
        _isOpen = true;
        _portal.enabled = true;
        _animator.SetBool("open", _isOpen);
    }

    public void Close()
    {
        _isOpen = false;
        _portal.enabled = false;
        _animator.SetBool("open", _isOpen);
    }
}
