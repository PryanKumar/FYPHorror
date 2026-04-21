using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SurgeryProgressManager : MonoBehaviour
{
    [Header("Surgery Settings")]
    public float progressSpeed = 15f;
    public float cooldownSpeed = 25f;
    public KeyCode surgeryKey = KeyCode.H;

    [Header("UI References")]
    public GameObject progressBarUI;
    public Slider progressSlider;

    [Header("Hand Tools (Attached to Camera)")]
    // Slot 0 = Tool 1, Slot 1 = Tool 2, Slot 2 = Tool 3
    public GameObject[] handTools;

    [Header("Phase Events")]
    public UnityEvent onPhase1Complete;
    public UnityEvent onPhase2Complete;
    public UnityEvent onPhase3Complete;

    private bool canPerformSurgery = false;
    private float currentProgress = 0f;

    // 0 = Cut, 1 = Open, 2 = Extract
    public int currentPhase = 0;

    void Start()
    {
        if (progressBarUI != null) progressBarUI.SetActive(false);
        if (progressSlider != null) progressSlider.value = 0f;

        // Ensure all hand tools are hidden at the start
        foreach (GameObject tool in handTools)
        {
            if (tool != null) tool.SetActive(false);
        }
    }

    void Update()
    {
        if (!canPerformSurgery) return;

        if (Input.GetKey(surgeryKey))
        {
            // 1. Fill the bar
            currentProgress += progressSpeed * Time.deltaTime;

            // 2. Show the correct tool in hand
            if (currentPhase < handTools.Length && handTools[currentPhase] != null)
            {
                handTools[currentPhase].SetActive(true);
            }

            // 3. Check if finished
            if (currentProgress >= 100f)
            {
                currentProgress = 100f;
                CompletePhase();
            }
        }
        else
        {
            // 1. Drain the bar
            if (currentProgress > 0f)
            {
                currentProgress -= cooldownSpeed * Time.deltaTime;
            }

            // 2. Hide the tool if they let go of 'H'
            if (currentPhase < handTools.Length && handTools[currentPhase] != null)
            {
                handTools[currentPhase].SetActive(false);
            }
        }

        // Update the UI
        if (progressSlider != null) progressSlider.value = currentProgress;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPerformSurgery = true;
            if (progressBarUI != null) progressBarUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPerformSurgery = false;
            if (progressBarUI != null) progressBarUI.SetActive(false);

            // Failsafe: Hide tool if player walks away while holding H
            if (currentPhase < handTools.Length && handTools[currentPhase] != null)
            {
                handTools[currentPhase].SetActive(false);
            }
        }
    }

    void CompletePhase()
    {
        canPerformSurgery = false;
        currentProgress = 0f;
        if (progressBarUI != null) progressBarUI.SetActive(false);

        // Hide the tool for good
        if (currentPhase < handTools.Length && handTools[currentPhase] != null)
        {
            handTools[currentPhase].SetActive(false);
        }

        // Trigger the scary events!
        if (currentPhase == 0)
        {
            Debug.Log("PHASE 1 COMPLETE: Ghost is coming!");
            onPhase1Complete.Invoke();
        }
        else if (currentPhase == 1)
        {
            Debug.Log("PHASE 2 COMPLETE: Ceiling drop!");
            onPhase2Complete.Invoke();
        }
        else if (currentPhase == 2)
        {
            Debug.Log("PHASE 3 COMPLETE: Final Cutscene!");
            onPhase3Complete.Invoke();
        }

        // Advance to the next phase internally
        currentPhase++;

        // TURN OFF THE SURGERY ZONE. Player must pick up the next tool to turn it back on!
        gameObject.SetActive(false);
    }
}