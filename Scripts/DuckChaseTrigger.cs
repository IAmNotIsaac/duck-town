using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckChaseTrigger : MonoBehaviour
{
    [SerializeField] DuckNavigation _duck;
    [SerializeField] AudioSource _music;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _music.Play();
            _duck.SwitchState(DuckNavigation.NavState.FINAL_CHASE);
        }
    }
}