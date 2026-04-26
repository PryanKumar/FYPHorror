using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Phase2DropManager : MonoBehaviour
{
    [Header("The Old Threat (Phase 1)")]
    public GameObject phase1Nurse;         // NEW: We must despawn her so there aren't two clones!

    [Header("The Dummy (Cinematic Only)")]
    public GameObject dummyGhost;
    public Animator dummyAnim;
    public AudioSource dropAudio;
    public float animationLength = 2.0f;

    [Header("The New Threat (Phase 2)")]
    public GameObject realNurseAI;
    public NurseAI nurseBrain;

    public void TriggerCeilingDrop()
    {
        StartCoroutine(DropSequence());
    }

    IEnumerator DropSequence()
    {
        // 1. DESPAWN PHASE 1 NURSE INSTANTLY
        // Cover our tracks so the player doesn't see two ghosts at once!
        if (phase1Nurse != null)
        {
            phase1Nurse.SetActive(false);
        }

        // 2. ACTIVATE THE CEILING DUMMY
        if (dummyGhost != null)
        {
            dummyGhost.SetActive(true);
            if (dummyAnim != null) dummyAnim.SetTrigger("Drop");
        }

        // Play loud crash sound
        if (dropAudio != null) dropAudio.Play();

        // 3. WAIT FOR HER TO FINISH STANDING UP
        yield return new WaitForSeconds(animationLength);

        // 4. THE MAGIC SWAP
        if (dummyGhost != null) dummyGhost.SetActive(false);

        if (realNurseAI != null)
        {
            // Move the real AI exactly to where the dummy landed
            realNurseAI.transform.position = dummyGhost.transform.position;
            realNurseAI.transform.rotation = dummyGhost.transform.rotation;

            // Turn her on!
            realNurseAI.SetActive(true);

            // Give her a massive quota so she aggressively sweeps the room
            if (nurseBrain != null)
            {
                nurseBrain.StartOTSearch(6); // Force her to check 6 spots!
            }
        }
    }
}