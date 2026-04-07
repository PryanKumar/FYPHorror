using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // This is the function the Cinematic script will call
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Generate a random jitter effect
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Apply the jitter to the camera's local position
            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            // Wait until the next frame to move again
            yield return null;
        }

        // Return the camera to exactly where it started
        transform.localPosition = originalPos;
    }
}