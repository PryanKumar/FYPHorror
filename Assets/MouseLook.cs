using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float mouseSensitivity = 100f;
    public float smoothTime = 0.05f;
    public Transform playerBody;

    [Header("Head Bob Settings")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.11f;

    [Header("Landing Impact")]
    public float landShakeMagnitude = 0.2f;
    public float landShakeDuration = 0.15f;

    float xRotation = 0f;
    float yRotation = 0f;

    float defaultYPos;
    float timer;

    Vector2 currentMouseDelta;
    Vector2 currentMouseDeltaVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        defaultYPos = transform.localPosition.y;
        yRotation = playerBody.eulerAngles.y;
    }

    void OnEnable()
    {
        LockCursor();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("<color=orange>MouseLook:</color> Cursor Force Locked.");
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        Vector2 targetMouseDelta = new Vector2(mouseX, mouseY);
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, smoothTime);

        xRotation -= currentMouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        yRotation += currentMouseDelta.x;
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);

        HandleHeadBob();
    }

    void HandleHeadBob()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float speed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float amount = isSprinting ? sprintBobAmount : walkBobAmount;

            timer += Time.deltaTime * speed;

            transform.localPosition = new Vector3(
                transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * amount,
                transform.localPosition.z
            );
        }
        else
        {
            timer = 0;
            Vector3 targetPos = new Vector3(transform.localPosition.x, defaultYPos, transform.localPosition.z);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 8f);
        }
    }

    public void TriggerLandShake()
    {
        // Don't interrupt a shock shake with a landing shake
        StartCoroutine(LandShakeAction());
    }

    IEnumerator LandShakeAction()
    {
        float elapsed = 0f;

        while (elapsed < landShakeDuration)
        {
            float t = elapsed / landShakeDuration;
            float yOffset = Mathf.Sin(t * Mathf.PI) * -landShakeMagnitude;

            transform.localPosition = new Vector3(transform.localPosition.x, defaultYPos + yOffset, transform.localPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = new Vector3(transform.localPosition.x, defaultYPos, transform.localPosition.z);
    }

    // --- NEW: ELECTRIC SHOCK SHAKE ---
    public void TriggerShockShake(float duration, float magnitude)
    {
        // Stop current headbobs or land shakes to prioritize the shock
        StopAllCoroutines();
        StartCoroutine(ShockShakeAction(duration, magnitude));
    }

    IEnumerator ShockShakeAction(float duration, float magnitude)
    {
        float elapsed = 0f;
        Vector3 originalPos = new Vector3(transform.localPosition.x, defaultYPos, transform.localPosition.z);

        while (elapsed < duration)
        {
            // Generates a random tiny offset in all directions for a "vibration" effect
            float xOffset = Random.Range(-1f, 1f) * magnitude;
            float yOffset = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + xOffset, defaultYPos + yOffset, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}