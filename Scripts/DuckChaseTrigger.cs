using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckChaseTrigger : MonoBehaviour
{
    [SerializeField] DuckNavigation _duck;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _duck.SwitchState(DuckNavigation.NavState.FINAL_CHASE);
        }
    }
}