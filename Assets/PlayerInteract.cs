using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerInteract : MonoBehaviour
{
    private static bool tutorialsPlayed = false;

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public LayerMask interactLayer;

    [Header("Electric Shock Settings")]
    public AudioSource shockScreamSource;
    public float shockDuration = 1.5f;
    public float shockIntensity = 0.15f;

    [Header("Inventory Status")]
    public bool hasFuse = false;
    public bool hasKey = false;
    public bool hasClockHand = false;
    public bool isPowerOn = false;

    [Header("Doll Setup")]
    public DollStalker creepyDoll;

    [Header("Movement SFX")]
    public AudioSource footstepSource;
    public AudioClip walkClip;
    public AudioClip runClip;
    public float walkStepInterval = 0.6f;
    public float runStepInterval = 0.35f;
    private float stepTimer;

    [Header("Interaction SFX")]
    public AudioSource interactionAudioSource;
    public AudioClip notePickupSound;
    public AudioClip itemPickupSound;

    [Header("Note System")]
    public GameObject notePanel;
    public Image noteBackgroundUI;
    public TextMeshProUGUI noteUIText;
    private bool isReadingNote = false;

    [Header("Note Inventory")]
    public List<string> collectedNoteTitles = new List<string>();
    public List<string> collectedNoteContents = new List<string>();

    [Header("Item Inspection System")]
    public ItemInspector itemInspector;
    public bool isInspecting = false;

    [Header("Door Settings")]
    public Transform actualDoor;
    public float openXPosition = 572.351f;
    public float slideSpeed = 2f;

    [Header("Ghost Scare Settings (Hallway Walk)")]
    public GameObject hallwayGhost;
    public AudioSource ghostWalkAudio;
    public float ghostTravelTime = 3.0f;

    [Header("Ghost Scare Settings (Hallway Run)")]
    public GameObject runningGhost;
    public AudioSource ghostRunAudio;
    public float ghostRunDuration = 1.5f;

    [Header("Final Jumpscare Settings (End Game)")]
    public GameObject faceJumpscareGhost;
    public AudioSource jumpscareScream;
    public Image blackScreenOverlay;
    public string loop2SceneName = "Loop2";

    [Header("General References")]
    public MonoBehaviour movementScript;
    public MouseLook cameraLookScript;
    public GameObject yellowKeyObject;
    public Light powerLight;
    public MenuManager menuManager;

    [Header("Flashlight Toggle")]
    public GameObject playerFlashlight;
    private bool isFlashlightOn = false;
    public AudioSource flashlightClickSound;

    [Header("Tutorial System")]
    public GameObject tutorialPanel;
    public Image tutorialIconSlot;
    public TextMeshProUGUI tutorialDescription;
    public Sprite mouseMoveIcon;
    public Sprite wasdIcon;
    public Sprite tabMenuIcon;
    public Sprite rightClickIcon;

    private bool isBeingShocked = false;

    void Awake()
    {
        hasFuse = false;
        hasKey = false;
        hasClockHand = false;
        isPowerOn = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (notePanel != null) notePanel.SetActive(false);
        if (playerFlashlight != null) playerFlashlight.SetActive(false);
    }

    void Start()
    {
        if (!tutorialsPlayed)
        {
            Invoke("ShowLookTutorial", 2.0f);
            Invoke("ShowMovementTutorial", 9.0f);
            Invoke("ShowMenuTutorial", 16.0f);
            tutorialsPlayed = true;
        }
    }

    void Update()
    {
        if (menuManager != null && menuManager.menuPanel.activeInHierarchy) return;
        if (isReadingNote) { if (Input.GetKeyDown(KeyCode.Escape)) CloseNote(); return; }
        if (isInspecting || isBeingShocked) return;

        HandleFootsteps();

        if (playerFlashlight != null && playerFlashlight.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(1)) ToggleFlashlight();
        }

        if (Input.GetKeyDown(interactKey)) TryInteract();
    }

    void ShowLookTutorial() { ShowTutorial(mouseMoveIcon, "Move Mouse to Look Around", 6f); }
    void ShowMovementTutorial() { ShowTutorial(wasdIcon, "Use WASD to Move", 6f); }
    void ShowMenuTutorial() { ShowTutorial(tabMenuIcon, "Press TAB to open Menu", 8f); }

    public void ShowTutorial(Sprite icon, string text, float duration)
    {
        StartCoroutine(TutorialRoutine(icon, text, duration));
    }

    private IEnumerator TutorialRoutine(Sprite icon, string text, float duration)
    {
        if (tutorialPanel == null) yield break;
        tutorialIconSlot.sprite = icon;
        tutorialDescription.text = text;
        tutorialPanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        tutorialPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ScareTrigger") && hasFuse && !isPowerOn)
        {
            StartCoroutine(HallwayWalkSequence());
            other.enabled = false;
        }

        if (other.CompareTag("RunScareTrigger"))
        {
            StartCoroutine(HallwayRunSequence());
            other.enabled = false;
        }

        if (other.CompareTag("FinalTrigger"))
        {
            StartCoroutine(FinalEndingSequence());
            other.enabled = false;
        }
    }

    IEnumerator HallwayWalkSequence()
    {
        if (ghostWalkAudio != null) ghostWalkAudio.Play();
        if (hallwayGhost != null) hallwayGhost.SetActive(true);
        yield return new WaitForSeconds(ghostTravelTime);
        if (ghostWalkAudio != null) ghostWalkAudio.Stop();
        if (hallwayGhost != null) Destroy(hallwayGhost);
    }

    IEnumerator HallwayRunSequence()
    {
        if (ghostRunAudio != null) ghostRunAudio.Play();
        if (runningGhost != null) runningGhost.SetActive(true);
        yield return new WaitForSeconds(ghostRunDuration);
        if (ghostRunAudio != null) ghostRunAudio.Stop();
        if (runningGhost != null) Destroy(runningGhost);
    }

    IEnumerator FinalEndingSequence()
    {
        ToggleControls(false);
        if (faceJumpscareGhost != null)
        {
            faceJumpscareGhost.SetActive(true);
            if (jumpscareScream != null) jumpscareScream.Play();
            Animator anim = faceJumpscareGhost.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("FinalScare");
        }

        yield return new WaitForSeconds(0.8f);

        if (blackScreenOverlay != null)
        {
            float elapsed = 0f;
            float fadeDuration = 1.0f;
            while (elapsed < fadeDuration)
            {
                blackScreenOverlay.color = new Color(0, 0, 0, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            blackScreenOverlay.color = Color.black;
        }

        if (faceJumpscareGhost != null) faceJumpscareGhost.SetActive(false);
        yield return new WaitForSeconds(4.0f);
        SceneManager.LoadScene(loop2SceneName);
    }

    IEnumerator ElectricShockSequence()
    {
        isBeingShocked = true;
        ToggleControls(false);
        if (shockScreamSource != null) shockScreamSource.Play();
        if (cameraLookScript != null) cameraLookScript.TriggerShockShake(shockDuration, shockIntensity);
        yield return new WaitForSeconds(shockDuration);
        if (shockScreamSource != null) shockScreamSource.Stop();
        ToggleControls(true);
        isBeingShocked = false;
    }

    private void HandleFootsteps()
    {
        bool isMoving = (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);
        if (isMoving && movementScript != null && movementScript.enabled)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentInterval = isRunning ? runStepInterval : walkStepInterval;
            AudioClip clipToPlay = isRunning ? runClip : walkClip;
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0)
            {
                if (footstepSource != null && clipToPlay != null)
                {
                    footstepSource.clip = clipToPlay;
                    footstepSource.Play();
                }
                stepTimer = currentInterval;
            }
        }
        else if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
    }

    void TryInteract()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Using QueryTriggerInteraction.Ignore ensures our raycast ONLY hits the solid mesh,
        // not the invisible UI trigger.
        if (Physics.Raycast(ray, out hit, interactionDistance, interactLayer, QueryTriggerInteraction.Ignore))
        {
            GameObject target = hit.collider.gameObject;

            if (target.CompareTag("LiftButton"))
            {
                StartCoroutine(ElectricShockSequence());
                return;
            }

            if (target.CompareTag("Generator"))
            {
                GeneratorLogic gen = target.GetComponent<GeneratorLogic>();
                if (gen != null) gen.Interacted();
                return;
            }

            if (target.CompareTag("Clock"))
            {
                ClockFixLogic clock = target.GetComponent<ClockFixLogic>();
                if (clock != null) clock.Interacted(this);
                return;
            }

            InspectableItem item = target.GetComponent<InspectableItem>();
            if (item != null)
            {
                if (item.noteUIBackground != null)
                {
                    ShowNote(item.itemName, item.itemDescription, item.noteUIBackground, target);
                }
                else
                {
                    StartInspection(target);
                }
                return;
            }

            if (target.CompareTag("LockedDoor"))
            {
                if (hasKey && actualDoor != null)
                {
                    StartCoroutine(SlideDoorOpen(actualDoor));
                }
                return;
            }
        }
    }

    public void StartInspection(GameObject target) { isInspecting = true; ToggleControls(false); Time.timeScale = 0f; if (itemInspector != null) itemInspector.StartInspection(target); }
    public void EndInspection() { isInspecting = false; Time.timeScale = 1f; ToggleControls(true); stepTimer = 0; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    public void ForceCloseEverything() { isReadingNote = false; isInspecting = false; if (notePanel != null) notePanel.SetActive(false); if (itemInspector != null) itemInspector.StopInspection(); ToggleControls(true); Time.timeScale = 1f; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

    public void ShowNote(string content, Sprite noteSprite) { InternalNoteLogic("", content, noteSprite, null); }
    public void ShowNote(string title, string content, Sprite noteSprite, GameObject worldObject) { InternalNoteLogic(title, content, noteSprite, worldObject); }

    private void InternalNoteLogic(string title, string content, Sprite noteSprite, GameObject worldObject)
    {
        isReadingNote = true;
        if (interactionAudioSource != null && notePickupSound != null) interactionAudioSource.PlayOneShot(notePickupSound);

        if (noteUIText != null) noteUIText.text = content;
        if (noteBackgroundUI != null && noteSprite != null) noteBackgroundUI.sprite = noteSprite;

        if (notePanel != null) notePanel.SetActive(true);

        if (!string.IsNullOrEmpty(title) && !collectedNoteTitles.Contains(title))
        {
            collectedNoteTitles.Add(title);
            collectedNoteContents.Add(content);
            if (menuManager != null) menuManager.collectedNoteSprites.Add(noteSprite);
        }

        if (menuManager != null && menuManager.currentStage == MenuManager.ObjectiveStage.Loop2_LiftNote)
        {
            menuManager.UpdateObjectiveStage((int)MenuManager.ObjectiveStage.Loop2_Flashlight);
        }

        if (worldObject != null) Destroy(worldObject);
        ToggleControls(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseNote() { if (!isReadingNote) return; isReadingNote = false; if (notePanel != null) notePanel.SetActive(false); ToggleControls(true); Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

    public void PickUpFuse()
    {
        hasFuse = true;
        PlayItemPickupSound();
        if (menuManager != null) menuManager.UpdateFuseUI();

        // WAKE UP THE DOLL!
        if (creepyDoll != null) creepyDoll.ActivateStalker();
    }
    public void PickUpKey() { hasKey = true; PlayItemPickupSound(); if (menuManager != null) menuManager.UpdateKeyUI(); }
    public void PickUpClockHand() { hasClockHand = true; PlayItemPickupSound(); }

    public void PickedUpFlashlight()
    {
        if (playerFlashlight != null)
        {
            playerFlashlight.SetActive(true);
            isFlashlightOn = true;
            Light lightComp = playerFlashlight.GetComponentInChildren<Light>();
            if (lightComp != null) lightComp.enabled = true;
        }
        ShowTutorial(rightClickIcon, "Right Click to toggle flashlight", 5f);

        if (menuManager != null)
        {
            menuManager.UpdateObjectiveStage((int)MenuManager.ObjectiveStage.Loop2_Generator);
        }
    }

    private void PlayItemPickupSound() { if (interactionAudioSource != null && itemPickupSound != null) interactionAudioSource.PlayOneShot(itemPickupSound); }

    void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;
        Light lightComp = playerFlashlight.GetComponentInChildren<Light>();
        if (lightComp != null) lightComp.enabled = isFlashlightOn;
        if (flashlightClickSound != null) flashlightClickSound.Play();
    }

    public void ActivatePower()
    {
        // THE FIX: Act as a bouncer. If we don't have the fuse, STOP immediately.
        if (!hasFuse)
        {
            Debug.Log("Player tried to turn on power without the fuse!");
            return;
        }

        hasFuse = false;
        isPowerOn = true;

        if (menuManager != null) menuManager.UpdatePowerUI();

        if (powerLight != null)
        {
            powerLight.intensity = 10f;
            powerLight.color = Color.green;
        }

        if (yellowKeyObject != null) yellowKeyObject.SetActive(true);

        PlayItemPickupSound();
    }

    IEnumerator SlideDoorOpen(Transform doorTransform)
    {
        Vector3 startPos = doorTransform.position;
        // Specifically targets your X position while keeping Y and Z the same
        Vector3 targetPos = new Vector3(openXPosition, startPos.y, startPos.z);

        float elapsed = 0;
        // The loop runs until the time is up
        while (elapsed < 1.0f)
        {
            doorTransform.position = Vector3.Lerp(startPos, targetPos, elapsed * slideSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        doorTransform.position = targetPos;
    }

    public void ToggleControls(bool state) { if (movementScript != null) movementScript.enabled = state; if (cameraLookScript != null) cameraLookScript.enabled = state; }
}