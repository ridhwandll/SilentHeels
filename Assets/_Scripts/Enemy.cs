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
    public float attackCooldown = 1.5f;
    public int attackDamage = 5;
    private float _attackTimer = 0f;

    [Header("Melee")]
    public float meleeHitRadius = 0.5f;
    public LayerMask playerLayer;

    [Header("Ranged")]
    public GameObject projectilePrefab;
    public float attackRange = 1.5f;
    public float projectileSpeed = 10f;

    private Rigidbody2D _rb;
    private Transform _player;
    private int _currentHealth;
    private int _facingDirection = 1;
    private CameraShake _mainCameraShaker;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentHealth = maxHealth;
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _mainCameraShaker = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>();
    }

    void Update()
    {
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
            StopMoving();
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
            _facingDirection = 1; // Face Right
        else if (_player.position.x < transform.position.x)
            _facingDirection = -1; // Face Left

        //if (_facingDirection == 1)
        //    Debug.LogWarning($"Enemy is facing right");
        //else
        //    Debug.LogWarning($"Enemy is facing left");

        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * _facingDirection;
        transform.localScale = currentScale;
    }

    private void ExecuteAttack()
    {
        _attackTimer = attackCooldown;

        if (currentType == EnemyType.Melee)
        {
            Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, meleeHitRadius, playerLayer);
            if (hitPlayer != null)
                hitPlayer.GetComponent<PlayerCombat>().TakeDamage(attackDamage);
        }
        else if (currentType == EnemyType.Ranged)
        {
            GameObject proj = Instantiate(projectilePrefab, attackPoint.position, attackPoint.rotation);
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();

            if (projRb != null)
                projRb.linearVelocity = new Vector2(_facingDirection * projectileSpeed, 0);

            Destroy(proj, 10.0f);
        }
    }

    private void Die()
    {
        Destroy(gameObject);
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
        Gizmos.color = Color.greenYellow;
        Gizmos.DrawWireSphere(attackPoint.position, meleeHitRadius);
    }
}
