using UnityEngine;
using System.Collections;

public class LiftController : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorTransform;     // Drag the "Door" cube group here
    public float doorOpenX = -21.54f;   // Your specific open position
    public float doorClosedX = -12f;    // Your specific closed position
    public float doorSpeed = 3f;

    [Header("Lift Movement Settings")]
    public float bottomY = -0.08f;      // Your specific bottom floor
    public float topY = 9.55f;         // Your specific top floor
    public float liftSpeed = 2.5f;

    [Header("State Control")]
    public bool isMoving = false;
    private bool playerInside = false;

    // --- 1. PLAYER DETECTION ---

    private void OnTriggerEnter(Collider other)
    {
        // Ensure your Player object is tagged "Player" in the Inspector
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            // Only start the sequence if we aren't already moving
            if (!isMoving)
            {
                StartCoroutine(OperateLift());
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Parenting prevents the player from "sliding" or falling through the floor while moving
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Something entered the lift: " + other.name); // This will tell us IF the lift sees anything

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Tag detected! Starting lift...");
            playerInside = true;
            if (!isMoving)
            {
                StartCoroutine(OperateLift());
            }
        }
        else
        {
            Debug.Log("Object entered, but tag was: " + other.tag);
        }
    }

    // --- 2. THE LIFT SEQUENCE ---

    IEnumerator OperateLift()
    {
        isMoving = true;

        // Step A: Close the Door
        yield return StartCoroutine(MoveDoor(doorClosedX));
        yield return new WaitForSeconds(0.5f); // Psychological pause

        // Step B: Move the entire Lift to the destination
        // We use world position for the Lift so it travels through the level
        float targetY = (transform.position.y < (topY - 1f)) ? topY : bottomY;
        yield return StartCoroutine(MoveLift(targetY));
        yield return new WaitForSeconds(0.5f);

        // Step C: Open the Door
        yield return StartCoroutine(MoveDoor(doorOpenX));

        isMoving = false;
    }

    // --- 3. HELPER MOVEMENT FUNCTIONS ---

    IEnumerator MoveDoor(float targetX)
    {
        // Uses localPosition so the door moves relative to the moving lift
        while (Mathf.Abs(doorTransform.localPosition.x - targetX) > 0.01f)
        {
            Vector3 pos = doorTransform.localPosition;
            pos.x = Mathf.MoveTowards(pos.x, targetX, doorSpeed * Time.deltaTime);
            doorTransform.localPosition = pos;
            yield return null;
        }
        // Snap to final position to be clean
        doorTransform.localPosition = new Vector3(targetX, doorTransform.localPosition.y, doorTransform.localPosition.z);
    }

    IEnumerator MoveLift(float targetY)
    {
        while (Mathf.Abs(transform.position.y - targetY) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, liftSpeed * Time.deltaTime);
            transform.position = pos;
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }
}