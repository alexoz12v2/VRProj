using FMOD;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using vrm;

public class Explosion : MonoBehaviour
{
    private VisualEffect _VEexplosion;
    private bool _isLunched = false;

    private void Awake()
    {
        _VEexplosion = GetComponent<VisualEffect>();
        if (_VEexplosion == null)
            UnityEngine.Debug.LogError("VE not found");

    }

    void Start()
    {
        _VEexplosion.Stop(); 
    }



    // Update is called once per frame
    void Update()
    {
        if (!_isLunched)
        {
            StartCoroutine(LunchExplosion());    
        }
        
    }


    IEnumerator LunchExplosion()
    {
        _isLunched = true;
        int waitTime = Random.Range(3, 20);
        yield return new WaitForSeconds(waitTime);
        AudioManager.Instance.PlaySound3D(FMODEvents.Instance.Explosion, transform.position);
        _VEexplosion.Play();
        _isLunched = false;
    }
}
