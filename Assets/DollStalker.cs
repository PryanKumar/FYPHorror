using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DollStalker : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera playerCamera;
    public Transform stalkStartPosition;
    public GameObject gameOverUI;

    [Header("Stalker Settings")]
    public float moveSpeed = 4f;
    public float killDistance = 1.5f;
    public LayerMask obstacleMask;

    [Header("Jumpscare Polish")]
    public Animator dollAnimator;
    public Transform jumpscarePoint; // The empty object under your camera
    public float screamDuration = 2.0f; // How long the animation takes before UI shows up

    private NavMeshAgent agent;
    private bool isActive = false;
    private bool isVisible = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.isStopped = true;
    }

    void Update()
    {
        if (!isActive) return;

        CheckVisibility();

        if (!isVisible)
        {
            // Player isn't looking! Run towards them.
            agent.isStopped = false;
            agent.SetDestination(player.position);

            // Snap rotation
            Vector3 direction = (player.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            }
        }
        else
        {
            // Player IS looking! Freeze.
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // Kill check
        if (Vector3.Distance(transform.position, player.position) <= killDistance)
        {
            TriggerGameOver();
        }
    }

    void CheckVisibility()
    {
        Vector3 directionToDoll = (transform.position - playerCamera.transform.position).normalized;
        float dotProduct = Vector3.Dot(playerCamera.transform.forward, directionToDoll);

        if (dotProduct > 0.3f)
        {
            float distanceToDoll = Vector3.Distance(playerCamera.transform.position, transform.position);

            if (!Physics.Raycast(playerCamera.transform.position, directionToDoll, distanceToDoll, obstacleMask))
            {
                isVisible = true;
                return;
            }
        }

        isVisible = false;
    }

    public void ActivateStalker()
    {
        if (stalkStartPosition != null)
        {
            agent.Warp(stalkStartPosition.position);
        }
        isActive = true;
    }

    void TriggerGameOver()
    {
        // Start the cinematic sequence instead of instantly dying
        StartCoroutine(JumpscareSequence());
    }

    IEnumerator JumpscareSequence()
    {
        // 1. Stop the doll's AI completely
        isActive = false;
        agent.isStopped = true;
        agent.enabled = false; // Turn off NavMesh so it can teleport off the ground

        // 2. Freeze the player
        PlayerInteract interactScript = player.GetComponent<PlayerInteract>();
        if (interactScript != null)
        {
            interactScript.ToggleControls(false);
        }

        // 3. Teleport the doll directly to the camera anchor
        if (jumpscarePoint != null)
        {
            transform.position = jumpscarePoint.position;
            transform.rotation = jumpscarePoint.rotation;
        }

        // 4. Play the Scream Animation
        if (dollAnimator != null)
        {
            dollAnimator.SetTrigger("Scream");
        }

        // 5. WAIT for the animation to finish (Adjust screamDuration in Inspector)
        yield return new WaitForSeconds(screamDuration);

        // 6. Free the mouse so they can click restart
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 7. Show and fade in the Game Over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            CanvasGroup cg = gameOverUI.GetComponent<CanvasGroup>();

            if (cg != null)
            {
                StartCoroutine(FadeInDeathScreen(cg));
            }
        }
    }

    IEnumerator FadeInDeathScreen(CanvasGroup cg)
    {
        while (cg.alpha < 1f)
        {
            cg.alpha += Time.deltaTime * 1.5f;
            yield return null;
        }

        cg.interactable = true;
        cg.blocksRaycasts = true;
    }
}