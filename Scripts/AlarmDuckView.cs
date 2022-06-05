using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmDuckView : MonoBehaviour
{
    [SerializeField] private AlarmDuck _alarmDuck;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _alarmDuck.playerInViewArea = true;
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _alarmDuck.playerInViewArea = false;
        }
    }
}
