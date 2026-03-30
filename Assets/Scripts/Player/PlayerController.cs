using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float easeInDuration = 0.1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerAnimationController animController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animController = GetComponent<PlayerAnimationController>();
    }

    public void OnMove(InputValue value)
    {
        bool canMove = GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.LevelPlaying;
        if (!canMove)
        {
            moveInput = Vector2.zero;

            return;
        }

        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput.normalized * moveSpeed;

        if (moveInput == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            animController.ResetFootstepTimer();
        }
        else
        {

            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, easeInDuration / Time.fixedDeltaTime * Time.deltaTime);

        }

        if (animController != null)
        {
            animController.UpdateAnimation(rb.linearVelocity);
            if (moveInput != Vector2.zero)
                animController.UpdateFootsteps(Time.deltaTime);
        }

    }
}
