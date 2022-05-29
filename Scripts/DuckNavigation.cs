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

    private const float IDLE_WANDER_TIME = 6.0f;

    // [SerializeField] public Vector3 _destination;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private Transform[] _wanderPoints;
    [SerializeField] private Transform _exitPoint;

    private NavState _navState = NavState.WANDER;

    private float _idleTime = 0.0f;
    private System.Random _rnd = new System.Random();
    private float _targetAngle = 0.0f;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StateUpdate(_navState);
        FixFacingDirection();
    }


    void FixFacingDirection()
    {
        _modelTransform.localEulerAngles = new Vector3(
            _modelTransform.localEulerAngles.x,
            Mathf.LerpAngle(_modelTransform.localEulerAngles.y, _targetAngle + 90.0f, 0.1f),
            _modelTransform.localEulerAngles.z
        );

        _targetAngle = Vector3.Angle(transform.position, transform.position + _agent.velocity);
    }


    void StateUpdate(NavState state)
    {
        switch (_navState) {
            case NavState.WANDER: {
                if (Vector3.Distance(_agent.destination, transform.position) < 1.0f)
                {
                    _idleTime += Time.deltaTime;

                    if (_idleTime >= IDLE_WANDER_TIME)
                    {
                        _agent.destination = _wanderPoints[_rnd.Next() % _wanderPoints.Length].position;
                    }
                }

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


    void LoadState(NavState newState)
    {
        switch (newState)
        {
            case NavState.WANDER: {
                _idleTime = 0.0f;
                _agent.destination = _wanderPoints[_rnd.Next() % _wanderPoints.Length].position;

                break;
            }
        }
    }


    public void SwitchState(NavState newState)
    {
        _navState = newState;

        LoadState(newState);
    }
}
