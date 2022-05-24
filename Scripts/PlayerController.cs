using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        DEFAULT,
        CLIMB_PREP,
        CLIMB
    }

    const float GRAVITY = 0.098f;
    const float JUMP_FORCE = 6.0f;
    const float TERMINAL_FALL_VELOCITY = -100f;
    const float WALK_SPEED = 12.0f;
    const float CLIMB_HEIGHT = 0.5f;
    const float CAMERA_HEIGHT = 0.5f;

    [SerializeField] private CharacterController _controller;
    [SerializeField] private Camera _camera;
    [SerializeField] private PlayableDirector _cameraDirector;
    private Vector3 _playerVelocity;
    private PlayerState _state;

    // state specific vars
    // CLIMB_PREP
    private float _climbPrep_targetAngle = 0.0f;
    private float _climbPrep_cameraTargetAngle = 0.0f;
    private float _climbPrep_startTime = 0.0f;

    // CLIMB
    private float _climb_startTime = 0.0f;


    void Start()
    {
        GlobalData.LockMouse();
    }


    void Update()
    {
        StateUpdate(_state);
    }


    // State stuff.
    // TODO: Move state stuff into ABC instead.
    void StateUpdate(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.DEFAULT: {
                DefaultCameraMovement();


                { // Walking
                    var forward = transform.TransformDirection(Vector3.forward);
                    var strafe = transform.TransformDirection(Vector3.right);

                    var speed = new Vector2(
                        WALK_SPEED * Input.GetAxis("Horizontal"),
                        WALK_SPEED * Input.GetAxis("Vertical")
                    );

                    var move = forward * speed.y + strafe * speed.x;

                    _controller.Move(move * Time.deltaTime / 2.0f);
                }


                { // Jumping/gravity
                    _controller.Move(_playerVelocity * Time.deltaTime);

                    if (_controller.isGrounded)
                    {
                        _playerVelocity.y = 0.0f;
                    }

                    if (Input.GetButtonDown("Jump") && CanJump())
                    {
                        if (
                            !Physics.Raycast(transform.position + new Vector3(0.0f, CLIMB_HEIGHT, 0.0f), transform.TransformDirection(Vector3.forward), 0.8f) &&
                            Physics.Raycast(transform.position - new Vector3(0.0f, 0.25f, 0.0f), transform.TransformDirection(Vector3.forward), 0.8f)
                        ) {
                            SwitchState(PlayerState.CLIMB_PREP);
                        }
                        else
                        {
                            _playerVelocity.y = JUMP_FORCE;
                        }
                    }

                    _playerVelocity.y = Mathf.Max(_playerVelocity.y - GRAVITY, TERMINAL_FALL_VELOCITY);
                }


                break;
            }


            case PlayerState.CLIMB_PREP: {
                transform.eulerAngles = new Vector3(
                    0.0f,
                    Mathf.LerpAngle(transform.eulerAngles.y, _climbPrep_targetAngle, 8.0f * Time.deltaTime),
                    0.0f
                );

                _camera.transform.eulerAngles = new Vector3(
                    Mathf.LerpAngle(_camera.transform.eulerAngles.x, _climbPrep_cameraTargetAngle, 8.0f * Time.deltaTime),
                    _camera.transform.eulerAngles.y,
                    _camera.transform.eulerAngles.z
                );

                if (Time.time - _climbPrep_startTime > 0.8f)
                {
                    SwitchState(PlayerState.CLIMB);
                }

                break;
            }


            case PlayerState.CLIMB: {
                if (Time.time - _climb_startTime > 1.5f)
                {
                    SwitchState(PlayerState.DEFAULT);
                }
                
                break;
            }
        }
    }


    void LoadState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.CLIMB_PREP: {
                RaycastHit hit;
                Physics.Raycast(transform.position - new Vector3(0.0f, 0.25f, 0.0f), transform.TransformDirection(Vector3.forward), out hit, 0.8f);
                Vector3 normal = hit.normal;
                
                _climbPrep_targetAngle = 180.0f + Quaternion.FromToRotation(Vector3.forward, normal).eulerAngles.y;
                _climbPrep_startTime = Time.time;
                
                break;
            }


            case PlayerState.CLIMB: {
                _cameraDirector.Play();
                _climb_startTime = Time.time;

                break;
            }
        }
    }


    void UnloadState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.CLIMB: {
                _cameraDirector.Stop();

                _camera.transform.eulerAngles = new Vector3(
                    0.0f,
                    _camera.transform.eulerAngles.y,
                    _camera.transform.eulerAngles.z
                );

                // _camera.transform.localPosition = new Vector3(
                //     0.0f,
                //     CAMERA_HEIGHT,
                //     0.0f
                // );

                _controller.Move(transform.TransformDirection(Vector3.forward) + new Vector3(0.0f, 1.5f, 0.0f));

                break;
            };
        }
    }


    public void SwitchState(PlayerState newState)
    {
        var lastState = _state;
        _state = newState;

        UnloadState(lastState);
        LoadState(newState);
    }


    void DefaultCameraMovement()
    {
        // get rotation speed
        var playerRotDir = new Vector3(0.0f, Input.GetAxis("Mouse X") * GlobalData.PlayerRotationSpeedHorz, 0.0f);
        var cameraRotDir = new Vector3(-Input.GetAxis("Mouse Y") * GlobalData.PlayerRotationSpeedVert, 0.0f, 0.0f);

        // apply rotation
        transform.Rotate(playerRotDir * Time.deltaTime);
        _camera.transform.Rotate(cameraRotDir * Time.deltaTime);

        Debug.Log(_camera.transform.localRotation);

        _camera.transform.eulerAngles = new Vector3(
            _camera.transform.eulerAngles.x,//Mathf.Clamp(_camera.transform.eulerAngles.x, 90.0f, -90.0f),
            _camera.transform.eulerAngles.y,
            _camera.transform.eulerAngles.z
        );

        _camera.transform.localPosition = new Vector3(
            Mathf.Lerp(_camera.transform.localPosition.x, 0.0f, 8.0f * Time.deltaTime),
            Mathf.Lerp(_camera.transform.localPosition.y, CAMERA_HEIGHT, 0.5f * Time.deltaTime),
            Mathf.Lerp(_camera.transform.localPosition.z, 0.0f, 8.0f * Time.deltaTime)
        );
    }


    bool CanJump()
    {
        return _controller.isGrounded || Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), 1.2f);
    }
}
