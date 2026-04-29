using UnityEngine;

public class MazeDoor : MonoBehaviour
{
    private bool hasBeenUsed = false;

    [Header("Door Control")]
    public Animator doorAnimator;

    [Header("Maze Logic")]
    public int doorIndex; // 0 for Door A, 1 for Door B
    public bool isCorrectPath;

    // Called by the Interaction Trigger when player presses 'E'
    public void UseDoor()
    {
        if (hasBeenUsed) return;
        hasBeenUsed = true;

        if (doorAnimator != null)
        {
            doorAnimator.SetBool("isOpen", true);
        }

        MazeManager manager = Object.FindFirstObjectByType<MazeManager>();
        if (manager != null)
        {
            // THE FIX: Tell the manager exactly which door we just opened!
            manager.lastUsedDoor = this;
            manager.OnDoorSelected(doorIndex, isCorrectPath);
        }
    }

    public void CloseDoor()
    {
        // 1. Slam the door shut behind the player
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("isOpen", false);
        }

        // 2. Ensure physics are solid so the player can't walk back out
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        // 3. THE FIX: Safely disable the interaction script on THIS door
        TriggerInteractable interactScript = GetComponent<TriggerInteractable>();
        if (interactScript != null)
        {
            interactScript.enabled = false;
        }
    }
}