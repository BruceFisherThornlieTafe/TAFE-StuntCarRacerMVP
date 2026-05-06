using UnityEngine;
using UnityEngine.UI;

public class LapInteraction : MonoBehaviour
{
    [SerializeField] Text LapDisplay;

    private int remainingLaps = 0;

    public GameObject lapInteract;
    public Text lapText;
    public int Score;
    public int maxLap = 3;
    public 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Score++;
            remainingLaps += 1;
            LapDisplay.text = $"Lap: {remainingLaps} / 3";
        }
    }

    // Source:
    // https://www.youtube.com/watch?v=lBMqlLcQmuE
}
