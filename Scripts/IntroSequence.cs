using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    enum State
    {
        PAUSE,
        MONOLOGUE,
        DOOR,
        DOOR_CLOSE
    }

    const float PAUSE_TIME = 1.0f;
    const float MONOLOGUE_TIME = 50.0f;
    const float DOOR_TIME = 5.0f;

    [SerializeField] private AudioSource _monologue;
    [SerializeField] private AudioSource _doorSound;
    [SerializeField] private AudioSource _doorCloseSound;

    private State _state = State.PAUSE;
    private float _stateTime = 0.0f;


    void Update()
    {
        _stateTime += Time.deltaTime;


        switch (_state)
        {
            case State.PAUSE: {
                if (_stateTime > PAUSE_TIME)
                {
                    SwitchState(State.MONOLOGUE);
                }
                
                break;
            }


            case State.MONOLOGUE: {
                if (_stateTime > MONOLOGUE_TIME)
                {
                    SwitchState(State.DOOR);
                }

                break;
            }


            case State.DOOR: {
                if (_stateTime > DOOR_TIME)
                {
                    SwitchState(State.DOOR_CLOSE);
                }

                break;
            }
        }
    }


    void SwitchState(State newState)
    {
        _state = newState;

        switch (newState) {
            case State.MONOLOGUE: {
                _monologue.Play();

                break;
            }


            case State.DOOR: {
                _doorSound.Play();

                break;
            }


            case State.DOOR_CLOSE: {
                _doorCloseSound.Play();

                break;
            }
        }
    }
}
