using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoopEndHandler : MonoBehaviour
{
    [Header("References")]
    // THE FIX: Use the Animator instead of the Transform
    public Animator otDoorAnimator;
    public GameObject endCutsceneCam;
    public GameObject mainFPSCamera;
    public GameObject playerController;
    public CanvasGroup fadeCanvas;

    [Header("Settings")]
    public string nextLoopScene = "Loop3";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(EndSequence());
        }
    }

    IEnumerator EndSequence()
    {
        // 1. Swap Cameras & Freeze Player
        if (endCutsceneCam != null) endCutsceneCam.SetActive(true);
        if (mainFPSCamera != null) mainFPSCamera.SetActive(false);
        if (playerController != null) playerController.SetActive(false);

        // 2. SLAM THE DOOR SHUT using the Animator
        if (otDoorAnimator != null)
        {
            otDoorAnimator.SetBool("isOpen", false);
        }

        // Dramatic pause to watch the door close
        yield return new WaitForSeconds(1.5f);

        // 3. Fade to Black
        if (fadeCanvas != null)
        {
            float fadeTimer = 0;
            while (fadeTimer < 1f)
            {
                fadeTimer += Time.deltaTime;
                fadeCanvas.alpha = fadeTimer;
                yield return null;
            }
        }

        // Add a tiny half-second pause in pure darkness before the scene swap for max creepiness
        yield return new WaitForSeconds(0.5f);

        // 4. Load Next Loop
        SceneManager.LoadScene(nextLoopScene);
    }
}