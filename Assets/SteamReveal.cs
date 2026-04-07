using UnityEngine;
using TMPro;

public class SteamReveal : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem steamParticles;
    public TextMeshPro codeText;
    public AudioSource steamSound;

    [Header("Cinematic Camera Settings")]
    public GameObject mainFPSCamera;    // Drag the WHOLE Main Camera object here
    public GameObject cutsceneCamera;  // Drag the WHOLE Cutscene Camera object here
    public float cutsceneDuration = 4f;
    public GameObject playerController;

    [Header("Timing Settings")]
    public float delayBeforeSteam = 1f;
    public float delayBeforeNumber = 2f;
    public float steamDuration = 10f;
    public float fadeSpeed = 0.5f;

    private bool isRevealed = false;

    public void ActivateReveal()
    {
        if (isRevealed) return;
        StartCoroutine(SteamSequence());
    }

    System.Collections.IEnumerator SteamSequence()
    {
        isRevealed = true;

        yield return new WaitForSeconds(delayBeforeSteam);

        // 1. SWITCH TO CUTSCENE
        ToggleCinematicMode(true);

        if (steamParticles != null) steamParticles.Play();
        if (steamSound != null) steamSound.Play();

        yield return new WaitForSeconds(cutsceneDuration);

        // 2. SWITCH BACK TO PLAYER
        ToggleCinematicMode(false);

        yield return new WaitForSeconds(delayBeforeNumber);

        float alpha = 0;
        Color c = codeText.color;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            codeText.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(steamDuration);

        if (steamParticles != null) steamParticles.Stop();
        if (steamSound != null) steamSound.Stop();
    }

    void ToggleCinematicMode(bool isCinematic)
    {
        // We use .SetActive for the whole object to ensure the display updates
        if (cutsceneCamera != null) cutsceneCamera.SetActive(isCinematic);
        if (mainFPSCamera != null) mainFPSCamera.SetActive(!isCinematic);

        // Lock player movement
        if (playerController != null)
        {
            // Try to find any movement script on the player or its children
            var moveScript = playerController.GetComponentInChildren<MonoBehaviour>();
            if (moveScript != null && moveScript.GetType().Name.Contains("Movement") || moveScript.GetType().Name.Contains("Controller"))
            {
                moveScript.enabled = !isCinematic;
            }
        }
    }
}