using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CinematicSequence : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup blackFadeGroup;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Player Scripts")]
    public MonoBehaviour fpsMovement;
    public MonoBehaviour mouseLook;

    [Header("Camera")]
    public CameraShake shaker;

    private List<string> lines = new List<string> {
        "What happened?",
        "Ahhh, must have been an electrical shock!"
    };

    public void StartShockSequence()
    {
        StartCoroutine(ExecuteSequence());
    }

    IEnumerator ExecuteSequence()
    {
        // 1. FORCE LOCK PLAYER IMMEDIATELY
        if (fpsMovement != null) fpsMovement.enabled = false;
        // Keep mouseLook enabled as requested so they can look at the text
        if (mouseLook != null) mouseLook.enabled = true;

        // 2. SHAKE CAMERA (3 Seconds)
        if (shaker != null) yield return StartCoroutine(shaker.Shake(3f, 0.3f));
        else yield return new WaitForSeconds(3f);

        // 3. FADE TO BLACK
        // We ensure Alpha goes to 1
        if (blackFadeGroup != null) yield return StartCoroutine(Fade(0, 1, 0.5f));

        // 4. WAIT IN DARKNESS (5 Seconds)
        yield return new WaitForSeconds(5f);

        // 5. FADE OUT BLACK
        if (blackFadeGroup != null) yield return StartCoroutine(Fade(1, 0, 1f));

        // 6. DIALOGUE SYSTEM
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);

            foreach (string line in lines)
            {
                dialogueText.text = line;
                yield return null;

                // WAIT for click before moving to next line
                yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
                yield return new WaitForEndOfFrame();
            }
            dialoguePanel.SetActive(false);
        }

        // 7. FINALLY UNLOCK PLAYER
        // Only happens after all dialogue is finished
        if (fpsMovement != null) fpsMovement.enabled = true;
        Debug.Log("Cinematic Complete: Player Unlocked.");
    }

    IEnumerator Fade(float start, float end, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            blackFadeGroup.alpha = Mathf.Lerp(start, end, elapsed / time);
            yield return null;
        }
        blackFadeGroup.alpha = end;
    }
}