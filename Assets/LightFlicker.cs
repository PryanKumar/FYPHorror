using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    private Light baseLight;
    private AudioSource buzzSound;

    [Header("Intensity Settings")]
    public float maxIntensity = 800.0f;
    [Tooltip("Keep this above 0 so the room is never completely pitch black!")]
    public float minIntensity = 50.0f;

    [Header("Breathing Settings")]
    [Tooltip("How fast the light breathes in and out.")]
    public float breatheSpeed = 2f;

    [Header("Audio Settings")]
    public float maxVolume = 0.6f;
    public float minVolume = 0.1f;
    public bool syncPitch = true;

    private float randomOffset;

    void Start()
    {
        baseLight = GetComponent<Light>();
        buzzSound = GetComponent<AudioSource>();

        // Gives each light a random start point. 
        // This ensures that if you have 5 lights in a hallway, they don't all breathe in perfect robotic unison.
        randomOffset = Random.Range(0f, 100f);

        // Make sure the audio is playing and looping
        if (buzzSound != null && !buzzSound.isPlaying)
        {
            buzzSound.loop = true;
            buzzSound.Play();
        }
    }

    void Update()
    {
        // 1. Calculate the breathing curve (Mathf.Sin naturally goes smoothly up and down)
        // Adding 1 and dividing by 2 turns the math into a clean percentage between 0.0 and 1.0
        float breathingRatio = (Mathf.Sin((Time.time + randomOffset) * breatheSpeed) + 1f) / 2f;

        // 2. Smoothly adjust the light intensity based on that curve
        if (baseLight != null)
        {
            baseLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, breathingRatio);
        }

        // 3. Smoothly adjust the sound to match the light getting brighter/dimmer
        if (buzzSound != null)
        {
            buzzSound.volume = Mathf.Lerp(minVolume, maxVolume, breathingRatio);

            if (syncPitch)
            {
                // Pitch gently drops when the light dims, and rises when it brightens
                buzzSound.pitch = Mathf.Lerp(0.8f, 1.2f, breathingRatio);
            }
        }
    }
}