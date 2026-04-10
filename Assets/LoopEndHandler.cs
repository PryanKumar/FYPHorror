using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoopEndHandler : MonoBehaviour
{
    [Header("References")]
    public Transform doorTransform;    // StoreDoor_Parent
    public GameObject endCutsceneCam; // The camera showing the door closing
    public GameObject mainFPSCamera;
    public GameObject playerController;
    public CanvasGroup fadeCanvas;    // For the fade to black

    [Header("Settings")]
    public float closeXPos = 570.784f;
    public float slideSpeed = 2f;
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

        // 2. Close the door
        Vector3 startPos = doorTransform.position;
        Vector3 endPos = new Vector3(closeXPos, startPos.y, startPos.z);
        float elapsed = 0;

        while (elapsed < 1.0f)
        {
            doorTransform.position = Vector3.Lerp(startPos, endPos, elapsed);
            elapsed += Time.deltaTime * slideSpeed;
            yield return null;
        }
        doorTransform.position = endPos;

        yield return new WaitForSeconds(1.5f); // Dramatic pause

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

        // 4. Load Next Loop
        SceneManager.LoadScene(nextLoopScene);
    }
}