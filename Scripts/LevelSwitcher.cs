using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSwitcher : MonoBehaviour
{
    [SerializeField] private int _nextLevelID = 0;


    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player)
        {
            player.SwitchLevel(_nextLevelID);
        }
    }
}
