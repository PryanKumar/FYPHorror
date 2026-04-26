using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Phase1BreachManager : MonoBehaviour
{
    [Header("Lighting")]
    public Light otSpotlight;
    public Color breachRedColor = new Color(0.8f, 0f, 0f);
    public float breatheSpeed = 2f;
    public float maxIntensity = 5f;
    public float minIntensity = 1f;

    [Header("Door & Audio")]
    public AudioSource doorAudioSource;
    public AudioClip bangingSound;
    public AudioClip doorSmashSound;
    public Animator otDoorAnimator;

    [Header("The Entity & Scripting")]
    public GameObject nurseGhost;
    public NurseAI nurseBrain;
    public Transform otInvestigationPoint;

    [Header("UI & Timing")]
    public GameObject hideWarningUI;       // Drag your "HIDE" text here
    public float breachTimer = 5.0f;       // Time spent banging
    public float enterDelay = 5.0f;        // Time she waits at the open door before walking in

    private bool isBlinking = false;
    private bool isBreathing = false;
    private float originalIntensity;

    void Start()
    {
        if (otSpotlight != null) originalIntensity = otSpotlight.intensity;
    }

    public void StartBreachSequence()
    {
        StartCoroutine(BreachCountdown());
        StartCoroutine(BlinkLight());
    }

    IEnumerator BreachCountdown()
    {
        // 1. INSTANTLY SPAWN GHOST AND FREEZE HER
        if (nurseGhost != null)
        {
            nurseGhost.SetActive(true);

            if (nurseBrain != null)
            {
                nurseBrain.isScriptedEvent = true;
                NavMeshAgent agent = nurseBrain.GetComponent<NavMeshAgent>();
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
            }

            Animator ghostAnim = nurseGhost.GetComponent<Animator>();
            if (ghostAnim != null) ghostAnim.SetTrigger("BangDoor");
        }

        // START AUDIO BANGING
        if (doorAudioSource != null && bangingSound != null)
        {
            doorAudioSource.clip = bangingSound;
            doorAudioSource.loop = true;
            doorAudioSource.Play();
        }

        // 2. WAIT 5 SECONDS (Banging phase)
        yield return new WaitForSeconds(breachTimer);

        // 3. THE DOOR FLIES OPEN
        isBlinking = false;

        if (doorAudioSource != null)
        {
            doorAudioSource.Stop();
            if (doorSmashSound != null) doorAudioSource.PlayOneShot(doorSmashSound);
        }

        if (otDoorAnimator != null) otDoorAnimator.SetBool("isOpen", true);

        if (otSpotlight != null)
        {
            otSpotlight.color = breachRedColor;
            isBreathing = true;
        }

        // 4. SHOW THE "HIDE" WARNING AND WAIT
        if (hideWarningUI != null) hideWarningUI.SetActive(true);

        // Let her stand there menacingly while the player scrambles
        yield return new WaitForSeconds(enterDelay);

        // 5. TURN OFF UI AND RELEASE HER INTO THE ROOM
        if (hideWarningUI != null) hideWarningUI.SetActive(false);

        if (nurseBrain != null)
        {
            NavMeshAgent agent = nurseBrain.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;

                if (otInvestigationPoint != null)
                {
                    agent.SetDestination(otInvestigationPoint.position);
                }
            }

            // --- THE MAGIC LINE ---
            // Tell her brain to check exactly 3 interior waypoints before leaving!
            nurseBrain.StartOTSearch(3);

            // Turn her brain back on!
            nurseBrain.isScriptedEvent = false;
        }
    }

    IEnumerator BlinkLight()
    {
        isBlinking = true;
        while (isBlinking && otSpotlight != null)
        {
            otSpotlight.intensity = Random.Range(0f, originalIntensity);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
    }

    void Update()
    {
        if (isBreathing && otSpotlight != null)
        {
            float pingPong = Mathf.PingPong(Time.time * breatheSpeed, maxIntensity - minIntensity);
            otSpotlight.intensity = minIntensity + pingPong;
        }
    }
}