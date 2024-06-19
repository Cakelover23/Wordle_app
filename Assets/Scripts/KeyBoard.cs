using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardHandler : MonoBehaviour
{
    // Reference to the input field or any other object you want to interact with
    public InputField inputField;

    // Method to be called when the "A" key or button is pressed
    public void OnAPressed()
    {
        // Simulate pressing the "A" key or perform any action you want
        Debug.Log("A key or button pressed");
        
        // Example: Add the letter "A" to the input field
        if (inputField != null)
        {
            inputField.text += "A";
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the "A" key is pressed
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnAPressed();
        }
    }
}