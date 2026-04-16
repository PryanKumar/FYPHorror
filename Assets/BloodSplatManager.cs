using System.Collections.Generic;
using UnityEngine;

public class BloodSplatManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject splatPrefab;
    public GameObject splashEffectPrefab; // NEW: The explosion of tiny droplets

    public float minScale = 0.05f;      // Start much smaller
    public float maxScale = 0.4f;       // Don't let them fill the hallway
    public float growthAmount = 0.01f; // VERY small growth per hit
    public int maxSplats = 1000;

    [Header("Detection")]
    public LayerMask bloodLayer;
    public float detectionRadius = 0.05f; // Must be almost exactly on top to "stack"

    private ParticleSystem bloodSystem;
    private List<ParticleCollisionEvent> collisionEvents;
    private int currentSplatCount = 0;

    void Start()
    {
        bloodSystem = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = bloodSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++)
        {
            Vector3 pos = collisionEvents[i].intersection;
            Vector3 normal = collisionEvents[i].normal;

            // --- NEW: SPAWN THE 3D SPLASH EFFECT ---
            if (splashEffectPrefab != null)
            {
                // Spawn a tiny burst of particles and destroy it after 1 second so it doesn't lag the game
                GameObject splashBurst = Instantiate(splashEffectPrefab, pos, Quaternion.LookRotation(normal));
                Destroy(splashBurst, 1f);
            }

            Collider[] existingSplats = Physics.OverlapSphere(pos, detectionRadius, bloodLayer);

            if (existingSplats.Length > 0)
            {
                Transform existingSplat = existingSplats[0].transform;
                if (existingSplat.localScale.x < maxScale)
                {
                    // Grow much more slowly
                    existingSplat.localScale += new Vector3(growthAmount, growthAmount, 0);
                }
            }
            else
            {
                if (currentSplatCount >= maxSplats) return;

                // Use a smaller offset (0.01f) so they don't "float" off the wall
                GameObject splat = Instantiate(splatPrefab, pos + (normal * 0.01f), Quaternion.LookRotation(normal));

                splat.transform.localScale = new Vector3(minScale, minScale, 1f);
                splat.transform.Rotate(Vector3.forward, Random.Range(0, 360f));
                splat.transform.SetParent(other.transform);

                currentSplatCount++;
            }
        }
    }
}