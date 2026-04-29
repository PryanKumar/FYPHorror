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
    public string correctCode = "0342";

    [Header("Door Control")]
    // THE FIX: We use the Animator now instead of trying to force the math!
    public Animator doorAnimator;

    private string currentInput = "";
    private bool isUnlocked = false;

    // --- PAUSE HANDLING HELPERS ---
    public void HideKeypadForPause() { keypadUI.SetActive(false); }
    public void ShowKeypadAfterResume() { if (!isUnlocked) keypadUI.SetActive(true); }

    // --- CORE LOGIC ---
    public void OpenKeypad()
    {
        if (isUnlocked) return;

        keypadUI.SetActive(true);
        currentInput = "";
        UpdateScreen("Enter Code", Color.red);

        // FREEZE TIME
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

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

        // 1. UNFREEZE TIME FIRST
        CloseKeypad();

        // 2. THE FIX: TELL THE ANIMATOR TO SWING IT OPEN
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("isOpen", true);
        }

        // 3. DISABLING THE INTERACTION UI
        TriggerInteractable trigger = GetComponent<TriggerInteractable>();
        if (trigger == null) trigger = GetComponentInParent<TriggerInteractable>();

        if (trigger != null)
        {
            if (trigger.floatingCanvas != null) trigger.floatingCanvas.SetActive(false);
            if (trigger.interactionUI != null) trigger.interactionUI.gameObject.SetActive(false);
            trigger.enabled = false;
        }

        // 4. CLEANUP
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
        Time.timeScale = 1f; // UNFREEZE TIME
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}