using UnityEngine;

public class BloodDrip : MonoBehaviour
{
    [Header("Drip Settings")]
    public float baseDripSpeed = 0.05f;
    public float maxDripTime = 3f; // How many seconds it drips before drying

    private bool isOnWall = false;
    private float currentDripTime = 0f;
    private float actualDripSpeed;

    void Start()
    {
        // Check if the surface is vertical (a wall)
        if (Mathf.Abs(transform.forward.y) < 0.1f)
        {
            isOnWall = true;

            // Randomize the speed and time slightly so they look organic, not robotic
            actualDripSpeed = baseDripSpeed * Random.Range(0.5f, 1.5f);
            maxDripTime *= Random.Range(0.5f, 1.2f);
        }
    }

    void Update()
    {
        // Only move IF it's on a wall AND the timer hasn't run out
        if (isOnWall && currentDripTime < maxDripTime)
        {
            // Move down globally
            transform.position += Vector3.down * actualDripSpeed * Time.deltaTime;

            // Add to our timer
            currentDripTime += Time.deltaTime;
        }
        else if (currentDripTime >= maxDripTime)
        {
            // Optional: Once it stops dripping, we can turn off this script entirely 
            // to save computer memory (since it doesn't need to run Update anymore)
            this.enabled = false;
        }
    }
}