using UnityEngine;
using System.Collections;

public class GeneratorLogic : MonoBehaviour
{
    [Header("References")]
    public Animator leverAnimator;
    public GameObject lightsParent;
    public GameObject sprinklerParent;
    public AudioSource engineSound;

    [Header("Door Control")]
    // THE FIX: We use the Animator now instead of the Transform!
    public Animator otDoorAnimator;

    [Header("Cinematic Settings")]
    public GameObject mainFPSCamera;
    public GameObject cutsceneCamera;
    public GameObject playerController;

    [Header("Timing")]
    public string animationTrigger = "SwitchOn";
    public float initialDelay = 2f;
    public float sprinklerDelay = 1f;
    public float postSequenceDelay = 2f;

    private bool isPowerOn = false;

    public void Interacted()
    {
        if (isPowerOn) return;
        isPowerOn = true;
        StartCoroutine(GeneratorSequence());
    }

    IEnumerator GeneratorSequence()
    {
        if (leverAnimator != null) leverAnimator.SetTrigger(animationTrigger);
        if (engineSound != null) engineSound.Play();

        ToggleCinematicMode(true);
        yield return new WaitForSeconds(initialDelay);

        if (lightsParent != null) lightsParent.SetActive(true);
        yield return new WaitForSeconds(sprinklerDelay);

        if (sprinklerParent != null) sprinklerParent.SetActive(true);
        yield return new WaitForSeconds(postSequenceDelay);

        ToggleCinematicMode(false);

        // --- THE FIX: Let the Master Prefab's Animator handle the opening! ---
        if (otDoorAnimator != null)
        {
            otDoorAnimator.SetBool("isOpen", true);
        }

        // Cleanup interaction so the player can't keep pulling the lever
        TriggerInteractable trigger = GetComponent<TriggerInteractable>();
        if (trigger != null) trigger.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void ToggleCinematicMode(bool isCinematic)
    {
        if (cutsceneCamera != null) cutsceneCamera.SetActive(isCinematic);
        if (mainFPSCamera != null) mainFPSCamera.SetActive(!isCinematic);

        if (playerController != null)
        {
            var moveScript = playerController.GetComponentInChildren<MonoBehaviour>();
            if (moveScript != null) moveScript.enabled = !isCinematic;
        }
    }
}