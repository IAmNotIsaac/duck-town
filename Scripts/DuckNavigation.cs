using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;


// TODO: literally finish it
public class DuckNavigation : MonoBehaviour
{
    public enum NavState {
        START_PAUSE,        // pause at start to give player to load in
        IDLE,               // no move dawg
        WANDER,             // go to random location
        CHASE,              // go to player's last seen location
        CHECK,              // we lost player, look around area
        ALERT,              // go to alert location
        EXIT,               // go to level exit,
        CLAW                // launch claw at player
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
    [SerializeField] private Claw _claw;

    [HideInInspector] public bool playerInViewArea = false;

    private NavState _navState = NavState.START_PAUSE;
    private float _idleTime = 0.0f;
    private float _startTime = 0.0f;
    private System.Random _rnd = new System.Random();
    private float _targetAngle = 0.0f;
    private bool _exitOpen = false;
    private List<Vector3> _checkLocs = new List<Vector3>();


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
            case NavState.START_PAUSE: {
                if (_startTime >= 3.5f)
                {
                    SwitchState(NavState.WANDER);
                }
                _startTime += Time.deltaTime;

                break;
            }


            case NavState.IDLE: {
                if (_idleTime >= IDLE_WANDER_TIME)
                {
                    SwitchState(NavState.WANDER);
                }
                _idleTime += Time.deltaTime;

                RaycastHit hit;
                if (Physics.Linecast(_eyesTransform.position, _player.transform.position, out hit))
                {
                    if (hit.collider.GetComponent<PlayerController>() && playerInViewArea)
                    {
                        SwitchState(NavState.CHASE);
                    }
                }

                if (_exitOpen)
                {
                    SwitchState(NavState.EXIT);
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
                    if (hit.collider.GetComponent<PlayerController>() && playerInViewArea)
                    {
                        SwitchState(NavState.CHASE);
                    }
                }


                // Go to random wander points
                if (Vector3.Distance(_agent.destination, transform.position) < 1.0f)
                {
                    SwitchState(NavState.IDLE);
                }

                if (_exitOpen)
                {
                    SwitchState(NavState.EXIT);
                }

                break;
            }


            case NavState.CHASE: {
                // We only want the duck to go to the player's last known location
                RaycastHit hit;
                if (Physics.Linecast(_eyesTransform.position, _player.transform.position, out hit) && hit.collider.GetComponent<PlayerController>())
                {
                    _agent.destination = _player.transform.position;

                    // for some reason we have to add this and im just too tired to figure out why right now oh well
                    if (_agent.path.corners.Length > 1 && false == true) // temporarily deprecated, possibly forever
                    {
                        var lastPoint = _agent.path.corners[_agent.path.corners.Length - 1];

                        if (Vector3.Distance(transform.position, _player.transform.position) < Claw.MAX_CHAIN_COUNT * Claw.NEW_CHAIN_DISTANCE && (
                            _player.transform.position.x != lastPoint.x ||
                            _player.transform.position.z != lastPoint.z ))
                        {
                            SwitchState(NavState.CLAW);
                        }
                    }
                }

                // Player is not in view
                else
                {
                    // We're nearby our destination, but we don't see the player.
                    if (Vector3.Distance(_agent.destination, transform.position) < 1.0f)
                    {
                        SwitchState(NavState.CHECK);
                    }
                }

                break;
            }


            case NavState.CLAW: {
                if (_claw.state == Claw.State.UNLAUNCHED)
                {
                    SwitchState(NavState.CHASE);
                }

                break;
            }


            case NavState.CHECK: {
                if (_checkLocs.Count == 0)
                {
                    SwitchState(NavState.WANDER);
                    break;
                }

                _agent.destination = _checkLocs[0];

                if (Vector3.Distance(_agent.destination, _eyesTransform.position) < 1.0f)
                {
                    _checkLocs.RemoveAt(0);
                }
                
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


            case NavState.ALERT: {
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


            case NavState.EXIT: {
                RaycastHit hit;
                if (Physics.Linecast(_eyesTransform.position, _player.transform.position, out hit))
                {
                    if (hit.collider.GetComponent<PlayerController>() && playerInViewArea)
                    {
                        SwitchState(NavState.CHASE);
                    }
                }

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


            case NavState.CLAW: {
                if (_player.state != PlayerController.PlayerState.CLIMB && _player.state != PlayerController.PlayerState.CLIMB_PREP)
                {
                    _claw.Launch(_player.transform.position);
                }

                break;
            }


            case NavState.CHECK: {
                _anim.Play("Base Layer.Run");

                var locCount = (int)Mathf.Max(2, _rnd.Next() % 6.0f);
                
                for (int i = 0; i < locCount; i++)
                {
                    Vector3 randomPoint = Random.insideUnitSphere * 8.0f;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        _checkLocs.Add(hit.position);
                    }
                }

                break;
            }


            case NavState.ALERT: {
                _anim.Play("Base Layer.Run");

                _agent.destination = _player.transform.position;

                break;
            }


            case NavState.EXIT: {
                _anim.Play("Base Layer.Run");

                _agent.destination = _exitPoint.position;
                _exitOpen = true;

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
