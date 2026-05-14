using UnityEngine;

public class CollisionBarrel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    /*
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Barrel_Tag")
        {
            Debug.Log("Hit Barrel");         
        }
    }
    */

    /*
    void OnCollisionEnter(Collision collision)
    {
       if (collision.collider.CompareTag("Barrel_Tag"))
        {
            Debug.Log("Hit Barrel");
            
        }
    }
    */

    // Chris - Collison script switch the barrels 

    public AudioSource BarrelAudio;
    public AudioClip BarrelClip;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Hit Barrel");
            BarrelAudio.PlayOneShot(BarrelClip);

        }
    }
}

