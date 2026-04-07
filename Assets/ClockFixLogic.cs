using UnityEngine;

public class ClockFixLogic : MonoBehaviour
{
    [Header("References")]
    public GameObject arrowsParent;      // Drag the 'Arrow' parent here
    public SteamReveal steamScript;      // Drag the object with SteamReveal script here
    public AudioSource fixSound;         // Optional: A 'click' sound when fixing

    public void Interacted(PlayerInteract player)
    {
        // 1. Check if the player actually found the hand first
        if (player.hasClockHand)
        {
            FixClock();
        }
        else
        {
            // Optional: Show a UI message like "I need the clock hands..."
            Debug.Log("Missing clock hands!");
        }
    }

    void FixClock()
    {
        // 2. Unhide the arrows
        if (arrowsParent != null) arrowsParent.SetActive(true);

        // 3. Play a sound effect if you have one
        if (fixSound != null) fixSound.Play();

        // 4. Trigger your existing Steam Reveal sequence
        if (steamScript != null)
        {
            steamScript.ActivateReveal();
        }
    }
}