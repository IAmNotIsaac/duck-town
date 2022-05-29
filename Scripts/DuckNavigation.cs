using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// TODO: literally finish it
public class DuckNavigation : MonoBehaviour
{
    public enum NavState {
        WANDER,             // go to random location
        CHASE,              // go to player's last seen location
        ALERT,              // go to alert location
        EXIT                // go to level exit
    }

    // [SerializeField] public Vector3 _destination;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Transform[] _wanderPoints;
    [SerializeField] private Transform _exitPoint;

    private NavState _navState = NavState.WANDER;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (_navState) {
            case NavState.WANDER: {
                break;
            }

            case NavState.CHASE: {
                break;
            }

            case NavState.ALERT: {
                break;
            }

            case NavState.EXIT: {
                _agent.destination = _exitPoint.position;
                break;
            }
        }
    }


    public void SwitchState(NavState newState)
    {
        _navState = newState;
    }
}
