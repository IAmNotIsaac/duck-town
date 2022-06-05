using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Switch : MonoBehaviour
{
    [SerializeField] private DoorManager _door;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _sound;
    [SerializeField] private AudioSource _finalMusic;

    public void Interact()
    {
        _door.Open();
        _sound.Play();
        _animator.Play("Base Layer.Activate");

        if (SceneManager.GetActiveScene().buildIndex == Ambience.FINAL_LEVEL_ID)
        {
            _finalMusic.Stop();
        }
    }
}
