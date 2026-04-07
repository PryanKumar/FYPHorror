using UnityEngine;
using TMPro;

public class TriggerInteractable : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI interactionUI;
    public string promptMessage = "Interact";
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

        // FIX: Hide UI IMMEDIATELY before firing events
        // This prevents the UI from getting stuck if the object is destroyed
        HideUI();

        if (onInteract != null)
        {
            onInteract.Invoke();
        }

        if (useOnlyOnce)
        {
            hasBeenUsed = true;

            // Disable physics so OnTriggerExit doesn't get confused
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
                interactionUI.text = "Press [E] to " + promptMessage;
                interactionUI.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HideUI();
        }
    }

    // This ensures that if the object is destroyed/disabled, the UI goes away
    private void OnDisable()
    {
        if (interactionUI != null) interactionUI.gameObject.SetActive(false);
    }

    private void HideUI()
    {
        playerIsNear = false;
        if (interactionUI != null)
        {
            interactionUI.gameObject.SetActive(false);
        }
    }
}