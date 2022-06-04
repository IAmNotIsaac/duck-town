using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Claw : MonoBehaviour
{
    enum State
    {
        UNLAUNCHED,
        LAUNCHING,
        REEL
    }

    const int MAX_CHAIN_COUNT = 10;
    const float NEW_CHAIN_DISTANCE = 0.75f;
    const float CLAW_SPEED = 25.0f;

    [SerializeField] private GameObject _chainPiece;
    [SerializeField] private GameObject _clawHead;
    
    private State _state = State.UNLAUNCHED;
    private int _chainCount = 0;
    private Vector3 _direction;
    private GameObject _lastChain;
    private PlayerController _player;
    private float _launchTime;
    private float _reelTime;


    void Start()
    {
        _lastChain = _clawHead;
    }


    public void HitSomething(Collider what)
    {
        if (_state == State.LAUNCHING)
        {
            _player = what.GetComponent<PlayerController>();

            if (what.GetComponent<DuckNavigation>()) { return; }
            if (_player)
            {
                _player.ClawReel(_clawHead.transform);
            }

            SwitchState(State.REEL);
        }
    }


    public void Launch(Vector3 target)
    {
        if (_state == State.UNLAUNCHED)
        {
            _direction = Vector3.Normalize(target - transform.position);

            SwitchState(State.LAUNCHING);
        }
    }
    

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Launch(new Vector3(0.0f, 1.0f, 0.0f));
        }

        switch (_state)
        {
            case State.LAUNCHING: {
                _clawHead.transform.position += _direction * CLAW_SPEED * Time.deltaTime;
                
                // Debug.Log(_lastChain);

                // if (Vector3.Distance(_lastChain.position, transform.position) > NEW_CHAIN_DISTANCE && _chainCount < MAX_CHAIN_COUNT)
                // {
                //     Debug.Log("new chain");
                //     _chainCount++;
                //     _lastChain = Instantiate(_chainPiece, _clawHead.transform);
                //     _lastChain.localPosition = Vector3.zero;
                //     _lastChain.localScale = Vector3.one;
                // }

                if (Vector3.Distance(transform.position, _clawHead.transform.position) > MAX_CHAIN_COUNT * NEW_CHAIN_DISTANCE + NEW_CHAIN_DISTANCE)
                {
                    SwitchState(State.REEL);
                }

                _launchTime += Time.deltaTime;
                
                break;
            }


            case State.REEL: {
                _clawHead.transform.position -= _direction * CLAW_SPEED * Time.deltaTime;

                if (_reelTime >= _launchTime)
                {
                    SwitchState(State.UNLAUNCHED);
                }

                _reelTime += Time.deltaTime;

                break;
            }
        }
    }


    void LoadState(State newState)
    {
        switch (newState)
        {
            case State.UNLAUNCHED: {
                _clawHead.transform.localPosition = Vector3.zero;
                _clawHead.enabled = false;

                if (_player)
                {
                    _player.ClawFree();
                }

                break;
            }


            case State.LAUNCHING: {
                _chainCount = 0;
                _launchTime = 0.0f;
                _clawHead.enabled = true;

                break;
            }


            case State.REEL: {
                _reelTime = 0.0f;

                break;
            }
        }
    }


    void SwitchState(State newState)
    {
        _state = newState;

        LoadState(newState);
    }
}
