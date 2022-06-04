using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawHead : MonoBehaviour
{
    [SerializeField] private Claw _main;

    void OnTriggerEnter(Collider what)
    {
        _main.HitSomething(what);
    }
}
