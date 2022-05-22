using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        DEFAULT
    }

    const float GRAVITY = 0.098f;
    const float JUMP_FORCE = 15.0f;
    const float TERMINAL_FALL_VELOCITY = -100f;
    const float WALK_SPEED = 12.0f;

    [SerializeField] CharacterController _controller;
    [SerializeField] private Camera _camera;
    private Vector3 _playerVelocity;
    private PlayerState _state;


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
                CameraMovement();
                PlayerMovement();
                PlayerGravity(true);
                break;
            }
        }
    }


    void LoadState(PlayerState state)
    {

    }


    void UnloadState(PlayerState state)
    {

    }


    public void SwitchState(PlayerState newState)
    {
        var lastState = _state;
        _state = newState;

        UnloadState(lastState);
        LoadState(newState);
    }



    // Hanging around til I embed everything into a state machine more
    // TODO: embed into SM
    void CameraMovement()
    {
        // get rotation speed
        var playerRotDir = new Vector3(0.0f, Input.GetAxis("Mouse X") * GlobalData.PlayerRotationSpeedHorz, 0.0f);
        var cameraRotDir = new Vector3(-Input.GetAxis("Mouse Y") * GlobalData.PlayerRotationSpeedVert, 0.0f, 0.0f);

        // apply rotation
        transform.Rotate(playerRotDir * Time.deltaTime);
        _camera.transform.Rotate(cameraRotDir * Time.deltaTime);
    }


    void PlayerMovement()
    {
        var forward = transform.TransformDirection(Vector3.forward);
        var strafe = transform.TransformDirection(Vector3.right);

        var speed = new Vector2(
            WALK_SPEED * Input.GetAxis("Horizontal"),
            WALK_SPEED * Input.GetAxis("Vertical")
        );

        var move = forward * speed.y + strafe * speed.x;

        _controller.Move(move * Time.deltaTime / 2.0f);
    }


    void PlayerGravity(bool allowJump)
    {
        _controller.Move(_playerVelocity * Time.deltaTime);


        if (_controller.isGrounded)
        {
            _playerVelocity.y = 0.0f;
        }


        if (allowJump && Input.GetButtonDown("Jump") && CanJump())
        {
            _playerVelocity.y = JUMP_FORCE;
        }


        _playerVelocity.y = Mathf.Max(_playerVelocity.y - GRAVITY, TERMINAL_FALL_VELOCITY);
    }


    bool CanJump()
    {
        return Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), 1.2f);
    }
}
