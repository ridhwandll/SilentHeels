using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossEnemy : MonoBehaviour, IHealth
{
    public enum EnemyType { Melee, Ranged }

    [Header("Boss Setup (Stats Scaled)")]
    public EnemyType currentType;
    public int maxHealth = 60;
    public float moveSpeed = 6f;
    public float aggroRange = 16f;
    public Transform attackPoint;

    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;
    public int attackDamage = 10;
    private float _attackTimer = 0f;

    [Header("Melee")]
    public float meleeHitRadius = 0.5f;
    public LayerMask playerLayer;

    [Header("Ranged")]
    public GameObject projectilePrefab;
    public float attackRange = 1.5f;
    public float projectileSpeed = 10f;

    [Header("Boss Mobility")]
    public float jumpForce = 15f;
    public float dashForce = 25f;
    public float dashDuration = 0.2f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform _player;
    private int _currentHealth;
    private int _facingDirection = 1;
    private CameraShake _mainCameraShaker;

    private bool _isMoving;
    private bool _isGrounded;
    private bool _isDashing;

    private int _damageSinceLastAbility = 0;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentHealth = maxHealth;
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (camObj != null)
            _mainCameraShaker = camObj.GetComponent<CameraShake>();

        _anim = GetComponentInChildren<Animator>();
        _attackTimer = attackCooldown;
    }

    void Update()
    {
        if (_isDashing)
        {
            UpdateAnimations();
            return;
        }

        _attackTimer -= Time.deltaTime;
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer <= aggroRange)
        {
            FacePlayer();
            if (distanceToPlayer > attackRange)
                ChasePlayer();
            else
            {
                StopMoving();
                if (_attackTimer <= 0f)
                    ExecuteAttack();
            }
        }
        else
        {
            StopMoving();
        }

        UpdateAnimations();
    }

    private void ChasePlayer()
    {
        _rb.linearVelocity = new Vector2(_facingDirection * moveSpeed, _rb.linearVelocity.y);
    }

    private void StopMoving()
    {
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
    }

    private void FacePlayer()
    {
        if (_player.position.x > transform.position.x)
            _facingDirection = 1;
        else if (_player.position.x < transform.position.x)
            _facingDirection = -1;

        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * _facingDirection;
        transform.localScale = currentScale;
    }

    private void ExecuteAttack()
    {
        _attackTimer = attackCooldown;

        if (_anim != null)
            _anim.SetTrigger("Attack");

        if (currentType == EnemyType.Melee)
        {
            Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, meleeHitRadius, playerLayer);
            if (hitPlayer != null)
            {
                PlayerCombat playerStats = hitPlayer.GetComponent<PlayerCombat>();
                if (playerStats != null)
                    playerStats.TakeDamage(attackDamage);
            }
        }
        else if (currentType == EnemyType.Ranged && projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, attackPoint.position, attackPoint.rotation);
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();

            if (projRb != null)
                projRb.linearVelocity = new Vector2(_facingDirection * projectileSpeed, 0);

            Destroy(proj, 10.0f);
        }
    }

    private void UpdateAnimations()
    {
        _isMoving = Mathf.Abs(_rb.linearVelocity.x) > 0.05f;

        if (groundCheckPoint != null)
            _isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        else
            _isGrounded = true;

        if (_anim != null)
        {
            _anim.SetBool("IsMoving", _isMoving);
            _anim.SetBool("IsGrounded", _isGrounded);
            _anim.SetBool("IsJumping", !_isGrounded && _rb.linearVelocity.y > 0.1f);
        }
    }

    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => maxHealth;

    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        _damageSinceLastAbility += amount;

        if (_damageSinceLastAbility >= 15 && !_isDashing)
        {
            _damageSinceLastAbility = 0;
            StartCoroutine(EvasionRoutine());
        }

        if (_currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
    }

    private void Die()
    {
        Destroy(gameObject);
        if (_mainCameraShaker != null)
            _mainCameraShaker.Shake();
    }

    private IEnumerator EvasionRoutine()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0);
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.15f);

        _isDashing = true;
        float originalGravity = _rb.gravityScale;
        _rb.gravityScale = 0f;

        FacePlayer();

        _rb.linearVelocity = new Vector2(_facingDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        _rb.gravityScale = originalGravity;
        _isDashing = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.greenYellow;
            Gizmos.DrawWireSphere(attackPoint.position, meleeHitRadius);
        }

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}