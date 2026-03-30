using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private float footstepInterval = 0.4f;
    private Animator animator;
    private Vector2 currentFacingDir = Vector2.down;
    private float footstepTimer = 0f;
    void Awake()
    {
        animator = GetComponent<Animator>();
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

    public void UpdateAnimation(Vector2 velocity)
    {
        if (animator == null) return;

        float speed = velocity.magnitude;


        if (velocity.sqrMagnitude > 0.01f)
            currentFacingDir = velocity.normalized;

        if (speed > 0.01f)
        {
            bool horizontal = Mathf.Abs(currentFacingDir.x) >= Mathf.Abs(currentFacingDir.y);
            animator.SetInteger("right", horizontal && currentFacingDir.x > 0 ? 1 : 0);
            animator.SetInteger("left", horizontal && currentFacingDir.x < 0 ? 1 : 0);
            animator.SetInteger("up", !horizontal && currentFacingDir.y > 0 ? 1 : 0);
            animator.SetInteger("down", !horizontal && currentFacingDir.y < 0 ? 1 : 0);
        }
        else
        {
            animator.SetInteger("right", 0);
            animator.SetInteger("left", 0);
            animator.SetInteger("up", 0);
            animator.SetInteger("down", 0);
        }
    }
}