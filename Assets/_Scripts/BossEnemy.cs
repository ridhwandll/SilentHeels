using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossEnemy : MonoBehaviour, IHealth
{
    public enum BossState { Intro, Chasing, Attacking, Evading, Transitioning, Dead }
    public enum BossPhase { Phase1_Melee, Phase2_Ranged }

    [Header("Boss Core Stats")]
    public int maxHealth = 100;
    public float moveSpeed = 4f;
    public float aggroRange = 20f;

    [Header("Phase Dynamics")]
    public float phase1AttackRange = 2.5f;
    public float phase2AttackRange = 12f;
    public float enragedSpeedMultiplier = 1.5f;
    public float enragedCooldownMultiplier = 0.6f;

    private BossPhase _currentPhase = BossPhase.Phase1_Melee;
    private bool _isEnraged = false;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float baseAttackCooldown = 1.5f;
    public int attackDamage = 15;

    [Header("Melee Specifics")]
    public float meleeHitRadius = 1f;
    public LayerMask playerLayer;

    [Header("Ranged Specifics")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;

    [Header("Boss Mobility & Evasion")]
    public float jumpForce = 40f;
    public float dashForce = 60f;
    public float dashDuration = 0.3f;
    public int damageThresholdForEvasion = 25;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // State Variables
    private BossState _currentState = BossState.Intro;
    private float _currentAttackRange;
    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform _player;
    private CameraShake _mainCameraShaker;

    private int _currentHealth;
    private int _facingDirection = 1;
    private int _damageSinceLastEvasion = 0;
    private float _lastAttackTime = 0f;
    private bool _isGrounded;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        _currentHealth = maxHealth;
        _currentAttackRange = phase1AttackRange; // Start in Phase 1 range

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (camObj != null) _mainCameraShaker = camObj.GetComponent<CameraShake>();

        TransitionToState(BossState.Chasing);
    }

    void Update()
    {
        if (_currentState == BossState.Dead || _player == null)
            return;

        UpdateAnimations();

        // Only check for phase transitions if we aren't currently transitioning or dead
        if (_currentState != BossState.Transitioning)
        {
            CheckPhase();
        }

        // State Machine Logic
        switch (_currentState)
        {
            case BossState.Chasing:
                HandleChasing();
                break;
            case BossState.Attacking:
            case BossState.Evading:
            case BossState.Transitioning:
                // Handled via Coroutines
                break;
        }
    }

    private void TransitionToState(BossState newState)
    {
        if (_currentState == BossState.Dead)
            return;

        _currentState = newState;
    }

    private void HandleChasing()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        FacePlayer();

        if (distanceToPlayer > _currentAttackRange)
        {
            float currentSpeed = _isEnraged ? moveSpeed * enragedSpeedMultiplier : moveSpeed;
            _rb.linearVelocity = new Vector2(_facingDirection * currentSpeed, _rb.linearVelocity.y);
        }
        else
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);

            float currentCooldown = _isEnraged ? baseAttackCooldown * enragedCooldownMultiplier : baseAttackCooldown;
            if (Time.time >= _lastAttackTime + currentCooldown)
            {
                StartCoroutine(AttackRoutine());
            }
        }
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

    private IEnumerator AttackRoutine()
    {
        TransitionToState(BossState.Attacking);
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);

        if (_anim != null) _anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.3f);

        if (_currentPhase == BossPhase.Phase1_Melee)
        {
            Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, meleeHitRadius, playerLayer);
            if (hitPlayer != null)
            {
                PlayerCombat playerStats = hitPlayer.GetComponent<PlayerCombat>();
                if (playerStats != null) playerStats.TakeDamage(attackDamage);
            }
        }
        else if (_currentPhase == BossPhase.Phase2_Ranged && projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, attackPoint.position, attackPoint.rotation);
            proj.GetComponent<Projectile>().Setup(new Vector2(_facingDirection, 0), attackDamage, projectileSpeed, true);
            Destroy(proj, 5.0f);
        }

        yield return new WaitForSeconds(0.5f);

        _lastAttackTime = Time.time;
        TransitionToState(BossState.Chasing);
    }

    private IEnumerator EvasionRoutine()
    {
        TransitionToState(BossState.Evading);

        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(new Vector2(-_facingDirection * dashForce, jumpForce), ForceMode2D.Impulse);

        yield return new WaitForSeconds(dashDuration);

        yield return new WaitUntil(() => _isGrounded);

        _damageSinceLastEvasion = 0;
        TransitionToState(BossState.Chasing);
    }

    private void CheckPhase()
    {
        if (!_isEnraged && _currentHealth <= maxHealth / 2)
            StartCoroutine(PhaseTransitionRoutine());
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        TransitionToState(BossState.Transitioning);
        _rb.linearVelocity = Vector2.zero;

        _isEnraged = true;
        _currentPhase = BossPhase.Phase2_Ranged;
        _currentAttackRange = phase2AttackRange; // Expand detection range for projectiles

        if (_anim != null) _anim.SetTrigger("Enrage");

        if (_mainCameraShaker != null) _mainCameraShaker.Shake();

        yield return new WaitForSeconds(1.5f);

        _damageSinceLastEvasion = 0;
        TransitionToState(BossState.Chasing);
    }

    public void TakeDamage(int amount)
    {
        if (_currentState == BossState.Dead)
            return;

        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        _damageSinceLastEvasion += amount;

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        if (_damageSinceLastEvasion >= damageThresholdForEvasion && _currentState == BossState.Chasing)
        {
            StartCoroutine(EvasionRoutine());
        }
    }

    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => maxHealth;
    public void Heal(int amount) => _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);

    private void Die()
    {
        TransitionToState(BossState.Dead);
        _rb.linearVelocity = Vector2.zero;

        if (_anim != null) _anim.SetTrigger("Die");
        if (_mainCameraShaker != null) _mainCameraShaker.Shake();

        GetComponent<Collider2D>().enabled = false;
        _rb.bodyType = RigidbodyType2D.Kinematic;

        Destroy(gameObject, 3f);
    }

    private void UpdateAnimations()
    {
        if (groundCheckPoint != null)
            _isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        else
            _isGrounded = true;

        if (_anim != null)
        {
            _anim.SetBool("IsMoving", _currentState == BossState.Chasing && Mathf.Abs(_rb.linearVelocity.x) > 0.1f);
            _anim.SetBool("IsGrounded", _isGrounded);
            _anim.SetBool("IsJumping", !_isGrounded && _rb.linearVelocity.y > 0.1f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, meleeHitRadius);
        }
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}