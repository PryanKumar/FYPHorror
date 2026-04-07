using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class FearVFXManager : MonoBehaviour
{
    public FPSMovement playerMovement; // Reference to your movement script
    public PostProcessVolume volume;   // Reference to the FearVolume

    private Vignette vignette;
    private LensDistortion distortion;

    void Start()
    {
        // Link into the Post Process settings
        volume.profile.TryGetSettings(out vignette);
        volume.profile.TryGetSettings(out distortion);
    }

    void Update()
    {
        // Calculate percentage (0.0 = empty, 1.0 = full)
        float staminaPercent = playerMovement.currentStamina / playerMovement.maxStamina;

        // This new math makes the effect start appearing as soon as you are below 50%
        // and gets very intense at 0%
        float fearIntensity = 1f - staminaPercent;

        if (vignette != null)
        {
            // Darkens edges (0.45 is a good max)
            vignette.intensity.value = fearIntensity * 0.5f;
        }

        if (distortion != null)
        {
            // Warps the screen (-40 is heavy distortion)
            distortion.intensity.value = fearIntensity * -40f;
        }
    }
}