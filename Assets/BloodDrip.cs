using UnityEngine;

public class BloodDrip : MonoBehaviour
{
    public float dripSpeed = 0.02f;
    private bool isOnWall = false;

    void Start()
    {
        // Check if the surface is vertical (a wall)
        if (Mathf.Abs(transform.forward.y) < 0.1f)
        {
            isOnWall = true;
        }
    }

    void Update()
    {
        if (isOnWall)
        {
            // Move down globally
            transform.position += Vector3.down * dripSpeed * Time.deltaTime;
        }
    }
}