using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Base Movement Stats")]
    public float maxSpeed = 10f;
    public float acceleration = 50f;
    public float decceleration = 50f;
    public float frictionAmount = 0.2f;

    [Header("Base Jump Stats")]
    public float jumpForce = 15f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Jump Limits")]
    [Tooltip("1 = Single Jump, 2 = Double Jump, 3 = Triple Jump, etc.")]
    public int maxJumps = 1;

    [Header("Feel Helpers")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.8f;

    [Header("Abilities")]
    public float dashForce = 25f;
    public float dashDuration = 0.2f;
    public float groundPoundForce = 30f;

    [Header("Unlocked Abilities")]
    public bool canGroundPound = false; // Pickups

    [Header("Layers & Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float lastFacingDirection = 1f;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isDashing;
    private bool isGroundPounding;

    // Tracks how many times the player has jumped in their current sequence
    private int currentJumps = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDashing || isGroundPounding)
            return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.x != 0)
            lastFacingDirection = Mathf.Sign(moveInput.x);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            currentJumps = 0;
        }
        else
            coyoteTimeCounter -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            ExecuteJump();
            currentJumps = 1;
        }
        // Mid-Air Jumps
        else if (Input.GetKeyDown(KeyCode.Space) && !isGrounded)
        {
            if (coyoteTimeCounter <= 0f && currentJumps == 0)
                currentJumps = 1;

            if (currentJumps < maxJumps)
            {
                ExecuteJump();
                currentJumps++;
            }
        }

        HandleAbilities();
    }

    void FixedUpdate()
    {
        if (isDashing || isGroundPounding) return;

        Run();
        ModifyFallPhysics();
    }

    private void Run()
    {
        float targetSpeed = moveInput.x * maxSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 0.9f) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);

        if (Mathf.Abs(moveInput.x) < 0.01f && isGrounded)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.linearVelocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(rb.linearVelocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
    }

    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
    }

    private void ModifyFallPhysics()
    {
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    private void HandleAbilities()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            StartCoroutine(DashCoroutine());

        if (moveInput.y < -0.1f && !isGrounded && canGroundPound)
            StartCoroutine(GroundPoundCoroutine());
    }

    private System.Collections.IEnumerator DashCoroutine()
    {
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(lastFacingDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    private System.Collections.IEnumerator GroundPoundCoroutine()
    {
        isGroundPounding = true;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);

        rb.linearVelocity = new Vector2(0, -groundPoundForce);

        while (!isGrounded)
        {
            yield return null;
        }

        isGroundPounding = false;
    }

    public void UpgradeStats(float speedBoost, float jumpBoost, int extraJumps, bool unlockGroundPound)
    {
        maxSpeed += speedBoost;
        jumpForce += jumpBoost;
        maxJumps += extraJumps;

        if (unlockGroundPound)
            canGroundPound = true;

        Debug.Log($"Upgraded! Speed: {maxSpeed}. Max Jumps is now {maxJumps}.");
    }
}