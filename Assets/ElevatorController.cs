using UnityEngine;
using System.Collections;

public class ElevatorController : MonoBehaviour
{
    [Header("Door References")]
    public Transform bigDoor;   // Drag 'Lift Door Big' here
    public Transform smallDoor; // Drag 'Lift Door Small' here
    public CinematicSequence cinematic;

    [Header("Big Door X Positions")]
    public float bigDoorOpenX = 10.6f;
    public float bigDoorClosedX = 11.576f;

    [Header("Small Door X Positions")]
    public float smallDoorOpenX = 10.53f;
    public float smallDoorClosedX = 12.56f;

    [Header("Settings")]
    public float slideSpeed = 2f;

    private bool isRunningSequence = false;
    private PlatformMoving movementScript;

    void Start()
    {
        movementScript = GetComponent<PlatformMoving>();

        // Ensure doors start in the open position
        SetDoorX(bigDoor, bigDoorOpenX);
        SetDoorX(smallDoor, smallDoorOpenX);
    }

    public void ActivateElevator()
    {
        if (!isRunningSequence)
        {
            if (cinematic != null)
            {
                cinematic.StartShockSequence();
            }

            StartCoroutine(ElevatorSequence());
        }
    }

    IEnumerator ElevatorSequence()
    {
        isRunningSequence = true;

        // 1. Close both doors
        yield return StartCoroutine(MoveBothDoors(bigDoorClosedX, smallDoorClosedX));

        // 2. Start movement
        if (movementScript != null)
        {
            movementScript.canMove = true;
        }

        // 3. Wait for arrival
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => movementScript.HasReachedTarget());

        // 4. Open both doors
        yield return StartCoroutine(MoveBothDoors(bigDoorOpenX, smallDoorOpenX));

        isRunningSequence = false;
    }

    IEnumerator MoveBothDoors(float targetBigX, float targetSmallX)
    {
        bool bigDoorArrived = false;
        bool smallDoorArrived = false;

        while (!bigDoorArrived || !smallDoorArrived)
        {
            // Move Big Door
            float newBigX = Mathf.MoveTowards(bigDoor.localPosition.x, targetBigX, slideSpeed * Time.deltaTime);
            SetDoorX(bigDoor, newBigX);
            if (Mathf.Abs(newBigX - targetBigX) < 0.001f) bigDoorArrived = true;

            // Move Small Door
            float newSmallX = Mathf.MoveTowards(smallDoor.localPosition.x, targetSmallX, slideSpeed * Time.deltaTime);
            SetDoorX(smallDoor, newSmallX);
            if (Mathf.Abs(newSmallX - targetSmallX) < 0.001f) smallDoorArrived = true;

            yield return null;
        }

        // Snap to final positions to be precise
        SetDoorX(bigDoor, targetBigX);
        SetDoorX(smallDoor, targetSmallX);
    }

    // Helper method to update only the X position while keeping Y and Z the same
    void SetDoorX(Transform door, float x)
    {
        if (door != null)
        {
            Vector3 pos = door.localPosition;
            pos.x = x;
            door.localPosition = pos;
        }
    }
}