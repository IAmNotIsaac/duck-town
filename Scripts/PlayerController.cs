using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        DEFAULT,
        WATER,
        CLIMB_PREP,
        CLIMB
    }

    const float GRAVITY = 20.0f;
    const float WATER_GRAVITY = 5.0f;
    const float JUMP_FORCE = 3.0f;
    const float TERMINAL_FALL_VELOCITY = -100.0f;
    const float WALK_SPEED = 12.0f;
    const float SWIM_SPEED = 10.0f;
    const float VERTICAL_SWIM_SPEED = 5.0f;
    const float SWIM_ACCELERATION = 0.4f;
    const float WATER_WALK_SPEED = 8.0f;
    const float UNDERWATER_WALK_SPEED = 6.0f;
    const float CLIMB_HEIGHT = 0.6f;
    const float CAMERA_HEIGHT = 0.5f;
    const float COYOTE_TIME = 0.2f;
    const float ARM_REACH = 2.0f;

    [SerializeField] private CharacterController _controller;
    [SerializeField] private Camera _camera;
    [SerializeField] private PlayableDirector _cameraDirector;
    [SerializeField] private RawImage _reticleHint;
    [SerializeField] private DuckNavigation duck;
    private Vector3 _playerVelocity;
    private PlayerState _state;

    // state specific vars
    // DEFAULT
    private float _default_leaveGroundTime = 0.0f;

    // WATER
    private Collider _water;

    // CLIMB_PREP
    private float _climbPrep_targetAngle = 0.0f;
    private float _climbPrep_cameraTargetAngle = 0.0f;
    private float _climbPrep_targetClimbHeight = 0.0f;
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

                    bool canJump = CanJump();

                    if (canJump || _default_leaveGroundTime < COYOTE_TIME) {
                        if (Input.GetButtonDown("Jump"))
                        {
                            if (
                                !Physics.Raycast(transform.position + new Vector3(0.0f, CLIMB_HEIGHT, 0.0f), transform.TransformDirection(Vector3.forward), 0.8f) &&
                                Physics.Raycast(transform.position - new Vector3(0.0f, 0.25f, 0.0f), transform.TransformDirection(Vector3.forward), 0.8f)
                            ) {
                                SwitchState(PlayerState.CLIMB_PREP);
                            }
                            else
                            {
                                _default_leaveGroundTime = COYOTE_TIME;
                                _playerVelocity.y = JUMP_FORCE;
                            }
                        }
                    }
                    
                    if (_controller.isGrounded)
                    {
                        _default_leaveGroundTime = 0.0f;
                    }

                    else
                    {
                        _default_leaveGroundTime += Time.deltaTime;
                    }

                    _playerVelocity.y = Mathf.Max(_playerVelocity.y - GRAVITY * Time.deltaTime, TERMINAL_FALL_VELOCITY);
                }


                { // Interacting
                    RaycastHit hit;

                    _reticleHint.enabled = false;
                    
                    if (Physics.Raycast(transform.position + new Vector3(0.0f, CAMERA_HEIGHT, 0.0f), _camera.transform.TransformDirection(Vector3.forward), out hit, ARM_REACH))
                    {
                        Switch switch_ = hit.collider.GetComponent<Switch>();
                        if (switch_ != null)
                        {
                            _reticleHint.enabled = true;

                            if (Input.GetButtonDown("Fire1"))
                            {
                                switch_.Interact();
                            }
                        }
                    }
                }


                if (_water)
                {
                    SwitchState(PlayerState.WATER);
                }


                break;
            }


            case PlayerState.WATER: {
                DefaultCameraMovement();


                int layerMask = 1 << 4;
                bool headUnderwater = Physics.CheckBox(_camera.transform.position, Vector3.zero, Quaternion.identity, layerMask);
                bool grounded = CanJump();


                { // Walking
                    float speedFactor = SWIM_SPEED;

                    if (grounded)
                    {
                        speedFactor = WATER_WALK_SPEED;
                        if (headUnderwater)
                        {
                            speedFactor = UNDERWATER_WALK_SPEED;
                        }
                    }

                    var forward = transform.TransformDirection(Vector3.forward);
                    var strafe = transform.TransformDirection(Vector3.right);

                    var speed = new Vector2(
                        speedFactor * Input.GetAxis("Horizontal"),
                        speedFactor * Input.GetAxis("Vertical")
                    );

                    var move = forward * speed.y + strafe * speed.x;

                    _controller.Move(move * Time.deltaTime / 2.0f);
                }


                { // Swimming up/down + gravity
                    _controller.Move(_playerVelocity * Time.deltaTime);
                    
                    if (grounded)
                    {
                        _playerVelocity.y = 0.0f;
                    }

                    if (headUnderwater)
                    {
                        _playerVelocity.y += Input.GetAxis("Jump") * SWIM_ACCELERATION;
                    }

                    _playerVelocity.y = Mathf.Clamp(_playerVelocity.y - WATER_GRAVITY * Time.deltaTime, -VERTICAL_SWIM_SPEED, VERTICAL_SWIM_SPEED / 2.0f);
                }


                { // Climbing
                    if (
                        Input.GetButtonDown("Jump") &&
                        !Physics.Raycast(transform.position + new Vector3(0.0f, CLIMB_HEIGHT, 0.0f), transform.TransformDirection(Vector3.forward), 0.8f) &&
                        Physics.Raycast(transform.position - new Vector3(0.0f, 0.25f, 0.0f), transform.TransformDirection(Vector3.forward), 0.8f)
                    ) {
                        SwitchState(PlayerState.CLIMB_PREP);
                    }
                }


                if (_water == null)
                {
                    SwitchState(PlayerState.DEFAULT);
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

                _controller.transform.position = new Vector3(
                    _controller.transform.position.x,
                    Mathf.Lerp(_controller.transform.position.y, _climbPrep_targetClimbHeight, 8.0f * Time.deltaTime),
                    _controller.transform.position.z
                );

                if (Time.time - _climbPrep_startTime > 0.4f)
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

                Vector3 origin = transform.TransformDirection(Vector3.forward) + new Vector3(0.0f, CLIMB_HEIGHT, 0.0f) + transform.position;

                Physics.Raycast(origin, Vector3.down, out hit);

                _climbPrep_targetClimbHeight = hit.point.y;
                
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
            case PlayerState.DEFAULT: {
                _reticleHint.enabled = false;

                break;
            }


            case PlayerState.CLIMB: {
                _cameraDirector.Stop();

                // reset angle
                _camera.transform.eulerAngles = new Vector3(
                    0.0f,
                    _camera.transform.eulerAngles.y,
                    _camera.transform.eulerAngles.z
                );

                // reset local cam position
                _camera.transform.localPosition = new Vector3(
                    0.0f,
                    CAMERA_HEIGHT,
                    0.0f
                );

                Vector3 origin = transform.TransformDirection(Vector3.forward) + new Vector3(0.0f, CLIMB_HEIGHT, 0.0f) + transform.position;
                RaycastHit hit;

                Physics.Raycast(origin, Vector3.down, out hit);


                // Unity is a broken engine on its last limbs and soon hopefully out the door, and the creators dont know how to fix bugs,
                // so we have to deal with this issue where sometimes we just cant set the position of a gameobject if it has a CharacterController.
                // Thus, we have to do a time-wasting stupid method of using the CharacterController's Move function which kind of works in
                // order to achieve what we wanted instead of spending our time doing literally anything else, anything remotely productive,
                // but here we are anyways.

                // Player controller floats 0.08 above ground for some reason, idk  VV 
                Vector3 move = hit.point - transform.position + new Vector3(0.0f, 1.08f, 0.0f);

                _controller.Move(move);

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
        var playerRotDir = new Vector3(0.0f, Input.GetAxis("Mouse X") * GlobalData.PlayerRotationSpeedHorz * Time.deltaTime, 0.0f);
        var cameraRotDir = new Vector3(-Input.GetAxis("Mouse Y") * GlobalData.PlayerRotationSpeedVert * Time.deltaTime, 0.0f, 0.0f);

        // apply rotation
        transform.Rotate(playerRotDir);
        _camera.transform.Rotate(cameraRotDir);

        // TODO: fix camera clamping. wtf why doesnt it work literally what are these values wtf who made this engine
        _camera.transform.eulerAngles = new Vector3(
            _camera.transform.eulerAngles.x,//Mathf.Clamp(_camera.transform.eulerAngles.x, 90.0f, -90.0f), ???
            _camera.transform.eulerAngles.y,
            _camera.transform.eulerAngles.z
        );
    }


    bool CanJump()
    {
        int layerMask = 1 << 4;
        return _controller.isGrounded || Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), 1.05f, ~layerMask);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            _water = other;
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            _water = null;
        }
    }
}
