using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [Header("Settings")]
    public float closeDistance = 5f;

    private Animator anim;
    private Transform player;
    private bool isUnlocked = false;

    void Start()
    {
        // Get the Animator component from this object (the Parent)
        anim = GetComponent<Animator>();

        // Find the player automatically
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // Called by the KeypadController SuccessSequence
    public void UnlockDoor()
    {
        isUnlocked = true;
    }

    void Update()
    {
        // If the code hasn't been entered, don't do anything
        if (!isUnlocked || player == null) return;

        // Calculate distance between this door and the player
        float dist = Vector3.Distance(transform.position, player.position);

        // Logic: If close, open. If far, close.
        if (dist < closeDistance)
        {
            // Sets the Animator parameter 'isOpen' to true
            anim.SetBool("isOpen", true);
        }
        else
        {
            // Sets the Animator parameter 'isOpen' to false
            anim.SetBool("isOpen", false);
        }
    }
}