using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Stats")]
    public float MaxSpeed = 10f;
    public float Acceleration = 50f;
    public float Decceleration = 50f;
    public float FrictionAmount = 0.2f;

    [Header("Jump Stats")]
    public float JumpForce = 15f;

    [Header("Abilities")]
    public float DashForce = 25f;

    [Header("Layers")]
    public Transform GroundCheck;
    public float GroundCheckRadius = 0.2f;
    public LayerMask GroundLayer;

    private Rigidbody2D _Rb;
    private Animator _Anim;
    private Vector2 _MoveInput;
    private float _LastFacingDirection = 1f;

    // Core State Variables
    private bool _IsGrounded;
    private bool _IsDashing;
    private bool _IsJumping;
    private bool _IsMoving;
    private int _CurrentJumps = 0;

    public float GetFacingDirection()
    {
        return _LastFacingDirection;
    }

    void Start()
    {
        _Rb = GetComponent<Rigidbody2D>();
        _Anim = GetComponentInChildren<Animator>();

        if (_Anim == null)
            Debug.Log("Animation is null!");
    }

    void Update()
    {
        // Instead of returning early, we just bypass inputs/jumps if dashing
        // This keeps our animation updates running cleanly every frame
        if (!_IsDashing)
        {
            _MoveInput.x = Input.GetAxisRaw("Horizontal");
            _MoveInput.y = Input.GetAxisRaw("Vertical");

            // FLIP
            if (_MoveInput.x != 0)
            {
                _LastFacingDirection = Mathf.Sign(_MoveInput.x);
                Vector3 currentScale = transform.localScale;
                currentScale.x = Mathf.Abs(currentScale.x) * _LastFacingDirection;
                transform.localScale = currentScale;
            }

            HandleJump();
            HandleAbilities();
        }
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (_IsDashing)
            return;

        Run();
    }

    private void HandleJump()
    {
        _IsGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundCheckRadius, GroundLayer);

        if (_Rb.linearVelocity.y <= 0f)
            _IsJumping = false;

        if (_IsGrounded && !_IsJumping)
            _CurrentJumps = 0;

        // If the player walks off a ledge without jumping, consume their first jump
        else if (!_IsGrounded && _CurrentJumps == 0)
            _CurrentJumps = 1;

        int maxJumps = 1 + PlayerData.Instance.Data.ExtraJumps;
        if (Input.GetKeyDown(KeyCode.Space) && _CurrentJumps < maxJumps)
            ExecuteJump();
    }

    private void Run()
    {
        float targetSpeed = _MoveInput.x * MaxSpeed * PlayerData.Instance.Data.MoveSpeedMultiplier;
        float speedDif = targetSpeed - _Rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Acceleration : Decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 0.9f) * Mathf.Sign(speedDif);

        _Rb.AddForce(movement * Vector2.right);

        if (Mathf.Abs(_MoveInput.x) < 0.01f && _IsGrounded)
        {
            float amount = Mathf.Min(Mathf.Abs(_Rb.linearVelocity.x), Mathf.Abs(FrictionAmount));
            amount *= Mathf.Sign(_Rb.linearVelocity.x);
            _Rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
    }

    private void ExecuteJump()
    {
        _Rb.linearVelocity = new Vector2(_Rb.linearVelocity.x, 0);
        _Rb.AddForce(Vector2.up * JumpForce * PlayerData.Instance.Data.JumpForceMultiplier, ForceMode2D.Impulse);
        _IsJumping = true;
        _CurrentJumps++;
    }

    private void HandleAbilities()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && PlayerData.Instance.Data.CanDash)
            StartCoroutine(DashCoroutine());
    }

    private System.Collections.IEnumerator DashCoroutine()
    {
        _IsDashing = true;
        float originalGravity = _Rb.gravityScale;
        _Rb.gravityScale = 0f;

        _Rb.linearVelocity = new Vector2(_LastFacingDirection * DashForce * PlayerData.Instance.Data.DashForceMultiplier, 0f);

        yield return new WaitForSeconds(PlayerData.Instance.Data.DashDuration);

        _Rb.gravityScale = originalGravity;
        _IsDashing = false;
    }

    private void UpdateAnimations()
    {
        _IsMoving = Mathf.Abs(_Rb.linearVelocity.x) > 0.05f;
        _Anim.SetBool("IsMoving", _IsMoving);
        _Anim.SetBool("IsJumping", _IsJumping);
        _Anim.SetBool("IsGrounded", _IsGrounded);
    }
}
