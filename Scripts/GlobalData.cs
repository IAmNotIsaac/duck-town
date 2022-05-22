using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour
{
    public static float PlayerRotationSpeedVert = 400.0f;
    public static float PlayerRotationSpeedHorz = 400.0f;


    public static void LockMouse() {
        Cursor.lockState = CursorLockMode.Locked;
    }


    public static void FreeMouse() {
        Cursor.lockState = CursorLockMode.None;
    }
}
