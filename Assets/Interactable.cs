using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("UI Message")]
    public string promptMessage = "Interact"; // Example: "Go to G floor" or "Pick up Fuse"

    // This is where you can link the button's actual function
    public UnityEngine.Events.UnityEvent onInteract;

    public void Interact()
    {
        onInteract.Invoke();
    }
}
