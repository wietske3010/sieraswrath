using UnityEngine;

public class EnemySoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private AudioClip breathingClip;
    [SerializeField] private float footstepInterval = 0.4f;
    [SerializeField] private float breathingIntervalMin = 4f;
    [SerializeField] private float breathingIntervalMax = 8f;

    private float footstepTimer = 0f;
    private float breathingTimer = 0f;

    public void StartBreathing()
    {

        breathingTimer = Random.Range(breathingIntervalMin, breathingIntervalMax);
    }

    // Call every FixedUpdate tick while the enemy is moving.
    public void UpdateFootsteps(float deltaTime)
    {
        footstepTimer -= deltaTime;
        if (footstepTimer <= 0f)
        {
            footstepTimer = footstepInterval;
            if (footstepClips != null && footstepClips.Length > 0 && footstepSource != null)
                footstepSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
        }
    }

    // Call when the enemy stops moving to prevent a footstep firing immediately on next move.
    public void ResetFootstepTimer()
    {
        footstepTimer = footstepInterval;
    }

    public void UpdateBreathing(float deltaTime)
    {
        if (breathingSource == null) return;

        breathingTimer -= deltaTime;
        if (breathingTimer <= 0f)
        {
            breathingSource.PlayOneShot(breathingClip);
            breathingTimer = Random.Range(breathingIntervalMin, breathingIntervalMax);
        }
    }
}
