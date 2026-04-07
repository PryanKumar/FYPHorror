using UnityEngine;

public class PlatformMoving : MonoBehaviour
{
    public bool canMove = false;

    [SerializeField] float speed = 2f;
    [SerializeField] Transform[] points;

    private int targetIndex = 0;

    void Start()
    {
        if (points.Length > 0)
        {
            // Set initial position to the first point
            transform.position = points[0].position;
            // The first time we move, we want to go to the OTHER point
            targetIndex = 1;
        }
    }

    void Update()
    {
        if (points.Length < 2 || !canMove) return;

        // Move the platform toward the current targetIndex
        transform.position = Vector3.MoveTowards(transform.position, points[targetIndex].position, speed * Time.deltaTime);

        // Check if we arrived at the destination
        if (Vector3.Distance(transform.position, points[targetIndex].position) < 0.05f)
        {
            canMove = false; // Stop the movement
            Debug.Log("Platform reached point: " + targetIndex);

            // Prepare the target for the NEXT journey (swap 0 and 1)
            targetIndex = (targetIndex == 0) ? 1 : 0;
        }
    }

    public bool HasReachedTarget()
    {
        return !canMove;
    }
}