using UnityEngine;
using System;


public class InputReader : MonoBehaviour
{
    public Action<KeyCode> OnKeyPressed;

    void Update()
    {
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                OnKeyPressed?.Invoke(key);
                break;
            }
        }
    }
}