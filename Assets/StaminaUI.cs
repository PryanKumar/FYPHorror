using UnityEngine;
using UnityEngine.UI; // Required for UI elements

public class StaminaUI : MonoBehaviour
{
    [Header("UI Components")]
    public Slider staminaSlider;    // Drag the Slider here
    public CanvasGroup canvasGroup; // Add a CanvasGroup component to the Slider to allow fading

    [Header("References")]
    public FPSMovement playerMovement; // Drag the Player Capsule here

    [Header("Settings")]
    public bool hideWhenFull = true;
    public float fadeSpeed = 5f;

    void Start()
    {
        // Initialize the slider values based on the player's lung capacity
        if (playerMovement != null)
        {
            staminaSlider.maxValue = playerMovement.maxStamina;
            staminaSlider.value = playerMovement.currentStamina;
        }
    }

    void Update()
    {
        if (playerMovement == null || staminaSlider == null) return;

        // 1. Update Slider Value
        staminaSlider.value = playerMovement.currentStamina;

        // 2. Handle Color Feedback
        // If the player is exhausted (Lung capacity is 0), turn the bar red
        Image fillImage = staminaSlider.fillRect.GetComponent<Image>();
        if (playerMovement.currentStamina <= 0.1f)
        {
            fillImage.color = Color.red;
        }
        else
        {
            fillImage.color = Color.white; // Or any "breath" color you prefer
        }

        // 3. Handle Smooth Fading (Immersion)
        if (hideWhenFull)
        {
            float targetAlpha = (playerMovement.currentStamina < playerMovement.maxStamina) ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }
}