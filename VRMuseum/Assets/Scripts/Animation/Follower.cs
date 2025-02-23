using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Follower : MonoBehaviour
{
    public PathCreator pathCreator;
    [SerializeField] private float _speed = 1.0f;
    private float _distanceTravelled;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _distanceTravelled += _speed * Time.deltaTime;
        transform.position = pathCreator.path.GetPointAtDistance(_distanceTravelled);
        transform.rotation = pathCreator.path.GetRotationAtDistance(_distanceTravelled);
    }
}
