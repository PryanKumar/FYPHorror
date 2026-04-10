using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Needed to restart the level

public class MazeManager : MonoBehaviour
{
    [Header("Room Setup")]
    public MazeRoom currentRoom;
    public List<GameObject> roomPrefabs;
    public GameObject operatingTheatrePrefab;

    [Header("Endings & UI")]
    public Transform storeRoomTeleportPoint;
    public CanvasGroup blackScreenFade; // Drag 'BlackFade' here
    public CanvasGroup gameOverUI;      // Drag 'GameOverUI' here
    public string mainMenuSceneName = "MainMenu"; // Make sure this exactly matches your menu scene name
    public float fadeSpeed = 1f;

    [Header("Game Rules")]
    public int requiredCorrectToWin = 3;
    public int maxWrongBeforeFail = 3;

    [Header("Fail State Tracking (Read Only)")]
    public int correctChoices = 0;
    public int wrongChoices = 0;
    public int loopFails = 0;

    private GameObject[] stagedRooms = new GameObject[2];
    private int lastSelectedDoorIndex = -1;

    public void OnDoorSelected(int doorIndex, bool isCorrectPath)
    {
        if (doorIndex >= stagedRooms.Length || stagedRooms[doorIndex] != null) return;

        lastSelectedDoorIndex = doorIndex;

        if (isCorrectPath)
        {
            correctChoices++;
            Debug.Log("Correct choice! Sequence: " + correctChoices + "/" + requiredCorrectToWin);
        }
        else
        {
            correctChoices = 0;
            wrongChoices++;
            Debug.Log("Wrong choice! Mistakes: " + wrongChoices + "/" + maxWrongBeforeFail);
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

            if (loopFails == 1)
            {
                selectedPrefab = operatingTheatrePrefab;
            }
            else if (loopFails >= 2)
            {
                selectedPrefab = operatingTheatrePrefab;
                StartCoroutine(GameOverSequence()); // Triggers the Death Screen
            }
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
            if (otLight != null)
            {
                if (loopFails == 1) otLight.intensity = 150f;
                else if (loopFails >= 2) otLight.intensity = 0f;
            }
        }
    }

    public void ConfirmRoomEntry(MazeRoom enteredRoom)
    {
        if (currentRoom != enteredRoom)
        {
            if (lastSelectedDoorIndex != -1)
            {
                MazeDoor doorToSlam = currentRoom.exitAnchors[lastSelectedDoorIndex].GetComponentInChildren<MazeDoor>();
                if (doorToSlam != null) doorToSlam.CloseDoor();
            }

            if (currentRoom.gameObject.name != operatingTheatrePrefab.name && currentRoom.gameObject.name.Contains("(Clone)"))
            {
                Destroy(currentRoom.gameObject, 1f);
            }

            currentRoom = enteredRoom;
            System.Array.Clear(stagedRooms, 0, stagedRooms.Length);
            lastSelectedDoorIndex = -1;
        }
    }

    // --- CINEMATICS & ENDINGS ---

    private IEnumerator CinematicTeleport()
    {
        Debug.Log("Player Escaped! Starting Fade...");

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) Debug.LogError("TELEPORT FAILED: Could not find an object tagged 'Player'.");
        if (storeRoomTeleportPoint == null) Debug.LogError("TELEPORT FAILED: Store Room Teleport Point is empty in the Manager!");

        CharacterController cc = null;
        Rigidbody rb = null;

        // 1. FREEZE THE PLAYER COMPLETELY
        if (player != null)
        {
            cc = player.GetComponent<CharacterController>();
            rb = player.GetComponent<Rigidbody>();

            if (cc != null) cc.enabled = false;
            if (rb != null) { rb.isKinematic = true; rb.linearVelocity = Vector3.zero; }
        }

        // 2. FADE TO BLACK
        if (blackScreenFade != null)
        {
            while (blackScreenFade.alpha < 1f)
            {
                blackScreenFade.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1.5f);

        // 3. THE BULLETPROOF TELEPORT
        if (player != null && storeRoomTeleportPoint != null)
        {
            player.transform.position = storeRoomTeleportPoint.position;
            player.transform.rotation = storeRoomTeleportPoint.rotation;

            // Force Unity's physics engine to accept the new location
            Physics.SyncTransforms();
        }

        // 4. FADE BACK IN
        if (blackScreenFade != null)
        {
            while (blackScreenFade.alpha > 0f)
            {
                blackScreenFade.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        // 5. UNFREEZE THE PLAYER
        if (cc != null) cc.enabled = true;
        if (rb != null) rb.isKinematic = false;
    }

    private IEnumerator GameOverSequence()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // Freeze player
        }

        Cursor.lockState = CursorLockMode.None; // Free the mouse
        Cursor.visible = true;

        // 1. Fade to Black first
        if (blackScreenFade != null)
        {
            while (blackScreenFade.alpha < 1f)
            {
                blackScreenFade.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        // 2. Fade in the UI second
        if (gameOverUI != null)
        {
            while (gameOverUI.alpha < 1f)
            {
                gameOverUI.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            gameOverUI.blocksRaycasts = true; // Let the buttons be clicked
            gameOverUI.interactable = true;
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void RestartLoop()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}