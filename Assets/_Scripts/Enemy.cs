using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IHealth
{
    public enum EnemyType { Melee, Ranged }

    [Header("Enemy Setup")]
    public EnemyType currentType;
    public int maxHealth = 3;
    public float moveSpeed = 3f;
    public float aggroRange = 8f;
    public Transform attackPoint;

    [Header("Attack Settings")]
    public float attackCooldown = 1f;
    public int attackDamage = 5;
    private float _attackTimer = 0f;

    [Header("Melee")]
    public float meleeAttackRange = 1.0f; // Added separate range for Melee
    public float meleeHitRadius = 0.5f;
    public LayerMask playerLayer;

    [Header("Ranged")]
    public GameObject projectilePrefab;
    public float rangedAttackRange = 5.0f; // Renamed for clarity
    public float projectileSpeed = 10f;

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

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        _currentHealth = maxHealth;
        _attackTimer = attackCooldown;

        // Added null checks here just in case the player or camera is missing at the start
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (camObj != null)
            _mainCameraShaker = camObj.GetComponent<CameraShake>();
    }

    void Update()
    {
        // 1. FATAL CRASH FIX: Stop logic if the player is dead/destroyed
        if (_player == null)
        {
            StopMoving();
            UpdateAnimations();
            return;
        }

        _attackTimer -= Time.deltaTime;
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        // 2. PHANTOM MELEE FIX: Check the correct range based on the enemy type
        float currentAttackRange = (currentType == EnemyType.Melee) ? meleeAttackRange : rangedAttackRange;

        if (distanceToPlayer <= aggroRange)
        {
            FacePlayer();
            if (distanceToPlayer > currentAttackRange)
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
        {
            _anim.SetTrigger("Attack");
        }

        if (currentType == EnemyType.Melee)
        {
            Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, meleeHitRadius, playerLayer);

            if (hitPlayer != null)
            {
                PlayerCombat playerStats = hitPlayer.GetComponent<PlayerCombat>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(attackDamage);
                }
            }
        }
        else if (currentType == EnemyType.Ranged)
        {
            GameObject proj = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
            Projectile projectileScript = proj.GetComponent<Projectile>();

            if (projectileScript != null)
            {
                // 3. COMPILE ERROR FIX: Changed 'rangedDamage' to 'attackDamage'
                projectileScript.Setup(new Vector2(_facingDirection, 0f), attackDamage, projectileSpeed, true);
            }

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
        }
    }

    private void Die()
    {
        Destroy(gameObject);
        if (_mainCameraShaker != null)
            _mainCameraShaker.Shake();
    }

    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => maxHealth;

    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - amount);

        if (_currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
    }

    void OnDrawGizmosSelected()
    {
        // Draw Aggro Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Draw correct attack range based on type
        Gizmos.color = (currentType == EnemyType.Melee) ? Color.red : Color.blue;
        float drawRange = (currentType == EnemyType.Melee) ? meleeAttackRange : rangedAttackRange;
        Gizmos.DrawWireSphere(transform.position, drawRange);

        if (attackPoint != null && currentType == EnemyType.Melee)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, meleeHitRadius);
        }

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}