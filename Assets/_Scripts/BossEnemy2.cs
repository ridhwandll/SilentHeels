using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossEnemy2 : MonoBehaviour, IHealth
{
    [Header("Boss Core Stats")]
    public int maxHealth = 60;
    public float moveSpeed = 6f;
    public float aggroRange = 16f;
    public Transform attackPoint;
    public float attackCooldown = 0.5f;
    public int attackDamage = 10;

    private float _attackTimer = 0f;

    [Header("Melee Settings")]
    public float meleeAttackRange = 1.5f; // change later
    public float meleeHitRadius = 0.5f;   // change 
    public LayerMask playerLayer;

    [Header("Ranged (Burst Fire)")]
    public float rangedAttackRange = 10f; // pore change
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public int projectilesPerBurst = 3;
    public float timeBetweenBurstShots = 0.15f;

    [Header("Boss Mobility")]
    public float jumpForce = 60f;
    public float dashForce = 80f;
    public float dashDuration = 0.2f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform _player;
    private CameraShake _mainCameraShaker;

    private int _currentHealth;
    private int _facingDirection = 1;
    private int _damageSinceLastAbility = 0;

    private bool _isMoving;
    private bool _isGrounded;
    private bool _isDashing;
    private bool _isBursting;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        _currentHealth = maxHealth;
        _attackTimer = attackCooldown;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (camObj != null)
            _mainCameraShaker = camObj.GetComponent<CameraShake>();
    }

    void Update()
    {
        
        if (_isDashing || _isBursting || _player == null)
        {
            UpdateAnimations();
            return;
        }

        _attackTimer -= Time.deltaTime;
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        
        if (distanceToPlayer <= aggroRange)
        {
            FacePlayer();

            
            if (distanceToPlayer <= meleeAttackRange)
            {
                StopMoving();
                if (_attackTimer <= 0f)
                    ExecuteMeleeAttack();
            }
            
            else if (distanceToPlayer <= rangedAttackRange)
            {
                StopMoving();
                if (_attackTimer <= 0f)
                    StartCoroutine(RangedBurstRoutine());
            }
            
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            StopMoving(); // Player is outside Aggro Range
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

    private void ExecuteMeleeAttack()
    {
        _attackTimer = attackCooldown;

        if (_anim != null)
            _anim.SetTrigger("Attack");

        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, meleeHitRadius, playerLayer);
        if (hitPlayer != null)
        {
            PlayerCombat playerStats = hitPlayer.GetComponent<PlayerCombat>();
            if (playerStats != null)
                playerStats.TakeDamage(attackDamage);
        }
    }

    private IEnumerator RangedBurstRoutine()
    {
        _isBursting = true;

        // Trigger attack animation at the start of the burst
        if (_anim != null)
            _anim.SetTrigger("Attack");

        for (int i = 0; i < projectilesPerBurst; i++)
        {
            FacePlayer();

            if (projectilePrefab != null)
            {
                GameObject proj = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
                Projectile projectileScript = proj.GetComponent<Projectile>();

                if (projectileScript != null)
                {
                    projectileScript.Setup(new Vector2(_facingDirection, 0f), attackDamage, projectileSpeed);
                }

                Destroy(proj, 10.0f);
            }

            yield return new WaitForSeconds(timeBetweenBurstShots);
        }

        _isBursting = false;
        _attackTimer = attackCooldown; // Reset cooldown after the burst is fully complete
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
        _isDashing = true;
        _rb.linearVelocity = new Vector2(0, 0);
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.3f);

        float originalGravity = _rb.gravityScale;
        _rb.gravityScale = 0.5f;

        FacePlayer();

        _rb.linearVelocity = new Vector2(_facingDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        _rb.gravityScale = originalGravity;
        _isDashing = false;
    }

    void OnDrawGizmosSelected()
    {
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);

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