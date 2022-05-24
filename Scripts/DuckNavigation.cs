using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// TODO: literally finish it
public class DuckNavigation : MonoBehaviour
{
    [SerializeField] public Vector3 _destination;
    [SerializeField] private NavMeshAgent _agent;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void GoTo(Vector3 where)
    {
        _agent.destination = where;
    }
}
