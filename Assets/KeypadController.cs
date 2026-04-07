using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public partial class KeypadController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject keypadUI;
    public TMP_Text screenText;
    public Image screenBackground;

    [Header("Settings")]
    public string correctCode = "0342"; //
    public GameObject doorToOpen; // Drag 'StoreDoor_Parent' here

    private string currentInput = "";
    private bool isUnlocked = false;

    // --- PAUSE HANDLING HELPERS ---

    public void HideKeypadForPause()
    {
        keypadUI.SetActive(false);
    }

    public void ShowKeypadAfterResume()
    {
        if (!isUnlocked)
        {
            keypadUI.SetActive(true);
        }
    }

    // --- CORE LOGIC ---

    public void OpenKeypad()
    {
        if (isUnlocked) return;

        keypadUI.SetActive(true);
        currentInput = "";
        UpdateScreen("Enter Code", Color.red);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Linked to your number buttons 0-9
    public void NumberButton(string number)
    {
        if (currentInput.Length < 4)
        {
            currentInput += number;
            screenText.text = currentInput;
        }
    }

    public void DeleteButton()
    {
        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            screenText.text = currentInput;
        }
    }

    public void EnterButton()
    {
        if (currentInput == correctCode)
        {
            StartCoroutine(SuccessSequence());
        }
        else
        {
            UpdateScreen("Incorrect Code", Color.red);
            currentInput = "";
        }
    }

    IEnumerator SuccessSequence()
    {
        isUnlocked = true;
        UpdateScreen("Unlocked", Color.green);

        // Wait while time is frozen so player can see "Unlocked"
        yield return new WaitForSecondsRealtime(1f);

        // 1. TRIGGER DOOR ANIMATION & SCRIPT
        if (doorToOpen != null)
        {
            // Trigger the Animator parameter 'isOpen'
            Animator anim = doorToOpen.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("isOpen", true);
            }

            // Also call the Unlock function if the SlidingDoor script is used
            SlidingDoor doorScript = doorToOpen.GetComponent<SlidingDoor>();
            if (doorScript != null)
            {
                doorScript.UnlockDoor();
            }
        }

        // 2. DISABLING THE INTERACTION UI
        TriggerInteractable trigger = GetComponent<TriggerInteractable>();
        if (trigger == null) trigger = GetComponentInParent<TriggerInteractable>();

        if (trigger != null)
        {
            if (trigger.interactionUI != null)
            {
                trigger.interactionUI.gameObject.SetActive(false);
            }
            trigger.enabled = false;
        }

        CloseKeypad();

        // 3. CLEANUP
        gameObject.tag = "Untagged";
        this.enabled = false;
    }

    private void UpdateScreen(string msg, Color col)
    {
        screenText.text = msg;
        screenBackground.color = col;
    }

    public void CloseKeypad()
    {
        keypadUI.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}