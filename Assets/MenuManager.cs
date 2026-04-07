using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public enum ObjectiveStage
    {
        EnterLift,
        CollectNote,
        FindTheatre,
        Loop1,
        OpenTheatre,
        Loop2_LiftNote,
        Loop2_Flashlight,
        Loop2_Generator
    }

    [Header("Current Progress")]
    public ObjectiveStage currentStage = ObjectiveStage.EnterLift;

    [Header("Setup")]
    public GameObject menuPanel;
    public PlayerInteract playerInteract;
    public KeypadController activeKeypad;

    [Header("UI Groups")]
    public GameObject loop1Container;
    public TextMeshProUGUI primaryObjectiveText;

    [Header("Loop 1 Objective Texts")]
    public TextMeshProUGUI keyText;
    public TextMeshProUGUI fuseText;
    public TextMeshProUGUI powerText;

    [Header("Note Inventory System")]
    public GameObject mainButtonsGroup;
    public GameObject notesInventoryPanel;
    public GameObject noteButtonPrefab;
    public Transform buttonContainer;

    // NEW: We need to store the sprites here too so the inventory can display them
    public List<Sprite> collectedNoteSprites = new List<Sprite>();

    private bool isPaused = false;
    private float lastToggleTime;
    private bool wasKeypadOpenBeforePause = false;

    void Awake()
    {
        if (playerInteract != null)
        {
            playerInteract.hasFuse = false;
            playerInteract.hasKey = false;
            playerInteract.isPowerOn = false;
        }
    }

    void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (notesInventoryPanel != null) notesInventoryPanel.SetActive(false);
        UpdateObjectiveDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Time.unscaledTime - lastToggleTime > 0.2f)
        {
            lastToggleTime = Time.unscaledTime;
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        if (activeKeypad != null && activeKeypad.keypadUI.activeSelf)
        {
            wasKeypadOpenBeforePause = true;
            activeKeypad.keypadUI.SetActive(false);
        }

        if (menuPanel != null) menuPanel.SetActive(true);
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(true);
        if (notesInventoryPanel != null) notesInventoryPanel.SetActive(false);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateAllObjectives();
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        if (menuPanel != null) menuPanel.SetActive(false);
        if (notesInventoryPanel != null) notesInventoryPanel.SetActive(false);

        if (wasKeypadOpenBeforePause && activeKeypad != null)
        {
            activeKeypad.keypadUI.SetActive(true);
            wasKeypadOpenBeforePause = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (playerInteract != null) playerInteract.ForceCloseEverything();
        }
    }

    // --- Note Inventory Logic ---
    public void OpenNotesInventory()
    {
        if (notesInventoryPanel == null || playerInteract == null) return;
        if (mainButtonsGroup != null) mainButtonsGroup.SetActive(false);
        notesInventoryPanel.SetActive(true);

        foreach (Transform child in buttonContainer) Destroy(child.gameObject);

        for (int i = 0; i < playerInteract.collectedNoteTitles.Count; i++)
        {
            GameObject newButton = Instantiate(noteButtonPrefab, buttonContainer);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = playerInteract.collectedNoteTitles[i];
            int index = i;
            newButton.GetComponent<Button>().onClick.AddListener(() => ShowNoteFromInventory(index));
        }
    }

    // FIXED: Now passes both the Content and the Sprite to PlayerInteract
    void ShowNoteFromInventory(int index)
    {
        if (notesInventoryPanel != null) notesInventoryPanel.SetActive(false);

        // We get the content from playerInteract and the Sprite from our local list
        string content = playerInteract.collectedNoteContents[index];
        Sprite noteSprite = (index < collectedNoteSprites.Count) ? collectedNoteSprites[index] : null;

        playerInteract.ShowNote(content, noteSprite);
    }

    public void BackToInventoryFromNote()
    {
        if (playerInteract != null) playerInteract.CloseNote();
        OpenNotesInventory();
    }

    // --- Objective Logic ---
    public void UpdateFuseUI() { UpdateAllObjectives(); }
    public void UpdateKeyUI() { UpdateAllObjectives(); }
    public void UpdatePowerUI() { UpdateAllObjectives(); }

    public void UpdateObjectiveStage(int stageIndex)
    {
        currentStage = (ObjectiveStage)stageIndex;
        UpdateObjectiveDisplay();
    }

    private void UpdateObjectiveDisplay()
    {
        if (primaryObjectiveText == null || loop1Container == null) return;

        primaryObjectiveText.gameObject.SetActive(false);
        loop1Container.SetActive(false);

        switch (currentStage)
        {
            case ObjectiveStage.EnterLift:
                primaryObjectiveText.gameObject.SetActive(true);
                primaryObjectiveText.text = "Enter the lift";
                break;
            case ObjectiveStage.CollectNote:
                primaryObjectiveText.gameObject.SetActive(true);
                primaryObjectiveText.text = "Collect the hospital note";
                break;
            case ObjectiveStage.FindTheatre:
                primaryObjectiveText.gameObject.SetActive(true);
                primaryObjectiveText.text = "Find the Operation Theatre";
                break;
            case ObjectiveStage.Loop1:
                loop1Container.SetActive(true);
                break;
            case ObjectiveStage.OpenTheatre:
                primaryObjectiveText.gameObject.SetActive(true);
                primaryObjectiveText.text = "Open Operation Theatre";
                break;
            case ObjectiveStage.Loop2_LiftNote:
                primaryObjectiveText.gameObject.SetActive(true);
                primaryObjectiveText.text = "Pick up Lift Note";
                break;
            case ObjectiveStage.Loop2_Flashlight:
                primaryObjectiveText.gameObject.SetActive(true);
                primaryObjectiveText.text = "Find the Flashlight";
                break;
            case ObjectiveStage.Loop2_Generator:
                primaryObjectiveText.gameObject.SetActive(true);
                primaryObjectiveText.text = "Turn on Generator";
                break;
        }
    }

    void UpdateAllObjectives()
    {
        if (playerInteract == null) return;

        if (currentStage == ObjectiveStage.Loop1 && playerInteract.hasKey)
        {
            UpdateObjectiveStage((int)ObjectiveStage.OpenTheatre);
            return;
        }

        UpdateObjectiveDisplay();

        if (currentStage == ObjectiveStage.Loop1)
        {
            if (keyText != null) keyText.color = playerInteract.hasKey ? Color.green : Color.white;
            if (fuseText != null) fuseText.color = (playerInteract.hasFuse || playerInteract.isPowerOn) ? Color.green : Color.white;
            if (powerText != null) powerText.color = playerInteract.isPowerOn ? Color.green : Color.white;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit requested");
    }
}