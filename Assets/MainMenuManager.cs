using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The exact name of your first loop scene")]
    public string firstLevelName = "Loop1";

    [Header("Fade Settings")]
    [Tooltip("Drag the black 'Fader' Image here")]
    public Image faderImage;
    public float fadeSpeed = 0.8f;

    [Header("Audio Sources")]
    [Tooltip("Source for UI Clicks and Hovers")]
    public AudioSource uiAudioSource;
    [Tooltip("Source for the looping Menu Theme")]
    public AudioSource themeAudioSource;
    [Tooltip("Source for the looping Blinking Light buzzing")]
    public AudioSource lightAudioSource;

    [Header("Audio Clips")]
    public AudioClip scrollSound; // Hover SFX
    public AudioClip clickSound;  // Click SFX
    public AudioClip menuTheme;   // The background music
    public AudioClip lightHum;    // The buzzing light SFX

    [Header("UI Sound Pitch Settings")]
    [Range(0.1f, 2f)] public float minPitch = 0.85f;
    [Range(0.1f, 2f)] public float maxPitch = 1.05f;

    void Start()
    {
        // Ensure cursor is usable in the menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (faderImage != null)
        {
            // Start transparent so the player can see the menu
            faderImage.color = new Color(0, 0, 0, 0);
            faderImage.raycastTarget = false;
        }

        // Initialize looping sounds
        StartLoopingSounds();
    }

    void StartLoopingSounds()
    {
        // Setup Theme
        if (themeAudioSource != null && menuTheme != null)
        {
            themeAudioSource.clip = menuTheme;
            themeAudioSource.loop = true;
            themeAudioSource.Play();
        }

        // Setup Light Hum
        if (lightAudioSource != null && lightHum != null)
        {
            lightAudioSource.clip = lightHum;
            lightAudioSource.loop = true;
            lightAudioSource.Play();
        }
    }

    // --- AUDIO FUNCTIONS ---

    public void PlayScrollSound()
    {
        if (uiAudioSource != null && scrollSound != null)
        {
            // Randomize pitch slightly for a "haunted/glitchy" medical feel
            uiAudioSource.pitch = Random.Range(minPitch, maxPitch);
            uiAudioSource.PlayOneShot(scrollSound);
        }
    }

    public void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.pitch = 1.0f; // Reset pitch for the main click
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void PlayGame()
    {
        PlayClickSound(); // Trigger sound immediately on click
        StartCoroutine(FadeAndExit());
    }

    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Game is quitting...");
        Application.Quit();
    }

    IEnumerator FadeAndExit()
    {
        if (faderImage == null)
        {
            Debug.LogError("Please assign the Fader Image in the Inspector!");
            SceneManager.LoadScene(firstLevelName);
            yield break;
        }

        // Block further clicks during the fade
        faderImage.raycastTarget = true;
        float alpha = 0;

        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            faderImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        SceneManager.LoadScene(firstLevelName);
    }
}