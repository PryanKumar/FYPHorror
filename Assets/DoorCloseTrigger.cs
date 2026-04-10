using UnityEngine;

public class DoorCloseTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Stop if already triggered
        if (hasTriggered) return;

        // Make sure the player has the "Player" tag!
        if (other.CompareTag("Player"))
        {
            MazeManager manager = FindFirstObjectByType<MazeManager>();

            if (manager != null)
            {
                // Tell the manager to shut the previous door and update current room
                manager.ConfirmRoomEntry(GetComponentInParent<MazeRoom>());
                hasTriggered = true;
                Debug.Log("Player entered new room. Triggering door slam...");
            }
            else
            {
                Debug.LogError("DoorCloseTrigger: Could not find MazeManager in scene!");
            }
        }
    }
}