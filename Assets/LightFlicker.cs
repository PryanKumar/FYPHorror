using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    private Light baseLight;
    private AudioSource buzzSound;

    [Header("Intensity Settings")]
    public float maxIntensity = 7500.0f;
    public float minIntensity = 0f;

    [Header("On/Off Timing")]
    [Tooltip("How long the light stays ON during a flicker.")]
    public float minOnTime = 0.05f;
    public float maxOnTime = 0.2f;

    [Tooltip("How long the light stays OFF during a flicker.")]
    public float minOffTime = 0.05f;
    public float maxOffTime = 0.2f;

    [Header("The Pause (Randomized per Light)")]
    [Tooltip("The delay before the next On/Off cycle begins.")]
    public float minPauseTime = 1.0f;
    public float maxPauseTime = 5.0f;

    [Header("Audio Settings")]
    public float maxVolume = 0.6f;
    public bool syncPitch = true;

    void Start()
    {
        baseLight = GetComponent<Light>();
        buzzSound = GetComponent<AudioSource>();

        // Start the infinite loop for this specific light
        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // 1. LIGHT ON
            float onTime = Random.Range(minOnTime, maxOnTime);
            SetLightState(maxIntensity, maxVolume, 1.2f);
            yield return new WaitForSeconds(onTime);

            // 2. LIGHT OFF
            float offTime = Random.Range(minOffTime, maxOffTime);
            SetLightState(minIntensity, 0.05f, 0.8f); // Faint buzz when off
            yield return new WaitForSeconds(offTime);

            // 3. THE PAUSE
            // Randomized duration ensures multiple lights don't sync up
            float pauseTime = Random.Range(minPauseTime, maxPauseTime);

            // Decide if light stays ON or OFF during the long pause
            // Usually, staying OFF is creepier for horror
            SetLightState(minIntensity, 0f, 0.8f);

            yield return new WaitForSeconds(pauseTime);
        }
    }

    void SetLightState(float intensity, float volume, float pitch)
    {
        if (baseLight != null) baseLight.intensity = intensity;

        if (buzzSound != null)
        {
            buzzSound.volume = volume;
            if (syncPitch) buzzSound.pitch = pitch;
        }
    }
}