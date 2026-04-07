using UnityEngine;
using TMPro;

public class ItemInspector : MonoBehaviour
{
    [Header("References")]
    public Camera inspectionCamera;
    public GameObject inspectionUI;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Transform itemAnchor;

    [Header("Rotation Settings")]
    public float rotationSpeed = 300f;

    private GameObject currentInspectedObject;
    private GameObject originalWorldObject;
    private bool isInspecting = false;
    private PlayerInteract playerInteract;

    void Start()
    {
        playerInteract = GetComponent<PlayerInteract>();

        if (inspectionCamera != null) inspectionCamera.gameObject.SetActive(false);
        if (inspectionUI != null) inspectionUI.SetActive(false);
    }

    public void StartInspection(GameObject originalItem)
    {
        if (isInspecting || originalItem == null) return;

        InspectableItem data = originalItem.GetComponent<InspectableItem>();
        if (data == null) return;

        // 1. CLEAN THE ANCHOR IMMEDIATELY
        itemAnchor.rotation = Quaternion.identity;
        foreach (Transform child in itemAnchor)
        {
            Destroy(child.gameObject);
        }

        originalWorldObject = originalItem;
        isInspecting = true;

        inspectionCamera.gameObject.SetActive(true);
        inspectionUI.SetActive(true);

        if (playerInteract.menuManager != null)
            playerInteract.menuManager.gameObject.SetActive(false);

        nameText.text = data.itemName;
        descriptionText.text = data.itemDescription;

        // 2. SPAWN THE CLONE
        GameObject objectToClone = (data.inspectionPrefab != null) ? data.inspectionPrefab : originalItem;

        currentInspectedObject = Instantiate(objectToClone, Vector3.zero, Quaternion.identity);
        currentInspectedObject.transform.SetParent(itemAnchor, false);
        currentInspectedObject.transform.localPosition = Vector3.zero;
        currentInspectedObject.transform.localRotation = Quaternion.Euler(data.inspectionRotationOffset);

        SetLayerRecursively(currentInspectedObject, LayerMask.NameToLayer("Inspection"));

        // 3. NUCLEAR PHYSICS CLEANUP
        Rigidbody[] rbs = currentInspectedObject.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            Destroy(rb);
        }

        Collider[] cols = currentInspectedObject.GetComponentsInChildren<Collider>();
        foreach (Collider col in cols) col.enabled = false;

        MonoBehaviour[] scripts = currentInspectedObject.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script == null) continue;
            string sName = script.GetType().Name;
            if (sName == "TriggerInteractable" || sName == "InspectableItem" || sName == "PlayerInteract" || sName == "ClockHandItem")
                Destroy(script);
        }

        playerInteract.ToggleControls(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!isInspecting || currentInspectedObject == null) return;

        // 4. IMPROVED ROTATION
        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.unscaledDeltaTime;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.unscaledDeltaTime;

            itemAnchor.Rotate(Vector3.up, -rotX, Space.Self);
            itemAnchor.Rotate(Vector3.right, rotY, Space.Self);
        }

        if (Input.GetKeyDown(KeyCode.F)) TakeItem();
        if (Input.GetKeyDown(KeyCode.Escape)) StopInspection();
    }

    public void TakeItem()
    {
        // Trim and convert to lowercase for better matching
        string itemName = nameText.text.Trim();

        if (itemName.Contains("Fuse"))
            playerInteract.PickUpFuse();
        else if (itemName.Contains("Key"))
            playerInteract.PickUpKey();
        else if (itemName.Contains("Flashlight") || itemName.Contains("Torch"))
            playerInteract.PickedUpFlashlight();
        // --- ADDED CLOCK HAND CHECK ---
        else if (itemName.Contains("Clock Hand") || itemName.Contains("ClockHand"))
            playerInteract.PickUpClockHand();

        if (originalWorldObject != null)
            Destroy(originalWorldObject);

        StopInspection();
    }

    public void StopInspection()
    {
        if (!isInspecting) return;
        isInspecting = false;

        inspectionCamera.gameObject.SetActive(false);
        inspectionUI.SetActive(false);

        if (itemAnchor != null)
        {
            itemAnchor.rotation = Quaternion.identity;
            foreach (Transform child in itemAnchor) Destroy(child.gameObject);
        }

        if (playerInteract.menuManager != null)
            playerInteract.menuManager.gameObject.SetActive(true);

        if (currentInspectedObject != null)
            Destroy(currentInspectedObject);

        playerInteract.EndInspection();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}