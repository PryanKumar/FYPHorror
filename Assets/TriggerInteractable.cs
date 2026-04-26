using UnityEngine;
using TMPro;

public class TriggerInteractable : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject floatingCanvas;
    public TextMeshProUGUI interactionUI;
    public string promptMessage = "E";
    public string missingKeyMessage = "Key Required";

    [Header("Requirements")]
    public bool needsKey = false;

    [Header("Event Settings")]
    public UnityEngine.Events.UnityEvent onInteract;

    [Header("Usage Settings")]
    public bool useOnlyOnce = false;
    private bool hasBeenUsed = false;
    private bool playerIsNear = false;
    private PlayerInteract playerScript;

    // NEW: Forces the UI to be invisible the exact second the game starts!
    void Start()
    {
        HideUI();
    }

    void Update()
    {
        if (playerIsNear && Input.GetKeyDown(KeyCode.E))
        {
            if (!needsKey || (playerScript != null && playerScript.hasKey))
            {
                Interact();
            }
            else
            {
                if (interactionUI != null) interactionUI.text = missingKeyMessage;
            }
        }
    }

    public void Interact()
    {
        if (useOnlyOnce && hasBeenUsed) return;

        // Hide UI IMMEDIATELY before firing events
        HideUI();

        if (onInteract != null)
        {
            onInteract.Invoke();
        }

        if (useOnlyOnce)
        {
            hasBeenUsed = true;

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            this.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenUsed) return;

        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
            playerScript = other.GetComponent<PlayerInteract>();

            if (interactionUI != null)
            {
                interactionUI.text = promptMessage;

                // Fallback for old items that don't have a floating canvas yet
                if (floatingCanvas == null) interactionUI.gameObject.SetActive(true);
            }

            // Turn on the entire Canvas group when you get close
            if (floatingCanvas != null) floatingCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Turns it off when you walk away
            HideUI();
        }
    }

    private void OnDisable()
    {
        if (floatingCanvas != null)
        {
            floatingCanvas.SetActive(false);
        }
        else if (interactionUI != null)
        {
            interactionUI.gameObject.SetActive(false);
        }
    }

    private void HideUI()
    {
        playerIsNear = false;

        // Safely turns off the whole visual group
        if (floatingCanvas != null)
        {
            floatingCanvas.SetActive(false);
        }
        else if (interactionUI != null)
        {
            interactionUI.gameObject.SetActive(false);
        }
    }
}