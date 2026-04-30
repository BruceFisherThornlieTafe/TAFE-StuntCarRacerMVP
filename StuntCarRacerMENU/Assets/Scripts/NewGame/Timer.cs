using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    // Callum Greenshields 30728074

    [SerializeField] TextMeshProUGUI timerDisplay;

    [SerializeField]  float currentTime = 0f;

    void Start()
    {
        currentTime = 0f;                       // Sets timer to 0
    }

    void Update()
    {
        DisplayTimer(currentTime);              // Updates timer every frame
    }

    private void DisplayTimer(float time)       // Takes the current time, adds Time.deltaTime, incase of in the future, the current time of a lap is to be saved.
    {
    currentTime = time + Time.deltaTime;

        timerDisplay.text = $"Time: {currentTime.ToString("F2")}";
    }

    // FUTURE: Toggle Timer
}
