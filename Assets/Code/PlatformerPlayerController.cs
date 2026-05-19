using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 45f;
    [SerializeField] private float deceleration = 55f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float airJumpForce = 11f;
    [SerializeField, Range(0.1f, 1f)] private float jumpCutMultiplier = 0.45f;
    [SerializeField] private int maxAirJumps = 1;

    [Header("Ground Check")]
    [SerializeField] private BoxCollider2D groundCheckCollider;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float groundNormalThreshold = 0.7f;
    [SerializeField] private float coyoteTime = 0.1f;

    [Header("Wall Check")]
    [SerializeField] private BoxCollider2D leftWallCheckCollider;
    [SerializeField] private BoxCollider2D rightWallCheckCollider;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float wallNormalThreshold = 0.7f;

    [Header("Wall Jump")]
    [SerializeField] private Vector2 wallJumpForce = new Vector2(6f, 6f);
    [SerializeField] private float wallJumpInputLockDuration = 0.18f;
    [SerializeField] private float wallJumpSpinInputWindow = 0.1f;
    [SerializeField] private float wallCoyoteTime = 0.05f;
    [SerializeField] private float wallSlideSpeed = 2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool flipSprite = true;

    [Header("Landing Squash")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Vector2 landingSquashScale = new Vector2(1.15f, 0.85f);
    [SerializeField] private Vector2 maxLandingSquashScale = new Vector2(1.4f, 0.7f);
    [SerializeField] private float minLandingSquashSpeed = 2f;
    [SerializeField] private float maxLandingSquashSpeed = 12f;
    [SerializeField] private float landingSquashDuration = 0.08f;
    [SerializeField] private float landingRecoverDuration = 0.12f;
    [SerializeField] private float visualHeight = 0f;

    [Header("Double Jump Spin")]
    [SerializeField] private bool spinOnAirJump = true;
    [SerializeField] private float airJumpSpinDuration = 0.28f;

    private Rigidbody2D rb;

    private float moveInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool isGrounded;
    private bool wasGrounded;
    private bool hasCheckedGround;
    private bool isTouchingWall;
    private int wallJumpDirection;
    private int lastWallJumpDirection;
    private int wallJumpBlockedMoveDirection;
    private int airJumpsRemaining;
    private int facingDirection = 1;
    private float lastFallSpeed;
    private float lastGroundedTime = float.NegativeInfinity;
    private float wallJumpInputLockUntil = float.NegativeInfinity;
    private float lastWallTouchTime = float.NegativeInfinity;
    private float pendingWallJumpSpinUntil = float.NegativeInfinity;
    private int pendingWallJumpSpinDirection;
    private Vector2 currentLandingSquashScale = Vector2.one;
    private Vector3 visualBaseScale = Vector3.one;
    private Vector3 visualBaseLocalPosition;
    private Quaternion visualBaseRotation = Quaternion.identity;
    private Coroutine landingSquashRoutine;
    private Coroutine airJumpSpinRoutine;
    private float animatorBaseSpeed = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animatorBaseSpeed = animator.speed;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (visualRoot == null && spriteRenderer != null && spriteRenderer.transform != transform)
        {
            visualRoot = spriteRenderer.transform;
        }

        if (visualRoot != null)
        {
            visualBaseScale = visualRoot.localScale;
            visualBaseLocalPosition = visualRoot.localPosition;
            visualBaseRotation = visualRoot.localRotation;
        }

        if (visualHeight <= 0f && spriteRenderer != null)
        {
            visualHeight = spriteRenderer.localBounds.size.y;
        }

        currentLandingSquashScale = landingSquashScale;
    }

    private void Update()
    {
        ReadInput();
        CheckGround();
        CheckWall();
        HandleJump();
        HandleVariableJumpHeight();
        HandlePendingWallJumpSpin();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        float effectiveMoveInput = GetEffectiveMoveInput();
        float targetSpeed = effectiveMoveInput * moveSpeed;
        float currentHorizontalSpeed = rb.linearVelocity.x;

        if (IsWallJumpMoveBlocked(currentHorizontalSpeed))
        {
            currentHorizontalSpeed = 0f;
        }

        float speedChangeRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float newHorizontalSpeed = Mathf.MoveTowards(
            currentHorizontalSpeed,
            targetSpeed,
            speedChangeRate * Time.fixedDeltaTime
        );

        float verticalSpeed = GetWallSlideVerticalSpeed(rb.linearVelocity.y);
        rb.linearVelocity = new Vector2(newHorizontalSpeed, verticalSpeed);
    }

    private float GetWallSlideVerticalSpeed(float currentVerticalSpeed)
    {
        if (!isTouchingWall || isGrounded || currentVerticalSpeed >= 0f)
        {
            return currentVerticalSpeed;
        }

        if (IsFastWallSlideHeld())
        {
            return currentVerticalSpeed;
        }

        return Mathf.Max(currentVerticalSpeed, -wallSlideSpeed);
    }

    private bool IsFastWallSlideHeld()
    {
        return Keyboard.current != null && Keyboard.current.sKey.isPressed;
    }

    private float GetEffectiveMoveInput()
    {
        if (Time.time >= wallJumpInputLockUntil)
        {
            return moveInput;
        }

        if (wallJumpBlockedMoveDirection < 0 && moveInput < -0.01f)
        {
            return 0f;
        }

        if (wallJumpBlockedMoveDirection > 0 && moveInput > 0.01f)
        {
            return 0f;
        }

        return moveInput;
    }

    private bool IsWallJumpMoveBlocked(float horizontalSpeed)
    {
        if (Time.time >= wallJumpInputLockUntil)
        {
            return false;
        }

        if (wallJumpBlockedMoveDirection < 0 && horizontalSpeed < -0.01f)
        {
            return true;
        }

        return wallJumpBlockedMoveDirection > 0 && horizontalSpeed > 0.01f;
    }

    private void ReadInput()
    {
        moveInput = 0f;
        jumpPressed = false;
        jumpReleased = false;

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

        if ((Keyboard.current.spaceKey.wasReleasedThisFrame ||
            Keyboard.current.wKey.wasReleasedThisFrame ||
            Keyboard.current.upArrowKey.wasReleasedThisFrame) &&
            !IsJumpKeyHeld())
        {
            jumpReleased = true;
        }
    }

    private bool IsJumpKeyHeld()
    {
        return Keyboard.current.spaceKey.isPressed ||
            Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed;
    }

    private void CheckGround()
    {
        if (groundCheckCollider == null)
        {
            isGrounded = false;
            return;
        }

        Bounds bounds = groundCheckCollider.bounds;

        RaycastHit2D groundHit = Physics2D.BoxCast(
            bounds.center,
            bounds.size,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        isGrounded = groundHit.collider != null && groundHit.normal.y >= groundNormalThreshold;

        if (hasCheckedGround && isGrounded && !wasGrounded)
        {
            PlayLandingSquash(lastFallSpeed);
        }

        if (!isGrounded)
        {
            lastFallSpeed = Mathf.Max(0f, -rb.linearVelocity.y);
        }

        if (isGrounded)
        {
            ClearPendingWallJumpSpin();
            lastGroundedTime = Time.time;
            airJumpsRemaining = maxAirJumps;
        }

        wasGrounded = isGrounded;
        hasCheckedGround = true;
    }

    private void CheckWall()
    {
        isTouchingWall = false;
        wallJumpDirection = 0;

        LayerMask currentWallLayer = wallLayer.value == 0 ? groundLayer : wallLayer;

        if (leftWallCheckCollider != null && IsTouchingWall(leftWallCheckCollider, Vector2.left, 1f, currentWallLayer))
        {
            isTouchingWall = true;
            wallJumpDirection = 1;
            RememberWallTouch();
            airJumpsRemaining = maxAirJumps;
            return;
        }

        if (rightWallCheckCollider != null && IsTouchingWall(rightWallCheckCollider, Vector2.right, -1f, currentWallLayer))
        {
            isTouchingWall = true;
            wallJumpDirection = -1;
            RememberWallTouch();
            airJumpsRemaining = maxAirJumps;
        }
    }

    private void RememberWallTouch()
    {
        lastWallJumpDirection = wallJumpDirection;
        lastWallTouchTime = Time.time;
    }

    private bool IsTouchingWall(BoxCollider2D wallCheckCollider, Vector2 direction, float requiredNormalX, LayerMask currentWallLayer)
    {
        Bounds bounds = wallCheckCollider.bounds;
        RaycastHit2D wallHit = Physics2D.BoxCast(
            bounds.center,
            bounds.size,
            0f,
            direction,
            wallCheckDistance,
            currentWallLayer
        );

        return wallHit.collider != null && wallHit.normal.x * requiredNormalX >= wallNormalThreshold;
    }

    private void HandleJump()
    {
        if (!jumpPressed)
        {
            return;
        }

        if (isGrounded)
        {
            PerformGroundJump();
            return;
        }

        if (isTouchingWall)
        {
            PerformWallJump();
            return;
        }

        if (CanUseWallCoyoteJump())
        {
            PerformWallJump(lastWallJumpDirection);
            return;
        }

        if (CanUseCoyoteJump())
        {
            PerformGroundJump();
            return;
        }

        if (airJumpsRemaining <= 0)
        {
            return;
        }

        PerformAirJump();
    }

    private void PerformAirJump()
    {
        ClearPendingWallJumpSpin();
        UpdateFacingFromMoveInput();
        airJumpsRemaining--;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, airJumpForce);
        PlayAirJumpSpin();
    }

    private void PerformGroundJump()
    {
        ClearPendingWallJumpSpin();
        airJumpsRemaining = maxAirJumps;
        lastGroundedTime = float.NegativeInfinity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void PerformWallJump()
    {
        PerformWallJump(wallJumpDirection);
    }

    private void PerformWallJump(int jumpDirection)
    {
        facingDirection = jumpDirection;

        if (flipSprite && spriteRenderer != null)
        {
            spriteRenderer.flipX = facingDirection < 0;
        }

        wallJumpBlockedMoveDirection = -jumpDirection;
        wallJumpInputLockUntil = Time.time + wallJumpInputLockDuration;
        rb.linearVelocity = new Vector2(jumpDirection * wallJumpForce.x, wallJumpForce.y);

        if (IsPressingAwayFromWall(jumpDirection))
        {
            PlayAirJumpSpin();
            ClearPendingWallJumpSpin();
            return;
        }

        pendingWallJumpSpinDirection = jumpDirection;
        pendingWallJumpSpinUntil = Time.time + wallJumpSpinInputWindow;
    }

    private bool CanUseCoyoteJump()
    {
        return Time.time - lastGroundedTime <= coyoteTime;
    }

    private bool CanUseWallCoyoteJump()
    {
        return lastWallJumpDirection != 0 && Time.time - lastWallTouchTime <= wallCoyoteTime;
    }

    private bool IsPressingTowardWall()
    {
        return wallJumpDirection > 0 && moveInput < -0.01f ||
            wallJumpDirection < 0 && moveInput > 0.01f;
    }

    private bool IsPressingAwayFromWall()
    {
        return IsPressingAwayFromWall(wallJumpDirection);
    }

    private bool IsPressingAwayFromWall(int jumpDirection)
    {
        return jumpDirection > 0 && moveInput > 0.01f ||
            jumpDirection < 0 && moveInput < -0.01f;
    }

    private void HandlePendingWallJumpSpin()
    {
        if (pendingWallJumpSpinDirection == 0)
        {
            return;
        }

        if (Time.time > pendingWallJumpSpinUntil)
        {
            ClearPendingWallJumpSpin();
            return;
        }

        if (pendingWallJumpSpinDirection > 0 && moveInput <= 0.01f ||
            pendingWallJumpSpinDirection < 0 && moveInput >= -0.01f)
        {
            return;
        }

        facingDirection = pendingWallJumpSpinDirection;

        if (flipSprite && spriteRenderer != null)
        {
            spriteRenderer.flipX = facingDirection < 0;
        }

        PlayAirJumpSpin();
        ClearPendingWallJumpSpin();
    }

    private void ClearPendingWallJumpSpin()
    {
        pendingWallJumpSpinDirection = 0;
        pendingWallJumpSpinUntil = float.NegativeInfinity;
    }

    private void HandleVariableJumpHeight()
    {
        if (!jumpReleased || rb.linearVelocity.y <= 0f)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    private void UpdateAnimation()
    {
        bool hasMoveInput = Mathf.Abs(moveInput) > 0.01f;
        bool isMoving = isGrounded && hasMoveInput;

        if (animator != null)
        {
            animator.speed = isGrounded ? animatorBaseSpeed : 0f;
            animator.SetBool("IsMoving", isMoving);
        }

        if (hasMoveInput)
        {
            UpdateFacingFromMoveInput();
        }

        if (flipSprite && spriteRenderer != null && hasMoveInput)
        {
            spriteRenderer.flipX = moveInput < 0f;
        }
    }

    private void UpdateFacingFromMoveInput()
    {
        if (Mathf.Abs(moveInput) <= 0.01f)
        {
            return;
        }

        facingDirection = moveInput < 0f ? -1 : 1;
    }

    private void PlayAirJumpSpin()
    {
        if (!spinOnAirJump || !CanAnimateVisualRoot())
        {
            return;
        }

        if (airJumpSpinRoutine != null)
        {
            StopCoroutine(airJumpSpinRoutine);
        }

        airJumpSpinRoutine = StartCoroutine(AnimateAirJumpSpin());
    }

    private System.Collections.IEnumerator AnimateAirJumpSpin()
    {
        if (airJumpSpinDuration <= 0f)
        {
            visualRoot.localRotation = visualBaseRotation;
            airJumpSpinRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        float spinDirection = facingDirection >= 0 ? -1f : 1f;

        while (elapsed < airJumpSpinDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / airJumpSpinDuration);
            float angle = 360f * spinDirection * progress;
            visualRoot.localRotation = visualBaseRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        visualRoot.localRotation = visualBaseRotation;
        airJumpSpinRoutine = null;
    }

    private void PlayLandingSquash(float fallSpeed)
    {
        if (!CanAnimateVisualRoot())
        {
            return;
        }

        if (landingSquashRoutine != null)
        {
            StopCoroutine(landingSquashRoutine);
        }

        float squashAmount = Mathf.InverseLerp(minLandingSquashSpeed, maxLandingSquashSpeed, fallSpeed);
        currentLandingSquashScale = Vector2.Lerp(landingSquashScale, maxLandingSquashScale, squashAmount);
        landingSquashRoutine = StartCoroutine(AnimateLandingSquash());
    }

    private bool CanAnimateVisualRoot()
    {
        return visualRoot != null && visualRoot != transform;
    }

    private System.Collections.IEnumerator AnimateLandingSquash()
    {
        Vector3 squashScale = new Vector3(
            visualBaseScale.x * currentLandingSquashScale.x,
            visualBaseScale.y * currentLandingSquashScale.y,
            visualBaseScale.z
        );
        Vector3 squashPosition = GetBottomAnchoredVisualPosition(squashScale);

        yield return ScaleVisual(visualRoot.localScale, squashScale, visualRoot.localPosition, squashPosition, landingSquashDuration);
        yield return ScaleVisual(visualRoot.localScale, visualBaseScale, visualRoot.localPosition, visualBaseLocalPosition, landingRecoverDuration);

        landingSquashRoutine = null;
    }

    private Vector3 GetBottomAnchoredVisualPosition(Vector3 scale)
    {
        float baseHeight = visualHeight * visualBaseScale.y;
        float scaledHeight = visualHeight * scale.y;
        float yOffset = (baseHeight - scaledHeight) * 0.5f;

        return visualBaseLocalPosition + Vector3.down * yOffset;
    }

    private System.Collections.IEnumerator ScaleVisual(
        Vector3 fromScale,
        Vector3 toScale,
        Vector3 fromPosition,
        Vector3 toPosition,
        float duration)
    {
        if (duration <= 0f)
        {
            visualRoot.localScale = toScale;
            visualRoot.localPosition = toPosition;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            visualRoot.localScale = Vector3.Lerp(fromScale, toScale, progress);
            visualRoot.localPosition = Vector3.Lerp(fromPosition, toPosition, progress);
            yield return null;
        }

        visualRoot.localScale = toScale;
        visualRoot.localPosition = toPosition;
    }

    private void OnDrawGizmosSelected()
    {
        DrawColliderGizmo(groundCheckCollider, Color.yellow);
        DrawColliderGizmo(leftWallCheckCollider, Color.cyan);
        DrawColliderGizmo(rightWallCheckCollider, Color.cyan);
    }

    private void DrawColliderGizmo(BoxCollider2D targetCollider, Color color)
    {
        if (targetCollider == null)
        {
            return;
        }

        Gizmos.color = color;

        Bounds bounds = targetCollider.bounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
