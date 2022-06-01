using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;


// TODO: literally finish it
public class DuckNavigation : MonoBehaviour
{
    public enum NavState {
        IDLE,               // no move dawg
        WANDER,             // go to random location
        CHASE,              // go to player's last seen location
        CHECK,              // we lost player, look around area
        ALERT,              // go to alert location
        EXIT                // go to level exit
    }

    private const float IDLE_WANDER_TIME = 2.0f;

    // [SerializeField] public Vector3 _destination;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private PlayerController _player;
    [SerializeField] private Transform[] _wanderPoints;
    [SerializeField] private Transform _exitPoint;
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private Transform _eyesTransform;
    [SerializeField] private Animator _anim;

    private NavState _navState = NavState.IDLE;

    private float _idleTime = 0.0f;
    private System.Random _rnd = new System.Random();
    private float _targetAngle = 0.0f;


    void Update()
    {
        StateUpdate(_navState);
        FixFacingDirection();
    }


    void FixFacingDirection()
    {
        _modelTransform.localEulerAngles = new Vector3(
            _modelTransform.localEulerAngles.x,
            _targetAngle + 90.0f,
            // Mathf.LerpAngle(_modelTransform.localEulerAngles.y, _targetAngle + 90.0f, 0.9f),
            _modelTransform.localEulerAngles.z
        );

        _targetAngle = Vector3.Angle(Vector3.zero, _agent.velocity);
    }


    void StateUpdate(NavState state)
    {
        switch (_navState) {
            case NavState.IDLE: {
                if (_idleTime >= IDLE_WANDER_TIME)
                {
                    SwitchState(NavState.WANDER);
                }
                _idleTime += Time.deltaTime;

                RaycastHit hit;
                if (Physics.Linecast(_eyesTransform.position, _player.transform.position, out hit))
                {
                    if (hit.collider.GetComponent<PlayerController>())
                    {
                        SwitchState(NavState.CHASE);
                    }
                }

                break;
            }


            case NavState.WANDER: {
                // TODO: custom movement scheme
                // ^^ move on step, not continuously.
                // ^^ possible plan of action:
                // ^^ 1) get next path point,
                // ^^ 2) move in direction based on time since state,
                // ^^ 3) profit.
                // ^^ Do this for NavState.CHASE too.

                RaycastHit hit;
                if (Physics.Linecast(_eyesTransform.position, _player.transform.position, out hit))
                {
                    if (hit.collider.GetComponent<PlayerController>())
                    {
                        // TODO: potential angle check?
                        // ^^ currently it works as the line collides with the duck's body, though
                        // ^^ something to look into and consider.

                        SwitchState(NavState.CHASE);
                    }
                }


                // Go to random wander points
                if (Vector3.Distance(_agent.destination, transform.position) < 1.0f)
                {
                    SwitchState(NavState.IDLE);
                }

                break;
            }

            case NavState.CHASE: {
                // We only want the duck to go to the player's last known location
                RaycastHit hit;
                if (Physics.Linecast(_eyesTransform.position, _player.transform.position, out hit) && hit.collider.GetComponent<PlayerController>())
                {
                    _agent.destination = _player.transform.position;
                }

                // Player is not in view
                else
                {
                    // We're nearby our destination, but we don't see the player.
                    if (Vector3.Distance(_agent.destination, transform.position) < 1.0f)
                    {
                        SwitchState(NavState.WANDER);
                    }
                }

                break;
            }

            case NavState.CHECK: {
                break;
            }

            case NavState.ALERT: {
                break;
            }

            case NavState.EXIT: {
                break;
            }
        }
    }


    void LoadState(NavState newState)
    {
        switch (newState)
        {
            case NavState.IDLE: {
                _anim.Play("Base Layer.Idle");

                _idleTime = 0.0f;

                break;
            }


            case NavState.WANDER: {
                _anim.Play("Base Layer.Walk");

                _agent.destination = _wanderPoints[_rnd.Next() % _wanderPoints.Length].position;

                break;
            }


            case NavState.CHASE: {
                _anim.Play("Base Layer.Run");

                break;
            }


            case NavState.EXIT: {
                _anim.Play("Base Layer.Run");

                _agent.destination = _exitPoint.position;

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
