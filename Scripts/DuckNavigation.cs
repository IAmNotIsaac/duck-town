using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;


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
        CLAW,               // launch claw at player
        FINAL_WAIT,         // wait for trigger in final level
        FINAL_CHASE,        // chase after player in final level
        PLAYER_DEATH        // player died, no behaviour
    }

    const float IDLE_WANDER_TIME = 2.0f;
    const float WALK_SPEED = 3.5f;
    const float CHASE_SPEED = WALK_SPEED * 2.0f; 

    // [SerializeField] public Vector3 _destination;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private PlayerController _player;
    [SerializeField] private Transform[] _wanderPoints;
    [SerializeField] private Transform _exitPoint;
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private Transform _eyesTransform;
    [SerializeField] private Animator _anim;
    [SerializeField] private Claw _claw;
    [SerializeField] private AudioSource[] _stepSounds;

    [HideInInspector] public bool playerInViewArea = false;

    private NavState _navState = NavState.START_PAUSE;
    private float _idleTime = 0.0f;
    private float _startTime = 0.0f;
    private float _stepTime = 0.0f;
    private System.Random _rnd = new System.Random();
    private float _targetAngle = 0.0f;
    private bool _exitOpen = false;
    private List<Vector3> _checkLocs = new List<Vector3>();
    private bool _canDamagePlayer = false;


    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == Ambience.FINAL_LEVEL_ID)
        {
            SwitchState(NavState.FINAL_WAIT);
        }
    }


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


                if (_stepTime > 1.25f)
                {
                    _stepTime = 0.0f;
                    var r = _rnd.Next() % (_stepSounds.Length - 1);
                    _player.CameraShake(transform.position);
                    _stepSounds[r].Play();
                }
                _stepTime += Time.deltaTime;

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
                
                if (_canDamagePlayer)
                {
                    _player.Damage();
                    if (_player.health < 0.0f)
                    {
                        SwitchState(NavState.PLAYER_DEATH);
                    }
                }


                if (_stepTime > 1.0f)
                {
                    _stepTime = 0.0f;
                    var r = _rnd.Next() % (_stepSounds.Length - 1);
                    _player.CameraShake(transform.position);
                    _stepSounds[r].Play();
                }
                _stepTime += Time.deltaTime;

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
                    if (hit.collider.GetComponent<PlayerController>() && playerInViewArea)
                    {
                        SwitchState(NavState.CHASE);
                    }
                }


                if (_stepTime > 1.0f)
                {
                    _stepTime = 0.0f;
                    var r = _rnd.Next() % (_stepSounds.Length - 1);
                    _player.CameraShake(transform.position);
                    _stepSounds[r].Play();
                }
                _stepTime += Time.deltaTime;

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


                if (_stepTime > 1.0f)
                {
                    _stepTime = 0.0f;
                    var r = _rnd.Next() % (_stepSounds.Length - 1);
                    _player.CameraShake(transform.position);
                    _stepSounds[r].Play();
                }
                _stepTime += Time.deltaTime;
                
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


                if (_stepTime > 1.0f)
                {
                    _stepTime = 0.0f;
                    var r = _rnd.Next() % (_stepSounds.Length - 1);
                    _player.CameraShake(transform.position);
                    _stepSounds[r].Play();
                }
                _stepTime += Time.deltaTime;

                break;
            }


            case NavState.FINAL_CHASE: {
                _agent.destination = _player.transform.position;

                if (transform.position.y < -2.0f)
                {
                    _agent.speed = WALK_SPEED;
                }
                
                if (_canDamagePlayer)
                {
                    _player.Damage();
                    if (_player.health < 0.0f)
                    {
                        SwitchState(NavState.PLAYER_DEATH);
                    }
                }


                if (_stepTime > 1.0f)
                {
                    _stepTime = 0.0f;
                    var r = _rnd.Next() % (_stepSounds.Length - 1);
                    _player.CameraShake(transform.position);
                    _stepSounds[r].Play();
                }
                _stepTime += Time.deltaTime;

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
                _stepTime = 0.0f;
                _agent.speed = WALK_SPEED;
                _anim.Play("Base Layer.Walk");

                _agent.destination = _wanderPoints[_rnd.Next() % _wanderPoints.Length].position;

                break;
            }


            case NavState.CHASE: {
                _stepTime = 0.0f;
                _agent.speed = CHASE_SPEED;
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
                _stepTime = 0.0f;
                _agent.speed = CHASE_SPEED;
                _anim.Play("Base Layer.Run");

                var locCount = (int)Mathf.Max(2, _rnd.Next() % 4.0f);
                
                for (int i = 0; i < locCount; i++)
                {
                    Vector3 randomPoint = Random.insideUnitSphere * 2.0f;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        _checkLocs.Add(hit.position);
                    }
                }

                break;
            }


            case NavState.ALERT: {
                _stepTime = 0.0f;
                _agent.speed = CHASE_SPEED;
                _anim.Play("Base Layer.Run");

                _agent.destination = _player.transform.position;

                break;
            }


            case NavState.EXIT: {
                _stepTime = 0.0f;
                _agent.speed = CHASE_SPEED;
                _anim.Play("Base Layer.Run");

                _agent.destination = _exitPoint.position;
                _exitOpen = true;

                break;
            }


            case NavState.FINAL_CHASE: {
                _stepTime = 0.0f;
                _agent.speed = CHASE_SPEED;
                _anim.Play("Base Layer.Run");

                break;
            }
        }
    }


    public void SwitchState(NavState newState)
    {
        _navState = newState;

        LoadState(newState);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _canDamagePlayer = true;
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _canDamagePlayer = false;
        }
    }
}
