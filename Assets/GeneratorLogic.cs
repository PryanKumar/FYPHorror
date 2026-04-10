using UnityEngine;
using System.Collections;

public class GeneratorLogic : MonoBehaviour
{
    [Header("References")]
    public Animator leverAnimator;
    public GameObject lightsParent;
    public GameObject sprinklerParent;
    public AudioSource engineSound;

    [Header("New Door Logic")]
    public Transform doorTransform; // Drag StoreDoor_Parent here
    public float openXPos = 572.332f;
    public float slideSpeed = 2f;

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

        // --- NEW POSITION-BASED DOOR OPENING ---
        if (doorTransform != null)
        {
            yield return StartCoroutine(SlideDoor(openXPos));
        }

        // Cleanup interaction
        TriggerInteractable trigger = GetComponent<TriggerInteractable>();
        if (trigger != null) trigger.enabled = false;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    IEnumerator SlideDoor(float targetX)
    {
        Vector3 startPos = doorTransform.position;
        Vector3 endPos = new Vector3(targetX, startPos.y, startPos.z);
        float elapsed = 0;

        while (elapsed < 1.0f)
        {
            doorTransform.position = Vector3.Lerp(startPos, endPos, elapsed);
            elapsed += Time.deltaTime * slideSpeed;
            yield return null;
        }
        doorTransform.position = endPos;
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