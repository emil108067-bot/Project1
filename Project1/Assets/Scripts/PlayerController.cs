using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public float crouchSpeedMultiplier = 0.4f;

    [Header("Slide")]
    public float slideFriction = 15f;
    public float minSlideSpeed = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Animation")]
    public Animator animator;
    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private PlayerControls controls;

    private float moveInput;
    private bool jumpPressed;
    private bool crouchHeld;
    private bool isGrounded;
    private bool isCrouching;
    private bool isSliding;
    private float slideDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        controls = new PlayerControls();
        controls.Player.Jump.performed += ctx => jumpPressed = true;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    void Update()
    {
        moveInput = controls.Player.Move.ReadValue<float>();

        if (moveInput > 0.01f)
            spriteRenderer.flipX = false; // facing right (adjust if your art faces left by default)
        else if (moveInput < -0.01f)
            spriteRenderer.flipX = true;

        crouchHeld = controls.Player.Crouch.IsPressed();

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool hasHorizontalMomentum = Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (crouchHeld && hasHorizontalMomentum && !isSliding && isGrounded)
        {
            isSliding = true;
            slideDirection = Mathf.Sign(rb.linearVelocity.x);
        }

        if (!crouchHeld)
        {
            isSliding = false;
        }

        isCrouching = crouchHeld && !isSliding;

        float facingDirection = isSliding ? slideDirection : moveInput;
        if (facingDirection > 0.01f) spriteRenderer.flipX = false;
        else if (facingDirection < -0.01f) spriteRenderer.flipX = true;

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isSliding)
        {
            float newSpeed = Mathf.Max(Mathf.Abs(rb.linearVelocity.x) - slideFriction * Time.fixedDeltaTime, 0f);
            rb.linearVelocity = new Vector2(slideDirection * newSpeed, rb.linearVelocity.y);

            if (newSpeed <= minSlideSpeed)
            {
                isSliding = false;
            }
        }
        else
        {
            float targetSpeed = moveInput * moveSpeed;
            if (isCrouching) targetSpeed *= crouchSpeedMultiplier;

            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }

        if (jumpPressed && isGrounded && !isCrouching && !isSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        jumpPressed = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}