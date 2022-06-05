using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DuckDoor : MonoBehaviour
{
    [SerializeField] AudioSource _sound;

    private PlayableDirector _director;

    void Start()
    {
        _director = GetComponent<PlayableDirector>();
    }

    public void Open()
    {
        _sound.Play();
        _director.Play();
    }
}
