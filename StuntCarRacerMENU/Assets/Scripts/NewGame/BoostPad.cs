using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [Tooltip("How long the boost lasts in seconds after the car hits this pad.")]
    public float boostDuration = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null) return;

        player.TriggerBoostPad(boostDuration);
    }
}