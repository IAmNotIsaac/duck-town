using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [SerializeField] private DoorManager _door;

    public void Interact()
    {
        _door.Open();
    }
}
