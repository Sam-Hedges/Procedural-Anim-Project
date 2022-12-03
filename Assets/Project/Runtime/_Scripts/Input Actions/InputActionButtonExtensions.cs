using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Used to extend the functionality of the unity input system and use it akin to the 
// legacy system
public static class InputActionButtonExtensions 
{
    public static bool GetButton(this InputAction action) => action.ReadValue<float>() > 0;
    public static bool GetButtonDown(this InputAction action) => action.triggered && action.ReadValue<float>() > 0;
    public static bool GetButtonUp(this InputAction action) => action.triggered && action.ReadValue<float>() == 0;
}
