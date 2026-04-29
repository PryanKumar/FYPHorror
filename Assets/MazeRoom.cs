using UnityEngine;

public class MazeRoom : MonoBehaviour
{
    public Transform entryAnchor;
    public Transform[] exitAnchors; // Array for multiple doors

    private bool playerHasEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        // When the player steps on the invisible floor trigger in this room
        if (!playerHasEntered && other.CompareTag("Player"))
        {
            playerHasEntered = true;

            MazeManager manager = Object.FindFirstObjectByType<MazeManager>();
            if (manager != null)
            {
                manager.ConfirmRoomEntry(this);
                Debug.Log("Entered new room: " + gameObject.name);
            }
        }
    }
}