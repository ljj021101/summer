using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Ground Check")]
    [SerializeField] private BoxCollider2D groundCheckCollider;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool flipSprite = true;

    private Rigidbody2D rb;

    private float moveInput;
    private bool jumpPressed;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        ReadInput();
        CheckGround();
        HandleJump();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void ReadInput()
    {
        moveInput = 0f;
        jumpPressed = false;

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput += 1f;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            jumpPressed = true;
        }
    }

    private void CheckGround()
    {
        if (groundCheckCollider == null)
        {
            isGrounded = false;
            return;
        }

        Bounds bounds = groundCheckCollider.bounds;

        isGrounded = Physics2D.OverlapBox(
            bounds.center,
            bounds.size,
            0f,
            groundLayer
        );
    }

    private void HandleJump()
    {
        if (!jumpPressed)
        {
            return;
        }

        if (!isGrounded)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void UpdateAnimation()
    {
        bool isMoving = Mathf.Abs(moveInput) > 0.01f;

        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }

        if (flipSprite && spriteRenderer != null && isMoving)
        {
            spriteRenderer.flipX = moveInput < 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckCollider == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;

        Bounds bounds = groundCheckCollider.bounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}