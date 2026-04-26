using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    public Animator doorAnimator;
    private bool isDoorOpen = false;

    public void ToggleDoorState()
    {
        // This flips the boolean: if false it becomes true, if true it becomes false
        isDoorOpen = !isDoorOpen;

        if (doorAnimator != null)
        {
            doorAnimator.SetBool("isOpen", isDoorOpen);
        }
    }
}