using UnityEngine;

public class NoteTrigger : MonoBehaviour
{
    public string noteTitle; // Example: "Doctor's Log"
    [TextArea]
    public string noteContent; // The text that appears on the note

    [Header("Visuals")]
    public Sprite noteSprite; // NEW: Drag the paper texture/sprite here

    public PlayerInteract playerInteract;

    public void SendNoteToPlayer()
    {
        if (playerInteract != null)
        {
            // Now sending all 4 arguments: Title, Content, Sprite, and this GameObject
            playerInteract.ShowNote(noteTitle, noteContent, noteSprite, this.gameObject);
        }
    }
}