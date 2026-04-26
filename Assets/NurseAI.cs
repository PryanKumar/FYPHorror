using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NurseAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject gameOverUI;
    private NavMeshAgent agent;
    private Animator anim;

    [Header("Patrol Settings")]
    public Transform[] hallwayWaypoints;   // NEW: Only points outside
    public Transform[] otWaypoints;        // NEW: Only points inside the OT
    public float walkSpeed = 2.0f;
    public int remainingOTPatrols = 0;     // NEW: How many spots she must check before leaving

    [Header("Hunting Settings")]
    public float runSpeed = 5.0f;
    public float sightDistance = 15.0f;
    public float fieldOfView = 90f;
    public float killDistance = 1.5f;

    [Header("Jumpscare & Cinematic")]
    public GameObject cameraGhost;
    public Animator cameraGhostAnim;
    public CanvasGroup blackFade;
    public float fadeSpeed = 1.5f;

    [Header("States")]
    public bool isChasing = false;
    public bool isScriptedEvent = false;
    private bool isKilling = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Default start: patrol the hallway
        if (hallwayWaypoints.Length > 0)
        {
            agent.speed = walkSpeed;
            agent.SetDestination(hallwayWaypoints[Random.Range(0, hallwayWaypoints.Length)].position);
        }
    }

    void Update()
    {
        if (isKilling || isScriptedEvent) return;

        CheckForPlayer();

        if (isKilling || isScriptedEvent) return;

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        UpdateAnimations();
    }

    void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= killDistance)
        {
            StartCoroutine(KillPlayer());
            return;
        }

        if (distanceToPlayer <= sightDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < fieldOfView / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, sightDistance))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        isChasing = true;
                        return;
                    }
                }
            }
        }

        // If she loses sight of you
        if (isChasing && distanceToPlayer > sightDistance)
        {
            isChasing = false;
            remainingOTPatrols = 0; // Cancel the OT search if she was chasing

            // Go back to the hallway
            if (hallwayWaypoints.Length > 0)
            {
                agent.SetDestination(hallwayWaypoints[Random.Range(0, hallwayWaypoints.Length)].position);
            }
        }
    }

    void Patrol()
    {
        agent.speed = walkSpeed;

        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            // If she still has an OT quota, force her to pick an OT point!
            if (remainingOTPatrols > 0 && otWaypoints.Length > 0)
            {
                agent.SetDestination(otWaypoints[Random.Range(0, otWaypoints.Length)].position);
                remainingOTPatrols--; // Subtract 1 from the quota
            }
            // Otherwise, she is free to roam the hallways
            else if (hallwayWaypoints.Length > 0)
            {
                agent.SetDestination(hallwayWaypoints[Random.Range(0, hallwayWaypoints.Length)].position);
            }
        }
    }

    void ChasePlayer()
    {
        agent.speed = runSpeed;
        agent.SetDestination(player.position);
    }

    // NEW PUBLIC METHOD: The Breach Manager will trigger this!
    public void StartOTSearch(int numberOfSweeps)
    {
        remainingOTPatrols = numberOfSweeps;
    }

    void UpdateAnimations()
    {
        if (isChasing)
        {
            anim.SetBool("isRunning", true);
            anim.SetBool("isWalking", false);
        }
        else if (agent.velocity.magnitude > 0.1f || agent.pathPending)
        {
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", false);
        }
    }

    IEnumerator KillPlayer()
    {
        isKilling = true;
        isChasing = false;

        PlayerInteract interactScript = player.GetComponent<PlayerInteract>();
        if (interactScript != null) interactScript.ToggleControls(false);

        agent.isStopped = true;
        agent.ResetPath();

        transform.position = new Vector3(transform.position.x, -100f, transform.position.z);

        if (cameraGhost != null) cameraGhost.SetActive(true);
        if (cameraGhostAnim != null) cameraGhostAnim.SetTrigger("Kill");

        yield return new WaitForSeconds(1.5f);

        if (blackFade != null)
        {
            float timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime * fadeSpeed;
                blackFade.alpha = Mathf.Lerp(0, 1, timer);
                yield return null;
            }
            blackFade.alpha = 1f;
        }

        yield return new WaitForSeconds(0.5f);

        if (cameraGhost != null) cameraGhost.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            CanvasGroup uiGroup = gameOverUI.GetComponent<CanvasGroup>();
            if (uiGroup != null) uiGroup.alpha = 1f;
        }
    }
}