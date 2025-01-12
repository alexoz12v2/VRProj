using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DissolvingController : MonoBehaviour
{
    public SkinnedMeshRenderer[] skinnedMeshes;
    private List<Material[]> _skinnedMaterials = new List<Material[]>();
    [SerializeField] private  float _dissolveRate = 0.0125f;
    [SerializeField] private float _refreshRate = 0.025f;
    // Start is called before the first frame update
    private void Awake()
    {
        skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();

        if (skinnedMeshes != null)
        {
            for(int i = 0; i< skinnedMeshes.Length; i++)
                _skinnedMaterials.Add(skinnedMeshes[i].materials);
        }
        else { Debug.LogError("SkinnedMeshRenderers not found"); }

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Dissolve());
        }
    }

    IEnumerator Dissolve() 
    {
        if (_skinnedMaterials.Any())
        {
            float counter = 0;

            while (_skinnedMaterials[0][0].GetFloat("_DissolveAmount") < 1)
            {
                counter += _dissolveRate;
                foreach (var skinnedMaterials in _skinnedMaterials)
                {
                    for (int i = 0; i < skinnedMaterials.Length; i++)
                    {
                        skinnedMaterials[i].SetFloat("_DissolveAmount", counter);
                    }
                    yield return new WaitForSeconds(_refreshRate);
                }
            }
        }
    }
}
