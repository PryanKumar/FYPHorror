using UnityEngine;

public class TriggerPlatform : MonoBehaviour
{
    // Drag Cube (4) into this slot in the Inspector!
    public PlatformMoving platform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if we actually assigned the platform to avoid the crash
            if (platform != null)
            {
                platform.canMove = true;
            }
            else
            {
                Debug.LogError("TriggerPlatform: You forgot to drag the platform into the slot!");
            }
        }
    }
}