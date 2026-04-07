using UnityEngine;

public class ClockHandItem : MonoBehaviour
{
    [Header("UI Settings")]
    public string pickupMessage = "Found a clock hand!";

    // This can be called by your TriggerInteractable or a simple TriggerEnter
    public void PickUp()
    {
        // 1. Find the player
        PlayerInteract player = Object.FindFirstObjectByType<PlayerInteract>();

        if (player != null)
        {
            // 2. Set the boolean we added to your PlayerInteract script earlier
            player.PickUpClockHand();

            Debug.Log(pickupMessage);

            // 3. Remove the item from the world
            Destroy(gameObject);
        }
    }
}