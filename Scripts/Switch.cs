using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Switch : MonoBehaviour
{
    [SerializeField] private DoorManager _door;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _sound;
    public bool locked = false;

    public void Interact()
    {
        _door.Open();
        _sound.Play();
        _animator.Play("Base Layer.Activate");
    }
}
