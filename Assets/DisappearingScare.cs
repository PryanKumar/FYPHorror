using UnityEngine;
using System.Collections;

public class DisappearingScare : MonoBehaviour
{
    [Header("References")]
    public PlayerInteract playerInteract; // We need this to check for the fuse

    [Header("Scare Targets")]
    public GameObject demonToHide;
    public Light redLight;

    [Header("Settings")]
    public float lightOffTime = 3.0f; // How long the light stays off

    private bool hasTriggered = false;
    private bool isScareReady = false; // Becomes true when the fuse is collected

    void Start()
    {
        // 1. Hide the demon the moment the game starts
        if (demonToHide != null)
        {
            demonToHide.SetActive(false);
        }
    }

    void Update()
    {
        // 2. Constantly check if the player has picked up the fuse
        if (!isScareReady && playerInteract != null)
        {
            if (playerInteract.hasFuse)
            {
                // The fuse was picked up! Arm the trap and spawn the demon.
                isScareReady = true;

                if (demonToHide != null)
                {
                    demonToHide.SetActive(true);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 3. Only trigger the scare IF the fuse was picked up AND it's the player
        if (isScareReady && !hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(ExecuteScareSequence());
        }
    }

    IEnumerator ExecuteScareSequence()
    {
        // Turn off the light and hide the demon
        if (redLight != null) redLight.enabled = false;
        if (demonToHide != null) demonToHide.SetActive(false);

        // Wait in the dark for 3 seconds
        yield return new WaitForSeconds(lightOffTime);

        // Turn the light back on (the demon stays dead/hidden)
        if (redLight != null) redLight.enabled = true;

        // Destroy this trigger so it cleans up your Hierarchy
        Destroy(gameObject);
    }
}