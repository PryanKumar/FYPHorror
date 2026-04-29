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
    public Transform[] hallwayWaypoints;
    public Transform[] otWaypoints;
    public float walkSpeed = 2.0f;
    public int remainingOTPatrols = 0;

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

    // THE FIX: Grab components in Awake so she is ready before any external scripts give her orders!
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // THE FIX: Prioritize the OT instantly if she spawns with a quota
        if (remainingOTPatrols > 0 && otWaypoints.Length > 0)
        {
            agent.speed = walkSpeed;
            agent.SetDestination(otWaypoints[Random.Range(0, otWaypoints.Length)].position);
            remainingOTPatrols--;
        }
        else if (hallwayWaypoints.Length > 0)
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

        if (isChasing && distanceToPlayer > sightDistance)
        {
            isChasing = false;
            remainingOTPatrols = 0;

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
            if (remainingOTPatrols > 0 && otWaypoints.Length > 0)
            {
                agent.SetDestination(otWaypoints[Random.Range(0, otWaypoints.Length)].position);
                remainingOTPatrols--;
            }
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

    // THE FIX: Immediately override her path when the Breach script tells her to search the OT!
    public void StartOTSearch(int numberOfSweeps)
    {
        remainingOTPatrols = numberOfSweeps;

        if (agent != null && agent.isActiveAndEnabled && otWaypoints.Length > 0)
        {
            agent.ResetPath(); // Stop heading outside immediately!
            agent.speed = walkSpeed;
            agent.SetDestination(otWaypoints[Random.Range(0, otWaypoints.Length)].position);
            remainingOTPatrols--; // We just used one of the sweeps
        }
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