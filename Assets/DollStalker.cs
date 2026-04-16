using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Needed for fading UI

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
        // 1. Stop the doll
        isActive = false;
        agent.isStopped = true;

        // 2. Free the mouse so they can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. FREEZE THE PLAYER (Stops them from walking around while dead)
        PlayerInteract interactScript = player.GetComponent<PlayerInteract>();
        if (interactScript != null)
        {
            interactScript.ToggleControls(false);
        }

        // 4. SHOW AND FADE IN THE DEATH SCREEN
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

    // Smoothly makes the death screen visible and clickable
    IEnumerator FadeInDeathScreen(CanvasGroup cg)
    {
        while (cg.alpha < 1f)
        {
            cg.alpha += Time.deltaTime * 1.5f; // Adjust 1.5f to make fade faster/slower
            yield return null;
        }

        // Ensure the buttons can be clicked once fully visible
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }
}