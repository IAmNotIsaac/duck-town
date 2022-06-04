using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSequence : MonoBehaviour
{
    enum State
    {
        PAUSE,
        MONOLOGUE,
        TITLE,
        AUTHORS,
        MUSIC_CRED,
        ADDITIONAL_CRED,
        SETTLE,
        DOOR,
        DOOR_CLOSE
    }

    const float PAUSE_TIME = 1.0f;
    const float MONOLOGUE_TIME = 51.0f;
    const float TITLE_TIME = 3.649f;
    const float AUTHORS_TIME = 3.649f;
    const float MUSIC_CRED_TIME = 3.649f;
    const float ADDITIONAL_CRED_TIME = 3.649f;
    const float SETTLE_TIME = 10.0f;
    const float DOOR_TIME = 10.0f;
    const float DOOR_CLOSE_TIME = 3.503f;
    const int FIRST_LEVEL = 1;

    [SerializeField] private AudioSource _monologue;
    [SerializeField] private AudioSource _doorSound;
    [SerializeField] private AudioSource _doorCloseSound;
    [SerializeField] private RawImage _title;
    [SerializeField] private RawImage _authors;
    [SerializeField] private RawImage _musicCred;
    [SerializeField] private RawImage _additionalCred;

    private State _state = State.PAUSE;
    private float _stateTime = 0.0f;


    void Update()
    {
        _stateTime += Time.deltaTime;


        switch (_state)
        {
            case State.PAUSE: {
                if (_stateTime > PAUSE_TIME) { SwitchState(State.MONOLOGUE); }
                break;
            }

            case State.MONOLOGUE: {
                if (_stateTime > MONOLOGUE_TIME) { SwitchState(State.TITLE); }
                break;
            }

            case State.TITLE: {
                if (_stateTime > TITLE_TIME) { SwitchState(State.AUTHORS); }
                break;
            }

            case State.AUTHORS: {
                if (_stateTime > AUTHORS_TIME) { SwitchState(State.MUSIC_CRED); }
                break;
            }

            case State.MUSIC_CRED: {
                if (_stateTime > MUSIC_CRED_TIME) { SwitchState(State.ADDITIONAL_CRED); }
                break;
            }

            case State.ADDITIONAL_CRED: {
                if (_stateTime > ADDITIONAL_CRED_TIME) { SwitchState(State.SETTLE); }
                break;
            }

            case State.SETTLE: {
                if (_stateTime > SETTLE_TIME) { SwitchState(State.DOOR); }
                break;
            }

            case State.DOOR: {
                if (_stateTime > DOOR_TIME) { SwitchState(State.DOOR_CLOSE); }
                break;
            }

            case State.DOOR_CLOSE: {
                if (_stateTime > DOOR_CLOSE_TIME) { SceneManager.LoadScene(FIRST_LEVEL); }
                break;
            }
        }
    }


    void SwitchState(State newState)
    {
        _state = newState;
        _stateTime = 0.0f;

        switch (newState) {
            case State.MONOLOGUE: {
                _monologue.Play();

                break;
            }


            case State.TITLE: {
                _title.enabled = true;

                break;
            }


            case State.AUTHORS: {
                _title.enabled = false;
                _authors.enabled = true;

                break;
            }


            case State.MUSIC_CRED: {
                _authors.enabled = false;
                _musicCred.enabled = true;

                break;
            }


            case State.ADDITIONAL_CRED: {
                _musicCred.enabled = false;
                _additionalCred.enabled = true;

                break;
            }


            case State.SETTLE: {
                _additionalCred.enabled = false;

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
