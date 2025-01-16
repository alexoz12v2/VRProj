using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dissolver : MonoBehaviour
{
    public SkinnedMeshRenderer[] skinnedMeshes;
    public Renderer[] renderers;
    private List<Material[]> _materials = new List<Material[]>();
    [SerializeField] private float _dissolveRate = 0.0125f;
    [SerializeField] private float _refreshRate = 0.025f;

    void Awake()
    {
        skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();

        if (skinnedMeshes.Length != 0)
        {
            Debug.Log("SkinnedMeshRenderers found");
            Debug.Log(skinnedMeshes.Length);
            for (int i = 0; i< skinnedMeshes.Length; i++)
                _materials.Add(skinnedMeshes[i].materials);
        }
        else 
        { 
            Debug.Log("SkinnedMeshRenderers not found, searching the Renderer component"); 
            renderers = GetComponentsInChildren<Renderer>();

            if (renderers != null)
            {
                Debug.Log(renderers.Length);
                for (int i = 0; i< renderers.Length; i++)
                {
                    _materials.Add(renderers[i].materials);
                        Debug.LogError(_materials[i][0]);
                }
            }
            else
            {
                Debug.LogError("Rederer not found");
            }
        }   
    }
    void Start()
    {
        Debug.Log("I'm sub to event");
        EventLauncher.Instance.EventToDissolve += DissolveCo;
    }
    void DissolveCo(object sender, EventArgs args) 
    {
        StartCoroutine(Dissolve());
    }
    IEnumerator Dissolve()
    {
        Debug.Log("Run dissolve");
        if (_materials.Any())
        {
            Debug.Log("There are materials");
            float counter = 0;

            while (_materials[0][0].GetFloat("_DissolveAmount") < 1)
            {
                counter += _dissolveRate;
                foreach (var skinnedMaterials in _materials)
                {
                    for (int i = 0; i < skinnedMaterials.Length; i++)
                    {
                        skinnedMaterials[i].SetFloat("_DissolveAmount", counter);
                    }
                    yield return new WaitForSeconds(_refreshRate);
                }
            }

            //destroy the gameobject
            Destroy(gameObject);

        }
    }

}
