using UnityEngine;

public class ClockPlaceholder : MonoBehaviour
{
    [Header("References")]
    public SteamReveal mirrorScript;
    public GameObject missingHandVisual; // Drag Clock (1) here
    public AudioSource repairSound;

    [Header("Settings")]
    public string missingMessage = "The clock is missing a hand...";

    private bool isFixed = false;

    public void TriggerFix()
    {
        if (isFixed) return;

        PlayerInteract player = Object.FindFirstObjectByType<PlayerInteract>();

        if (player != null)
        {
            if (player.hasClockHand)
            {
                FixClock();
            }
            else
            {
                Debug.Log(missingMessage);
            }
        }
    }

    void FixClock()
    {
        isFixed = true;

        if (missingHandVisual != null) missingHandVisual.SetActive(true);
        if (repairSound != null) repairSound.Play();

        if (mirrorScript != null)
        {
            mirrorScript.ActivateReveal();
        }

        // --- SYNTAX FIX APPLIED HERE ---
        TriggerInteractable trigger = GetComponent<TriggerInteractable>();
        if (trigger != null)
        {
            // We use .gameObject because SetActive belongs to the object, not the text component
            if (trigger.interactionUI != null) trigger.interactionUI.gameObject.SetActive(false);

            trigger.enabled = false;
        }

        // Disable collider so the player can't interact twice
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

        Debug.Log("Clock repaired! Steam system activated.");
    }

    public void Interacted(PlayerInteract player)
    {
        if (isFixed) return;
        if (player != null && player.hasClockHand) FixClock();
    }
}