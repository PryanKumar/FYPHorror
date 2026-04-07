using UnityEngine;
using System.Collections;

public class GhostSighting : MonoBehaviour
{
    [Header("Ghost & Scare Objects")]
    public GameObject ghostObject;
    public Animator bedAnimator;
    public string bedAnimationTrigger = "Launch";

    [Header("Movement Settings")]
    public Transform targetPoint;
    public float moveSpeed = 3f;

    public enum RequiredItem { None, Key, Fuse, FusePlaced }
    [Header("Requirements")]
    public RequiredItem itemNeeded;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            PlayerInteract interactScript = other.GetComponent<PlayerInteract>();

            if (interactScript != null)
            {
                // DEBUG: This tells us exactly what the player has when they hit the box
                Debug.Log($"Checking Scare: Needed={itemNeeded} | PlayerHasFuse={interactScript.hasFuse} | PlayerHasKey={interactScript.hasKey}");

                bool conditionMet = false;

                if (itemNeeded == RequiredItem.Key)
                    conditionMet = interactScript.hasKey;
                else if (itemNeeded == RequiredItem.Fuse)
                    conditionMet = interactScript.hasFuse;
                else if (itemNeeded == RequiredItem.FusePlaced)
                    conditionMet = interactScript.isPowerOn;
                else if (itemNeeded == RequiredItem.None)
                    conditionMet = true;

                if (conditionMet)
                {
                    Debug.Log("<color=green>SCARE VALIDATED:</color> Starting ghost sequence.");
                    StartCoroutine(TriggerScareSequence());
                }
                else
                {
                    // If this prints, the ghost WON'T show up.
                    Debug.Log("<color=red>SCARE REJECTED:</color> Player does not have the required item yet.");
                }
            }
        }
    }

    IEnumerator TriggerScareSequence()
    {
        hasTriggered = true; // Mark as fired immediately

        if (ghostObject != null)
        {
            ghostObject.SetActive(true);
            Animator anim = ghostObject.GetComponent<Animator>();
            if (anim == null) anim = ghostObject.GetComponentInChildren<Animator>();

            if (anim != null)
            {
                string animTrigger = (itemNeeded == RequiredItem.Key) ? "Run" : "Walk";
                anim.SetTrigger(animTrigger);
            }

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

            yield return new WaitForSeconds(0.5f);
            ghostObject.SetActive(false);
        }

        if (bedAnimator != null)
        {
            bedAnimator.SetTrigger(bedAnimationTrigger);
        }

        Destroy(gameObject, 1f);
    }
}