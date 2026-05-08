using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LapUI : MonoBehaviour
{
    [SerializeField] Text LapDisplay;

    private int remainingLaps = 0;

    public GameObject lapInteract;
    public Text lapText;
    public int Score;
    public int maxLap = 3;
    public GameObject Lap3WinUI;

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
        if (other.gameObject.CompareTag("lapInteract"))
        {
            EventBus.LapIncrease();

            remainingLaps += 1;

            if (remainingLaps <= maxLap)
            {
                Score++;
                LapDisplay.text = $"Lap: {remainingLaps} / {maxLap}";
            }
            else
            {
                Lap3WinUI.SetActive(true);
                Debug.Log("I win");
            }

        }
    }
    //private void ()
    //{
    //    EventBus.LapIncrease();

    //    remainingLaps += 1;

    //    if (remainingLaps <= maxLap)
    //    {
    //        Score++;
    //        LapDisplay.text = $"Lap: {remainingLaps} / {maxLap}";
    //    }
    //    else
    //    {
    //        Lap3WinUI.SetActive(true);
    //        Debug.Log("I win");
    //    }
    //}




}
