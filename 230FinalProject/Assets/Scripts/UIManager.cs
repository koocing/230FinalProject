using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// A very simple UI manager. Used only to display the players speed
/// </summary>
public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI speedText; // Reference to the speed text box

    // Make it a singleton so it can be called in the player's update
    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Displays the given speed to the UI
    public void UpdateSpeed(float speed)
    {
        speedText.text = "Speed: " + speed.ToString("F1"); // Display speed with one decimal place
    }
}