using UnityEngine;
using System.Collections;

public class MazeDoor : MonoBehaviour
{
    private bool hasBeenUsed = false;
    private Vector3 closedLocalPos; // Remembers where the door started

    [Header("Movement Settings")]
    [Tooltip("The direction and distance the door slides. e.g., X = -1.2 for Left, X = 1.2 for Right.")]
    public Vector3 openOffset = new Vector3(-1.2f, 0, 0);
    public float slideSpeed = 2f;

    [Header("Maze Logic")]
    public int doorIndex; // 0 for Door A, 1 for Door B
    public bool isCorrectPath;

    void Start()
    {
        // Save the starting position relative to the Anchor Parent
        closedLocalPos = transform.localPosition;
    }

    // Called by the Interaction Trigger when player presses 'E'
    public void UseDoor()
    {
        if (hasBeenUsed) return;
        hasBeenUsed = true;

        // Start sliding the door to its 'Open' position
        StopAllCoroutines();
        StartCoroutine(AnimateDoor(closedLocalPos + openOffset));

        // Tell the MazeManager to spawn the next room
        MazeManager manager = Object.FindFirstObjectByType<MazeManager>();
        if (manager != null)
        {
            // This triggers the snap logic and next room creation
            // It also tells the manager which door index we are currently moving through
            manager.OnDoorSelected(doorIndex, isCorrectPath);
        }
    }

    // Called by the MazeManager inside 'ConfirmRoomEntry' when player hits the floor trigger
    public void CloseDoor()
    {
        // Instantly snap the door back to its original closed position (Slams shut)
        StopAllCoroutines();
        transform.localPosition = closedLocalPos;

        // Ensure physics are solid so the player can't walk back out
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        // Disable the interaction script so you can't open it again
        if (transform.parent != null)
        {
            var interactScript = transform.parent.GetComponentInChildren<TriggerInteractable>();
            if (interactScript != null)
            {
                interactScript.enabled = false;
            }
        }
    }

    // This replaces the Animator. It smoothly slides the door over time.
    IEnumerator AnimateDoor(Vector3 targetPos)
    {
        float elapsed = 0;
        Vector3 startPos = transform.localPosition;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * slideSpeed;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed);
            yield return null;
        }

        // Ensure it reaches the exact target
        transform.localPosition = targetPos;
    }
}