using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Automatically find the player's camera
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Forces the UI to perfectly face the camera at all times
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}