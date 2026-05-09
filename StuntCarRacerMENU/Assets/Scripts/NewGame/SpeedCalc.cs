using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SpeedCalc : MonoBehaviour
{
    //Elise Avery 30128123
    [SerializeField] Rigidbody carBody;
    [SerializeField] TextMeshProUGUI speedDisplay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DisplayText();
    }

    void DisplayText()
    {
        //sets the speed display text to the velocity magnitude in unity measurements (meters) *3.6 to be KM/h, converted to a short string with no decimals
        speedDisplay.text = (carBody.linearVelocity.magnitude * 3.6f).ToString("F0");
        
        
    }
}
