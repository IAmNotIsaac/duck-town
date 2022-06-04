using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        DEFAULT,
        WATER,
        CLIMB_PREP,
        CLIMB,
        HESITATE,
        DOOR_SOUND,
        LEVEL_EXIT,
        LEVEL_ENTER,
        REEL
    }

    const float GRAVITY = 20.0f;
    const float WATER_GRAVITY = 5.0f;
    const float JUMP_FORCE = 3.0f;
    const float TERMINAL_FALL_VELOCITY = -100.0f;
    const float WALK_SPEED = 12.0f;
    const float SWIM_SPEED = 8.0f;
    const float VERTICAL_SWIM_SPEED = 3.0f;
    const float SWIM_ACCELERATION = 0.4f;
    const float WATER_WALK_SPEED = 5.0f;
    const float UNDERWATER_WALK_SPEED = 3.0f;
    const float CLIMB_HEIGHT = 0.6f;
    const float CAMERA_HEIGHT = 0.5f;
    const float COYOTE_TIME = 0.2f;
    const float ARM_REACH = 2.0f;
    const float STEP_FREQUENCY = 0.75f;
    const float MAX_HEALTH = 3.0f;
    const float HEAL_SPEED = 0.1f;
    const float DAMAGE_AMOUNT = 1.0f;
    const float DAMAGE_COOLDOWN = 0.5f;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Camera _camera;
    [SerializeField] private PlayableDirector _cameraDirector;
    [SerializeField] private RawImage _reticleHint;
    [SerializeField] private Image _waterOverlay;
    [SerializeField] private DuckNavigation duck;
    [SerializeField] private AudioSource _jumpSound;
    [SerializeField] private AudioSource[] _stepSounds;
    [SerializeField] private AudioSource _climbSound;
    [SerializeField] private AudioSource _doorCloseSound;
    [SerializeField] private PlayableDirector _levelExitFade;
    [SerializeField] private PlayableDirector _levelEnterFade;
    [SerializeField] private RawImage _damageVignette;
    [HideInInspector] public PlayerState state;
    private Vector3 _playerVelocity;
    private Vector2 _inputVector = Vector2.zero;
    private System.Random _rnd = new System.Random();
    private float _health = MAX_HEALTH;
    private float _damageCooldown = 0.0f;

    // state specific vars
    // DEFAULT
    private float _default_leaveGroundTime = 0.0f;
    private float _default_stepSoundTimer = 0.0f;

    // WATER
    private Collider _water;

    // CLIMB_PREP
    private float _climbPrep_targetAngle = 0.0f;
    private float _climbPrep_cameraTargetAngle = 0.0f;
    private float _climbPrep_targetClimbHeight = 0.0f;
    private float _climbPrep_startTime = 0.0f;

    // CLIMB
    private float _climb_startTime = 0.0f;

    // HESITATE
    private float _hesitate_startTime = 0.0f;

    // DOOR_SOUND
    private float _doorSound_startTime = 0.0f;

    // LEVEL_EXIT
    private float _loadLevel_startTime = 0.0f;
    private int _nextLevelID = 0;

    // LEVEL_ENTER
    private float _levelEnter_startTime = 0.0f;

    // REEL
    private Transform _reel_ogParent;


    void Start()
    {
        GlobalData.LockMouse();
        SwitchState(PlayerState.HESITATE);
    }


    public void Damage()
    {
        if (_damageCooldown <= 0.0f)
        {
            _damageCooldown = DAMAGE_COOLDOWN;
            _health -= DAMAGE_AMOUNT;

            if (_health <= 0.0f)
            {
                Debug.Log("you died");
            }
        }
    }


    void Update()
    {
        _inputVector = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
        // _inputVector.Normalize();
        StateUpdate(state);
        Health();
    }


    void Health()
    {
        _damageCooldown -= Time.deltaTime;
        _health = Mathf.Clamp(_health + HEAL_SPEED * Time.deltaTime, 0.0f, MAX_HEALTH);
        _damageVignette.color = new Color(
            1.0f,
            1.0f,
            1.0f,
            1.0f - _health / MAX_HEALTH
        );
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
                        WALK_SPEED * _inputVector.x,
                        WALK_SPEED * _inputVector.y
                    );

                    var move = forward * speed.y + strafe * speed.x;

                    if (_inputVector != Vector2.zero)
                    {
                        if (_default_stepSoundTimer >= STEP_FREQUENCY)
                        {
                            _default_stepSoundTimer = 0.0f;
                            _stepSounds[_rnd.Next() % _stepSounds.Length].Play();
                        }
                    }

                    else
                    {
                        foreach (AudioSource s in _stepSounds)
                        {
                            s.Stop();
                        }
                    }

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
                                _jumpSound.Play();
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


                _default_stepSoundTimer += Time.deltaTime;


                break;
            }


            case PlayerState.WATER: {
                DefaultCameraMovement();


                int layerMask = 1 << 4;
                bool headUnderwater = Physics.CheckBox(_camera.transform.position, Vector3.zero, Quaternion.identity, layerMask);
                bool grounded = CanJump();


                _waterOverlay.enabled = headUnderwater;


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
                        speedFactor * _inputVector.x,
                        speedFactor * _inputVector.y
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


            case PlayerState.HESITATE: {
                if (_hesitate_startTime > 1.0f)
                {
                    SwitchState(PlayerState.DOOR_SOUND);
                }
                _hesitate_startTime += Time.deltaTime;
                
                break;
            }


            case PlayerState.DOOR_SOUND: {
                if (_doorSound_startTime > 3.5f)
                {
                    SwitchState(PlayerState.LEVEL_ENTER);
                }
                _doorSound_startTime += Time.deltaTime;

                break;
            }


            case PlayerState.LEVEL_EXIT: {
                if (_loadLevel_startTime > 1.0f)
                {
                    SceneManager.LoadScene(_nextLevelID);
                }
                _loadLevel_startTime += Time.deltaTime;

                break;
            }


            case PlayerState.LEVEL_ENTER: {
                if (_levelEnter_startTime > 1.2f)
                {
                    SwitchState(PlayerState.DEFAULT);
                }
                _levelEnter_startTime += Time.deltaTime;

                break;
            }


            case PlayerState.REEL: {
                // DefaultCameraMovement();

                break;
            }
        }
    }


    void LoadState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.DEFAULT: {
                _default_stepSoundTimer = 0.0f;

                break;
            }


            case PlayerState.CLIMB_PREP: {
                RaycastHit hit;
                Physics.Raycast(transform.position - new Vector3(0.0f, 0.25f, 0.0f), transform.TransformDirection(Vector3.forward), out hit, 0.8f);
                Vector3 normal = hit.normal;
                
                _climbPrep_targetAngle = 180.0f + Quaternion.FromToRotation(Vector3.forward, normal).eulerAngles.y;
                _climbPrep_startTime = Time.time;

                Vector3 origin = transform.TransformDirection(Vector3.forward) + new Vector3(0.0f, CLIMB_HEIGHT, 0.0f) + transform.position;

                Physics.Raycast(origin, Vector3.down, out hit);

                _climbPrep_targetClimbHeight = hit.point.y;
                
                _climbSound.Play();
                
                break;
            }


            case PlayerState.CLIMB: {
                _cameraDirector.Play();
                _climb_startTime = Time.time;

                break;
            }


            case PlayerState.HESITATE: {
                _hesitate_startTime = 0.0f;

                break;
            }


            case PlayerState.DOOR_SOUND: {
                _doorSound_startTime = 0.0f;
                _doorCloseSound.Play();

                break;
            }


            case PlayerState.LEVEL_EXIT: {
                _loadLevel_startTime = 0.0f;
                _levelExitFade.Play();

                break;
            }


            case PlayerState.LEVEL_ENTER: {
                _levelEnter_startTime = 0.0f;
                _levelEnterFade.Play();

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

                foreach (AudioSource s in _stepSounds)
                {
                    s.Stop();
                }

                break;
            }


            case PlayerState.WATER: {
                _waterOverlay.enabled = false;

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


            case PlayerState.REEL: {
                // var move = _camera.transform.position - transform.position - new Vector3(0.0f, CAMERA_HEIGHT, 0.0f);
                var destination = _camera.transform.position - new Vector3(0.0f, CAMERA_HEIGHT, 0.0f);
                Vector3 move = Vector3.zero;

                RaycastHit hit;
                Physics.Raycast(transform.position, destination, out hit);
                move = hit.point - transform.position;
                _camera.transform.SetParent(transform);
                _camera.transform.localPosition = new Vector3(0.0f, CAMERA_HEIGHT, 0.0f);

                _controller.Move(move);

                break;
            }
        }
    }


    public void SwitchState(PlayerState newState)
    {
        var lastState = state;
        state = newState;

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


    public void SwitchLevel(int nextLevelID)
    {
        _nextLevelID = nextLevelID;
        SwitchState(PlayerState.LEVEL_EXIT);
    }


    public void ClawReel(Transform reparent)
    {
        SwitchState(PlayerState.REEL);
        _camera.transform.SetParent(reparent.transform);
    }


    public void ClawFree()
    {
        SwitchState(PlayerState.DEFAULT);
    }
}
