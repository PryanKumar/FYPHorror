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
    public GameObject[] handTools;

    [Header("Phase Events")]
    public UnityEvent onPhase1Start;    // Triggers the second they touch 'H'
    public UnityEvent onPhase1Complete;
    public UnityEvent onPhase2Complete;
    public UnityEvent onPhase3Complete;

    [Header("Cinematic Connections")]
    public Phase2DropManager dropManager; // NEW: Connects to our ceiling drop script!

    private bool canPerformSurgery = false;
    private float currentProgress = 0f;
    private bool hasPhase1Started = false;
    private bool hasCeilingDropTriggered = false; // NEW: Prevents the scare from firing twice!

    public int currentPhase = 0;

    void Start()
    {
        if (progressBarUI != null) progressBarUI.SetActive(false);
        if (progressSlider != null) progressSlider.value = 0f;

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
            // Trigger the banging the very first time they hold H in Phase 0!
            if (currentPhase == 0 && !hasPhase1Started)
            {
                hasPhase1Started = true;
                onPhase1Start.Invoke();
            }

            currentProgress += progressSpeed * Time.deltaTime;

            // NEW: Phase 2 Ceiling Drop Logic (Happens at 30 Progress)
            // Note: currentPhase is 1 because Phase 1 was 0.
            if (currentPhase == 1 && currentProgress >= 30f && !hasCeilingDropTriggered)
            {
                hasCeilingDropTriggered = true;
                if (dropManager != null) dropManager.TriggerCeilingDrop();
            }

            if (currentPhase < handTools.Length && handTools[currentPhase] != null)
                handTools[currentPhase].SetActive(true);

            if (currentProgress >= 100f)
            {
                currentProgress = 100f;
                CompletePhase();
            }
        }
        else
        {
            if (currentProgress > 0f) currentProgress -= cooldownSpeed * Time.deltaTime;

            if (currentPhase < handTools.Length && handTools[currentPhase] != null)
                handTools[currentPhase].SetActive(false);
        }

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

            if (currentPhase < handTools.Length && handTools[currentPhase] != null)
                handTools[currentPhase].SetActive(false);
        }
    }

    void CompletePhase()
    {
        canPerformSurgery = false;
        currentProgress = 0f;
        if (progressBarUI != null) progressBarUI.SetActive(false);

        if (currentPhase < handTools.Length && handTools[currentPhase] != null)
            handTools[currentPhase].SetActive(false);

        if (currentPhase == 0) onPhase1Complete.Invoke();
        else if (currentPhase == 1) onPhase2Complete.Invoke();
        else if (currentPhase == 2) onPhase3Complete.Invoke();

        currentPhase++;
        gameObject.SetActive(false);
    }
}