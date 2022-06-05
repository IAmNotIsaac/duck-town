using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckRoomTrigger : MonoBehaviour
{
    [SerializeField] Switch _switch;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<DuckNavigation>())
        {
            _switch.locked = false;
        }
    }
}
