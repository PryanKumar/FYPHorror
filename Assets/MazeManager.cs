using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MazeManager : MonoBehaviour
{
    [Header("Room Setup")]
    public MazeRoom currentRoom;
    public List<GameObject> roomPrefabs;
    public GameObject operatingTheatrePrefab;

    [Header("Endings & UI")]
    public Transform storeRoomTeleportPoint;
    public CanvasGroup blackScreenFade;
    public CanvasGroup gameOverUI;
    public string mainMenuSceneName = "MainMenu";
    public float fadeSpeed = 1f;

    [Header("Player Freeze Logic")]
    public MonoBehaviour playerMouseLookScript;
    public MonoBehaviour playerMovementScript;

    [Header("Game Rules")]
    public int requiredCorrectToWin = 3;
    public int maxWrongBeforeFail = 3;

    [Header("Fail State Tracking (Read Only)")]
    public int correctChoices = 0;
    public int wrongChoices = 0;
    public int loopFails = 0;

    private GameObject[] stagedRooms = new GameObject[2];
    private int lastSelectedDoorIndex = -1;

    [HideInInspector] public MazeDoor lastUsedDoor;

    public void OnDoorSelected(int doorIndex, bool isCorrectPath)
    {
        if (doorIndex >= stagedRooms.Length || stagedRooms[doorIndex] != null) return;

        lastSelectedDoorIndex = doorIndex;

        if (isCorrectPath)
        {
            correctChoices++;
        }
        else
        {
            correctChoices = 0;
            wrongChoices++;
        }

        DetermineNextRoom(doorIndex);
    }

    private void DetermineNextRoom(int doorIndex)
    {
        if (correctChoices >= requiredCorrectToWin)
        {
            StartCoroutine(CinematicTeleport());
            return;
        }

        GameObject selectedPrefab = null;

        if (wrongChoices >= maxWrongBeforeFail)
        {
            loopFails++;
            wrongChoices = 0;
            correctChoices = 0;

            selectedPrefab = operatingTheatrePrefab;
        }
        else
        {
            if (roomPrefabs.Count > 0)
            {
                selectedPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            }
        }

        if (selectedPrefab != null)
        {
            SpawnAndSnapRoom(selectedPrefab, doorIndex);
        }
    }

    private void SpawnAndSnapRoom(GameObject prefab, int doorIndex)
    {
        GameObject newRoomObj = Instantiate(prefab);
        MazeRoom nextRoomScript = newRoomObj.GetComponent<MazeRoom>();

        Transform exitAnchor = currentRoom.exitAnchors[doorIndex];
        Transform entryAnchor = nextRoomScript.entryAnchor;

        float rotationOffset = exitAnchor.eulerAngles.y - entryAnchor.eulerAngles.y;
        newRoomObj.transform.Rotate(0, rotationOffset + 180, 0);
        newRoomObj.transform.position += (exitAnchor.position - entryAnchor.position);

        stagedRooms[doorIndex] = newRoomObj;

        if (newRoomObj.name.Contains(operatingTheatrePrefab.name))
        {
            Light otLight = newRoomObj.GetComponentInChildren<Light>();
            MannequinScareLogic scareLogic = newRoomObj.GetComponent<MannequinScareLogic>();

            if (loopFails == 1)
            {
                if (otLight != null) otLight.intensity = 150f;
                if (scareLogic != null) scareLogic.SetPassiveMode();
            }
            else if (loopFails >= 2)
            {
                if (otLight != null) otLight.intensity = 0f;
                if (scareLogic != null) scareLogic.PrepareJumpscare();
            }
        }
    }

    public void ConfirmRoomEntry(MazeRoom enteredRoom)
    {
        if (currentRoom != enteredRoom)
        {
            if (lastUsedDoor != null)
            {
                lastUsedDoor.CloseDoor();
                lastUsedDoor.transform.SetParent(enteredRoom.transform, true);
            }

            if (loopFails >= 2 && enteredRoom.gameObject.name.Contains(operatingTheatrePrefab.name))
            {
                MannequinScareLogic scareLogic = enteredRoom.GetComponent<MannequinScareLogic>();
                StartCoroutine(GameOverSequence(scareLogic));
            }

            if (currentRoom != null && currentRoom.gameObject.name != operatingTheatrePrefab.name && currentRoom.gameObject.name.Contains("(Clone)"))
            {
                Destroy(currentRoom.gameObject, 1.5f);
            }

            currentRoom = enteredRoom;
            System.Array.Clear(stagedRooms, 0, stagedRooms.Length);
            lastSelectedDoorIndex = -1;
            lastUsedDoor = null;
        }
    }

    // --- CINEMATICS & ENDINGS ---

    private IEnumerator GameOverSequence(MannequinScareLogic scareLogic)
    {
        Debug.Log("TRIGGERED: Player stepped in the trap! Freezing and Screaming!");

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = true; rb.linearVelocity = Vector3.zero; }

            if (playerMovementScript != null) playerMovementScript.enabled = false;
            if (playerMouseLookScript != null) playerMouseLookScript.enabled = false;
        }

        // THE UPGRADE: We pass the player's transform to the mannequin!
        if (scareLogic != null && player != null)
        {
            scareLogic.ExecuteJumpscare(player.transform);

            yield return new WaitForSeconds(scareLogic.screamAnimationLength);
        }

        if (blackScreenFade != null)
        {
            while (blackScreenFade.alpha < 1f)
            {
                blackScreenFade.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverUI != null)
        {
            while (gameOverUI.alpha < 1f)
            {
                gameOverUI.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            gameOverUI.blocksRaycasts = true;
            gameOverUI.interactable = true;
        }
    }

    private IEnumerator CinematicTeleport()
    {
        Debug.Log("Player Escaped! Starting Fade...");

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) Debug.LogError("TELEPORT FAILED: Could not find an object tagged 'Player'.");
        if (storeRoomTeleportPoint == null) Debug.LogError("TELEPORT FAILED: Store Room Teleport Point is empty in the Manager!");

        CharacterController cc = null;
        Rigidbody rb = null;

        if (player != null)
        {
            cc = player.GetComponent<CharacterController>();
            rb = player.GetComponent<Rigidbody>();

            if (cc != null) cc.enabled = false;
            if (rb != null) { rb.isKinematic = true; rb.linearVelocity = Vector3.zero; }

            if (playerMovementScript != null) playerMovementScript.enabled = false;
            if (playerMouseLookScript != null) playerMouseLookScript.enabled = false;
        }

        if (blackScreenFade != null)
        {
            while (blackScreenFade.alpha < 1f)
            {
                blackScreenFade.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1.5f);

        if (player != null && storeRoomTeleportPoint != null)
        {
            player.transform.position = storeRoomTeleportPoint.position;
            player.transform.rotation = storeRoomTeleportPoint.rotation;
            Physics.SyncTransforms();
        }

        if (blackScreenFade != null)
        {
            while (blackScreenFade.alpha > 0f)
            {
                blackScreenFade.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        if (cc != null) cc.enabled = true;
        if (rb != null) rb.isKinematic = false;

        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (playerMouseLookScript != null) playerMouseLookScript.enabled = true;
    }

    public void RestartLoop() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void QuitToMenu() { SceneManager.LoadScene(mainMenuSceneName); }
}