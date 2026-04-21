using UnityEngine;
using System.Collections;

public class GhostSighting : MonoBehaviour
{
    [Header("Ghost & Scare Objects")]
    public GameObject ghostObject;       // Drag your new Nurse child object here
    public Animator bedAnimator;         // Optional: For the bed jumpscare
    public string bedAnimationTrigger = "Launch";

    [Header("Movement Settings")]
    public Transform targetPoint;        // Where the Nurse walks/runs to
    public float moveSpeed = 3f;         // Speed of the Nurse

    public enum RequiredItem { None, Key, Fuse, FusePlaced }
    [Header("Requirements")]
    public RequiredItem itemNeeded;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        // 1. Check if it's the Player and the scare hasn't happened yet
        if (other.CompareTag("Player") && !hasTriggered)
        {
            PlayerInteract interactScript = other.GetComponent<PlayerInteract>();

            if (interactScript != null)
            {
                // DEBUG: Helps you check if the logic is working in the console
                Debug.Log($"Checking Scare: Needed={itemNeeded} | PlayerHasFuse={interactScript.hasFuse} | PlayerHasKey={interactScript.hasKey}");

                bool conditionMet = false;

                // 2. Validate if the player has the right items
                if (itemNeeded == RequiredItem.Key)
                    conditionMet = interactScript.hasKey;
                else if (itemNeeded == RequiredItem.Fuse)
                    conditionMet = interactScript.hasFuse;
                else if (itemNeeded == RequiredItem.FusePlaced)
                    conditionMet = interactScript.isPowerOn;
                else if (itemNeeded == RequiredItem.None)
                    conditionMet = true;

                // 3. Execute the scare if conditions are met
                if (conditionMet)
                {
                    Debug.Log("<color=green>SCARE VALIDATED:</color> Starting Nurse sequence.");
                    StartCoroutine(TriggerScareSequence());
                }
                else
                {
                    Debug.Log("<color=red>SCARE REJECTED:</color> Player does not have the required item yet.");
                }
            }
        }
    }

    IEnumerator TriggerScareSequence()
    {
        hasTriggered = true; // Lock the trigger so it doesn't fire twice

        if (ghostObject != null)
        {
            // Turn the Nurse on
            ghostObject.SetActive(true);

            // Get her Animator component
            Animator anim = ghostObject.GetComponent<Animator>();
            if (anim == null) anim = ghostObject.GetComponentInChildren<Animator>();

            // Trigger the correct animation (Walk or Run)
            if (anim != null)
            {
                string animTrigger = (itemNeeded == RequiredItem.Key) ? "Run" : "Walk";
                anim.SetTrigger(animTrigger);
            }

            // Move the Nurse towards the target point smoothly
            if (targetPoint != null)
            {
                while (Vector3.Distance(ghostObject.transform.position, targetPoint.position) > 0.2f)
                {
                    ghostObject.transform.position = Vector3.MoveTowards(
                        ghostObject.transform.position,
                        targetPoint.position,
                        moveSpeed * Time.deltaTime
                    );
                    yield return null;
                }
            }

            // Wait half a second after she reaches the end, then disappear
            yield return new WaitForSeconds(0.5f);
            ghostObject.SetActive(false);
        }

        // Optional: Trigger a physics prop (like the hospital bed)
        if (bedAnimator != null)
        {
            bedAnimator.SetTrigger(bedAnimationTrigger);
        }

        // Clean up the trigger box from the scene to save memory
        Destroy(gameObject, 1f);
    }
}