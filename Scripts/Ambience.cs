using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ambience : MonoBehaviour
{
    const float MUSIC_INTERMEDIATE_TIME = 30.0f;
    const float SCREAMS_INTERMEDIATE_TIME = 300.0f;
    public const int FINAL_LEVEL_ID = 5;

    [SerializeField] private AudioSource[] _music;
    [SerializeField] private AudioSource[] _screams;

    private float _timeSinceMusic = 29.0f;
    private float _timeSinceScream = 150.0f;
    private AudioSource _currentMusic;
    private System.Random _rnd = new System.Random();
    private bool _finalLevel = false;


    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == FINAL_LEVEL_ID)
        {
            _finalLevel = true;
            foreach (AudioSource a in _music) { a.Stop(); }
            foreach (AudioSource a in _screams) { a.Stop(); }
        }
    }


    void Update()
    {
        if (!_finalLevel)
        {
            if (_currentMusic == null || !_currentMusic.isPlaying)
            {
                if (_timeSinceMusic >= MUSIC_INTERMEDIATE_TIME)
                {
                    _timeSinceMusic = 0.0f;
                    _music[_rnd.Next() % _music.Length - 1].Play();
                }
                _timeSinceMusic += Time.deltaTime;
            }

            if (_timeSinceScream >= SCREAMS_INTERMEDIATE_TIME)
            {
                _timeSinceScream = 0.0f;
                _screams[_rnd.Next() % _screams.Length - 1].Play();
            }
            _timeSinceScream += Time.deltaTime;
        }
    }
}
