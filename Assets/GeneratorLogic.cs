using UnityEngine;
using System.Collections;

public class GeneratorLogic : MonoBehaviour
{
    [Header("References")]
    public Animator leverAnimator;
    public GameObject lightsParent;
    public GameObject sprinklerParent;
    public AudioSource engineSound;

    [Header("Cinematic Settings")]
    public GameObject mainFPSCamera;    // Drag the WHOLE Main Camera object here
    public GameObject cutsceneCamera;   // Drag the WHOLE Cutscene Camera object here
    public GameObject playerController; // Drag your Player object here

    [Header("Timing")]
    public string animationTrigger = "SwitchOn";
    public float initialDelay = 2f;     // Time looking at room before lights
    public float sprinklerDelay = 1f;   // Time between lights and sprinklers
    public float postSequenceDelay = 2f; // Time to look at everything before switching back

    private bool isPowerOn = false;

    public void Interacted()
    {
        if (isPowerOn) return;

        // Safety Check
        if (mainFPSCamera == null || cutsceneCamera == null)
        {
            Debug.LogError("Generator Error: Assign both Camera OBJECTS in the Inspector!");
            return;
        }

        isPowerOn = true;
        StartCoroutine(GeneratorSequence());
    }

    IEnumerator GeneratorSequence()
    {
        // 1. Pull lever and play sound
        if (leverAnimator != null) leverAnimator.SetTrigger(animationTrigger);
        if (engineSound != null) engineSound.Play();

        // 2. SWITCH TO CINEMATIC (Matching SteamReveal style)
        ToggleCinematicMode(true);

        // 3. First Wait
        yield return new WaitForSeconds(initialDelay);

        // 4. Lights On
        if (lightsParent != null) lightsParent.SetActive(true);

        // 5. Second Wait
        yield return new WaitForSeconds(sprinklerDelay);

        // 6. Sprinklers On
        if (sprinklerParent != null) sprinklerParent.SetActive(true);

        // 7. Final Look
        yield return new WaitForSeconds(postSequenceDelay);

        // 8. RETURN TO PLAYER
        ToggleCinematicMode(false);

        // Cleanup UI from the Trigger script
        TriggerInteractable trigger = GetComponent<TriggerInteractable>();
        if (trigger != null)
        {
            if (trigger.interactionUI != null) trigger.interactionUI.gameObject.SetActive(false);
            trigger.enabled = false;
        }

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void ToggleCinematicMode(bool isCinematic)
    {
        // Using SetActive on the GameObjects is much more reliable for Unity's display
        if (cutsceneCamera != null) cutsceneCamera.SetActive(isCinematic);
        if (mainFPSCamera != null) mainFPSCamera.SetActive(!isCinematic);

        // Lock player movement so they don't walk away during the cutscene
        if (playerController != null)
        {
            // This tries to disable the movement script automatically
            var moveScript = playerController.GetComponentInChildren<MonoBehaviour>();
            // If your movement script has "Movement" or "Controller" in the name, this handles it
            if (moveScript != null && (moveScript.GetType().Name.Contains("Movement") || moveScript.GetType().Name.Contains("Controller")))
            {
                moveScript.enabled = !isCinematic;
            }
        }
    }
}