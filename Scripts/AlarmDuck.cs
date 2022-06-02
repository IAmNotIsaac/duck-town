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

    [SerializeField] private float _speed;
    [SerializeField] private Transform[] _pathPoints;
    [SerializeField] private TurnMode _turnMode = TurnMode.CONTINUOUS;
    
    private State _state = State.PATROL;
    private int _i = 0;
    private float _stateTime = 0.0f;

    void Update()
    {
        switch (_state)
        {
            case State.TURN: {
                Transform nextPoint = _pathPoints[_i % _pathPoints.Length];
                float targetAngle = Vector2.Angle(
                    new Vector2(transform.position.x, transform.position.z),
                    new Vector2(nextPoint.position.x, nextPoint.position.z)
                );

                transform.eulerAngles.y = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, 0.9f);

                if (Mathf.Abs(targetAngle - transform.eulerAngles.y) < 5.0f) {
                    transform.eulerAngles.y = targetAngle;
                    _state = State.PATROL;
                }

                break;
            }

            case State.PATROL: {
                Transform nextPoint = _pathPoints[_i % _pathPoints.Length];
                
                transform.position += transform.position - nextPoint.position;

                if (Vector3.Distance(transform.position, nextPoint) <= 0.1f)
                {
                    Transform.position = nextPoint.position;
                    _i++;
                    
                    if (_turnMode == TurnMode.PAUSE)
                    {
                        _state = State.TURN;
                    }
                }

                break;
            }

            case State.ALARM: {
                break;
            }
        }
    }


    void SwitchState(State newState)
    {
        _stateTime = 0.0f;
        _state = newState;
    }
}
