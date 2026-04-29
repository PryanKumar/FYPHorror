using UnityEngine;

public class MannequinScareLogic : MonoBehaviour
{
    [Header("Mannequins")]
    public GameObject passiveMannequin;
    public GameObject jumpscareMannequin;

    [Header("Jumpscare Settings")]
    public Animator jumpscareAnimator;
    public string screamTriggerName = "Scream";
    public AudioSource screamAudio;
    public float screamAnimationLength = 2.0f;

    [Header("Scare Positioning")]
    [Tooltip("How far in front of the player she teleports (1.0 is usually a good scare distance)")]
    public float distanceInFrontOfPlayer = 1.0f;

    [Tooltip("Tweak this to move her perfectly into the camera's view (e.g., 0.5 for higher, -0.5 for lower)")]
    public float heightOffset = 0f; // <--- THE NEW VARIABLE!

    public void SetPassiveMode()
    {
        if (passiveMannequin != null) passiveMannequin.SetActive(true);
        if (jumpscareMannequin != null) jumpscareMannequin.SetActive(false);
    }

    public void PrepareJumpscare()
    {
        if (passiveMannequin != null) passiveMannequin.SetActive(false);
        if (jumpscareMannequin != null) jumpscareMannequin.SetActive(false);
    }

    public void ExecuteJumpscare(Transform playerTransform)
    {
        if (jumpscareMannequin != null && playerTransform != null)
        {
            // 1. Calculate the spot directly in front of the player
            Vector3 spawnPos = playerTransform.position + (playerTransform.forward * distanceInFrontOfPlayer);

            // 2. THE FIX: Set her height, and add your custom adjustment!
            spawnPos.y = jumpscareMannequin.transform.position.y + heightOffset;

            // 3. Teleport her!
            jumpscareMannequin.transform.position = spawnPos;

            // 4. Make her stare directly into the player's soul
            jumpscareMannequin.transform.LookAt(playerTransform);

            // Ensure she stands perfectly straight and doesn't tilt backwards
            Vector3 euler = jumpscareMannequin.transform.eulerAngles;
            jumpscareMannequin.transform.eulerAngles = new Vector3(0, euler.y, 0);

            // 5. BAM! Turn her on.
            jumpscareMannequin.SetActive(true);
        }

        // Trigger the scream!
        if (jumpscareAnimator != null) jumpscareAnimator.SetTrigger(screamTriggerName);
        if (screamAudio != null) screamAudio.Play();
    }
}