using UnityEngine;
using UnityEngine.SceneManagement; // <-- We need this to change levels!
using System.Collections;

public class OTFinalScare : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("The Two Nurses")]
    public GameObject cornerNurse;       // The one standing in the room
    public GameObject jumpscareNurse;    // The hidden one attached to your camera

    [Header("Jumpscare Details")]
    public Animator jumpscareAnimator;   // The animator on the HIDDEN nurse
    public Transform jumpscarePoint;     // The anchor attached to your Main Camera

    [Header("Next Level Transition")]
    public string nextSceneName = "Loop2"; // EXACT name of your Loop 2 Scene

    [Header("Settings")]
    public float screamDuration = 2.0f;  // How long she screams before changing level

    private bool hasTriggered = false;

    void Start()
    {
        // Ensure the corner one is visible, and the face one is hidden at the start
        if (cornerNurse != null) cornerNurse.SetActive(true);
        if (jumpscareNurse != null) jumpscareNurse.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(ExecuteFinalScare());
        }
    }

    IEnumerator ExecuteFinalScare()
    {
        // 1. FREEZE THE PLAYER
        PlayerInteract interactScript = player.GetComponent<PlayerInteract>();
        if (interactScript != null)
        {
            interactScript.ToggleControls(false);
        }

        // 2. HIDE THE CORNER NURSE
        if (cornerNurse != null)
        {
            cornerNurse.SetActive(false);
        }

        // 3. SHOW AND SNAP THE JUMPSCARE NURSE
        if (jumpscareNurse != null)
        {
            jumpscareNurse.SetActive(true);

            if (jumpscarePoint != null)
            {
                jumpscareNurse.transform.position = jumpscarePoint.position;
                jumpscareNurse.transform.rotation = jumpscarePoint.rotation;
            }
        }

        // 4. TRIGGER THE SCREAM ANIMATION
        if (jumpscareAnimator != null)
        {
            jumpscareAnimator.SetTrigger("Scream");
        }

        // 5. WAIT FOR THE SCREAM TO FINISH
        yield return new WaitForSeconds(screamDuration);

        // 6. TRANSITION TO LOOP 2
        Cursor.lockState = CursorLockMode.None; // Optional: Unlock mouse if loading takes a second
        Cursor.visible = true;

        SceneManager.LoadScene(nextSceneName);
    }
}