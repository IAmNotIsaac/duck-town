using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckView : MonoBehaviour
{
    [SerializeField] private DuckNavigation _duck;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _duck.playerInViewArea = true;
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _duck.playerInViewArea = false;
        }
    }
}
