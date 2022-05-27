using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DuckDoor : MonoBehaviour
{
    private PlayableDirector _director;

    void Start()
    {
        _director = GetComponent<PlayableDirector>();
    }

    public void Open()
    {
        _director.Play();
    }
}
