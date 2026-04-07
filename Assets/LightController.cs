using UnityEngine;

public class LightController : MonoBehaviour
{
    [Header("Breathing Light (Lift)")]
    public Light breathingLight;
    public float minBreathingIntensity = 20f; // New minimum floor
    public float maxBreathingIntensity = 70f;
    public float breathingSpeed = 1.5f;

    [Header("Blinking Light")]
    public Light blinkingLight;
    public float blinkingIntensity = 70f;
    public float blinkInterval = 1.0f;

    private float timer;
        
    void Update()
    {
        // 1. Breathing Logic: Using Sin wave mapped to a specific range
        if (breathingLight != null)
        {
            // Sin returns -1 to 1, we map it to 0 to 1
            float rawSin = Mathf.Sin(Time.time * breathingSpeed);
            float normalizedSin = (rawSin + 1f) / 2f;

            // Interpolate between your 20 and 70 values
            breathingLight.intensity = Mathf.Lerp(minBreathingIntensity, maxBreathingIntensity, normalizedSin);
        }

        // 2. Blinking Logic: Remains the same
        if (blinkingLight != null)
        {
            timer += Time.deltaTime;
            if (timer >= blinkInterval)
            {
                blinkingLight.intensity = (blinkingLight.intensity == 0) ? blinkingIntensity : 0;
                timer = 0;
            }
        }
    }
}