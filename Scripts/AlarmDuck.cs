using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmDuck : MonoBehaviour
{
    enum TurnMode
    {
        CONTINUOUS,
        PAUSE
    }

    enum State
    {
        PATROL,
        TURN,
        ALARM
    }

    const float ALARM_TIME = 4.0f;

    [HideInInspector] public bool playerInViewArea = false;

    [SerializeField] private float _speed;
    [SerializeField] private Transform[] _pathPoints;
    [SerializeField] private TurnMode _turnMode = TurnMode.CONTINUOUS;
    [SerializeField] private DuckNavigation _duck;
    [SerializeField] private PlayerController _player;
    
    private State _state = State.PATROL;
    private int _i = 0;
    private float _stateTime = 0.0f;

    void Update()
    {
        FacePoint();
        switch (_state)
        {
            case State.TURN: {
                Transform nextPoint = _pathPoints[_i % _pathPoints.Length];
                float targetAngle = Vector2.Angle(
                    new Vector2(transform.position.x, transform.position.z),
                    new Vector2(nextPoint.position.x, nextPoint.position.z)
                );

                transform.eulerAngles = new Vector3(
                    0.0f,
                    Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, 0.9f),
                    0.0f
                );

                if (Mathf.Abs(targetAngle - transform.eulerAngles.y) < 5.0f) {
                    transform.eulerAngles = new Vector3(
                        0.0f,
                        targetAngle,
                        0.0f
                    );
                    SwitchState(State.PATROL);
                }

                break;
            }

            case State.PATROL: {
                Transform nextPoint = _pathPoints[_i % _pathPoints.Length];
                Vector2 destination = new Vector2(
                    nextPoint.position.x,
                    nextPoint.position.z
                );
                Vector2 pos = new Vector2(
                    transform.position.x,
                    transform.position.z
                );
                Vector2 direction = (destination - pos).normalized;
                Vector2 speed = direction * Time.deltaTime * _speed;

                transform.position += new Vector3(
                    speed.x,
                    0.0f,
                    speed.y
                );

                if (Vector3.Distance(transform.position, nextPoint.position) <= 0.1f)
                {
                    _i++;
                    
                    if (_turnMode == TurnMode.PAUSE)
                    {
                        SwitchState(State.TURN);
                    }
                }

                RaycastHit hit;
                if (Physics.Linecast(transform.position, _player.transform.position, out hit))
                {
                    if (hit.collider.GetComponent<PlayerController>() && playerInViewArea)
                    {
                        SwitchState(State.ALARM);
                    }
                }

                break;
            }

            case State.ALARM: {
                if (_stateTime >= ALARM_TIME)
                {
                    SwitchState(State.PATROL);
                }

                break;
            }
        }

        _stateTime += Time.deltaTime;
    }


    void LoadState(State state)
    {
        switch (state)
        {
            case State.ALARM: {
                _duck.SwitchState(DuckNavigation.NavState.ALERT);

                break;
            }
        }
    }


    void SwitchState(State newState)
    {
        _stateTime = 0.0f;
        _state = newState;

        LoadState(newState);
    }


    void FacePoint()
    {
        Transform nextPoint = _pathPoints[_i % _pathPoints.Length];
        Vector2 destination = new Vector2(
            nextPoint.position.x,
            nextPoint.position.z
        );
        Vector2 pos = new Vector2(
            transform.position.x,
            transform.position.z
        );
        Vector2 direction = (destination - pos).normalized;
        
        float targetAngle = Mathf.Atan2(direction.y - 0.0f, direction.x - 0.0f) * 180 / Mathf.PI;

        transform.eulerAngles = new Vector3(
            0.0f,
            Mathf.LerpAngle(transform.eulerAngles.y, -targetAngle + 90.0f, 0.9f),
            0.0f
        );
    }
}
