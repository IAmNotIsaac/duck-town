using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class DoorManager : MonoBehaviour
{
    enum OpenState {
        OPEN,
        CLOSED
    }

    [SerializeField] private DuckNavigation _duck;
    [SerializeField] private int _nextLevelID;
    [SerializeField] private DuckDoor _duckDoor;
    [SerializeField] private Collider _doorCollision;
    [SerializeField] private AudioSource _sound;
    private PlayableDirector _director;
    private OpenState _state = OpenState.CLOSED;

    void Start()
    {
        _director = GetComponent<PlayableDirector>();
    }

    public void Enter(PlayerController player)
    {
        // TODO: call player enter door animation
        // TODO: switch level
    }

    public void Open()
    {
        if (_state == OpenState.CLOSED)
        {
            _state = OpenState.OPEN;
            _doorCollision.enabled = false;
            _sound.Play();
            _director.Play();
            _duckDoor.Open();

            if (SceneManager.GetActiveScene().buildIndex != Ambience.FINAL_LEVEL_ID)
            {
                _duck.SwitchState(DuckNavigation.NavState.EXIT);
            }
        }
    }
}
