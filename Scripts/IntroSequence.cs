using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    const float MONOLOGUE_TIME = 48.0f;
    const float DOOR_TIME = 10.0f;
    const float DOOR_CLOSE_TIME = 10.0f;
    const int FIRST_LEVEL = 1;

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
                    SwitchState(State.DOOR);
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


            case State.DOOR_CLOSE: {
                if (_stateTime > DOOR_CLOSE_TIME)
                {
                    SceneManager.LoadScene(FIRST_LEVEL);
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
