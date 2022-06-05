using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutroSequence : MonoBehaviour
{
    const float THANK_YOU_TIME = 38.8f;
    const float END_TIME = 166.0f;

    [SerializeField] private RawImage[] _images;
    [SerializeField] private RawImage _thankYou;

    private List<(float, RawImage)> _creditsSequence = new List<(float, RawImage)>();
    private float _sceneTime = 0.0f;


    void Start()
    {
        _creditsSequence.Add((7.0f, _images[0]));
        _creditsSequence.Add((14.8f, _images[1]));
        _creditsSequence.Add((23.8f, _images[2]));
        _creditsSequence.Add((30.8f, _images[3]));
    }


    void Update()
    {
        foreach ((float t, RawImage i) in _creditsSequence)
        {
            if (_sceneTime < THANK_YOU_TIME)
            {
                if (t < _sceneTime)
                {
                    i.enabled = true;
                }
            }

            else
            {
                i.enabled = false;
                _thankYou.enabled = true;
            }
        }

        if (_sceneTime >= END_TIME)
        {
            Application.Quit();
        }
        
        _sceneTime += Time.deltaTime;
    }
}