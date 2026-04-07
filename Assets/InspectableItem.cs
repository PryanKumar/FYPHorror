using UnityEngine;

public class InspectableItem : MonoBehaviour
{
    public string itemName;
    [TextArea]
    public string itemDescription;

    [Header("Visuals (3D Items)")]
    // Use this for 3D items you want to rotate (like the Fuse or Key)
    public GameObject inspectionPrefab;
    public Vector3 inspectionRotationOffset;

    [Header("Note System (2D Items)")]
    // NEW: Drag your specific note texture (Lift note, OT note, etc.) here
    public Sprite noteUIBackground;
}