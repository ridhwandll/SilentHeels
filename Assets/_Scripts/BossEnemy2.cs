using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossEnemy2 : MonoBehaviour, IHealth
{
    public enum BossState { Intro, Chasing, Attacking, Blocking, Transitioning, Dead }

    [Header("Boss Core Stats")]
    public int maxHealth = 100;
    public float moveSpeed = 4f;

    [Header("AI Zones (Distances)")]
    public float aggroRange = 16f;
    public float rangedAttackRange = 10f;
    public float meleeAttackRange = 1.5f;

    [Header("Phase Dynamics (Enrage)")]
    public float enragedSpeedMultiplier = 1.5f;
    public float enragedCooldownMultiplier = 0.6f;
    public float enragedSizeMultiplier = 1.35f;
    private bool _isEnraged = false;

    [Header("Melee Attack")]
    public Transform attackPoint;
    public float baseAttackCooldown = 1.5f;
    public int meleeDamage = 15;
    public float meleeHitRadius = 1f;
    public LayerMask playerLayer;

    [Header("Ranged Attack (Burst Fire)")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public int rangedDamage = 10;
    public int projectilesPerBurst = 3;
    public float timeBetweenBurstShots = 0.15f;

    [Header("Block & Knockback Mechanism")]
    public int damageThresholdForBlock = 25;
    public float blockDuration = 0.5f;
    public float playerKnockbackForce = 15f; // We will adjust this later with your Player script

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // State Variables
    private BossState _currentState = BossState.Intro;
    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform _player;
    private CameraShake _mainCameraShaker;

    private int _currentHealth;
    private int _facingDirection = 1;
    private int _damageSinceLastBlock = 0;
    private float _lastAttackTime = 0f;
    private bool _isGrounded;
    private Vector3 _baseScale;

    private bool _hasTriggeredHitAnim = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        _currentHealth = maxHealth;
        _baseScale = transform.localScale;

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

        if (_currentState != BossState.Transitioning)
        {
            CheckPhase();
        }

        switch (_currentState)
        {
            case BossState.Chasing:
                HandleChasing();
                break;
            case BossState.Attacking:
            case BossState.Blocking:
            case BossState.Transitioning:
                break;
        }
    }

    private void TransitionToState(BossState newState)
    {
        if (_currentState == BossState.Dead)
            return;

        _currentState = newState;
    }

    private void ClearAnimatorTriggers()
    {
        if (_anim != null)
        {
            _anim.ResetTrigger("MeleeAttack");
            _anim.ResetTrigger("RangedAttack");
            _anim.ResetTrigger("TakeDamage");
            _anim.ResetTrigger("Block");
            _anim.ResetTrigger("Enrage");
        }
    }

    private void HandleChasing()
    {
        if (_damageSinceLastBlock >= damageThresholdForBlock)
        {
            StartCoroutine(BlockRoutine());
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        // 2. Out of Aggro Range -> Do nothing
        if (distanceToPlayer > aggroRange)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            return;
        }

        // Inside Aggro Range, we must face the player
        FacePlayer();

        float currentCooldown = _isEnraged ? baseAttackCooldown * enragedCooldownMultiplier : baseAttackCooldown;

        // 3. Zone AI Logic
        if (distanceToPlayer <= meleeAttackRange)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y); // Stop moving
            if (Time.time >= _lastAttackTime + currentCooldown)
            {
                StartCoroutine(MeleeAttackRoutine());
            }
        }
        else if (distanceToPlayer <= rangedAttackRange)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y); // Stop moving
            if (Time.time >= _lastAttackTime + currentCooldown)
            {
                StartCoroutine(RangedBurstRoutine());
            }
        }
        else
        {
            // Chase the player
            float currentSpeed = _isEnraged ? moveSpeed * enragedSpeedMultiplier : moveSpeed;
            _rb.linearVelocity = new Vector2(_facingDirection * currentSpeed, _rb.linearVelocity.y);
        }
    }

    private void FacePlayer()
    {
        if (_player.position.x > transform.position.x)
            _facingDirection = 1;
        else if (_player.position.x < transform.position.x)
            _facingDirection = -1;

        float targetSize = _isEnraged ? enragedSizeMultiplier : 1f;
        Vector3 currentScale = _baseScale * targetSize;

        currentScale.x = Mathf.Abs(currentScale.x) * _facingDirection;
        transform.localScale = currentScale;
    }

    private IEnumerator MeleeAttackRoutine()
    {
        TransitionToState(BossState.Attacking);
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        ClearAnimatorTriggers();

        if (_anim != null) _anim.SetTrigger("MeleeAttack");

        yield return new WaitForSeconds(0.3f); // Wind-up time

        float currentHitRadius = meleeHitRadius * (_isEnraged ? enragedSizeMultiplier : 1f);
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, currentHitRadius, playerLayer);

        if (hitPlayer != null)
        {
            PlayerCombat playerStats = hitPlayer.GetComponent<PlayerCombat>();
            if (playerStats != null) playerStats.TakeDamage(meleeDamage);
        }

        yield return new WaitForSeconds(0.5f); // Recovery time

        _lastAttackTime = Time.time;
        TransitionToState(BossState.Chasing);
    }

    private IEnumerator RangedBurstRoutine()
    {
        TransitionToState(BossState.Attacking);
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        ClearAnimatorTriggers();

        if (_anim != null) _anim.SetTrigger("RangedAttack");

        // Fire multiple projectiles
        for (int i = 0; i < projectilesPerBurst; i++)
        {
            FacePlayer(); // Ensure boss aims correctly before each shot

            if (projectilePrefab != null)
            {
                GameObject proj = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
                Projectile projectileScript = proj.GetComponent<Projectile>();

                if (projectileScript != null)
                    projectileScript.Setup(new Vector2(_facingDirection, 0f), rangedDamage, projectileSpeed, true);

                Destroy(proj, 10.0f);
            }

            yield return new WaitForSeconds(timeBetweenBurstShots);
        }

        yield return new WaitForSeconds(0.5f); // Recovery time after burst

        _lastAttackTime = Time.time;
        TransitionToState(BossState.Chasing);
    }

    private IEnumerator BlockRoutine()
    {
        TransitionToState(BossState.Blocking);
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y); // Stop Boss
        ClearAnimatorTriggers();

        if (_anim != null) _anim.SetTrigger("Block");

        // --- NEW KNOCKBACK CALL ---
        if (_player != null)
        {
            PlayerMovement pMovement = _player.GetComponent<PlayerMovement>();
            if (pMovement != null)
            {
                int pushDirection = (_player.position.x > transform.position.x) ? 1 : -1;
                Vector2 knockbackForce = new Vector2(pushDirection * playerKnockbackForce, 5f);

                // Calls the new coroutine we just made on the player! (Push lasts for 0.4 seconds)
                pMovement.Knockback(knockbackForce, 0.4f);
            }
        }
        // ---------------------------

        yield return new WaitForSeconds(blockDuration);

        _damageSinceLastBlock = 0;
        _hasTriggeredHitAnim = false;

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
        ClearAnimatorTriggers();

        if (_anim != null) _anim.SetTrigger("Enrage");
        if (_mainCameraShaker != null) _mainCameraShaker.Shake();

        yield return new WaitForSeconds(1.5f);

        _damageSinceLastBlock = 0;
        _hasTriggeredHitAnim = false;
        TransitionToState(BossState.Chasing);
    }

    public void TakeDamage(int amount)
    {
        if (_currentState == BossState.Dead)
            return;

        if (!_hasTriggeredHitAnim && amount > 0)
        {
            if (_anim != null) _anim.SetTrigger("TakeDamage");
            _hasTriggeredHitAnim = true;
        }

        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        _damageSinceLastBlock += amount;

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        // Trigger Block if threshold is reached while chasing
        if (_damageSinceLastBlock >= damageThresholdForBlock && _currentState == BossState.Chasing)
            StartCoroutine(BlockRoutine());
    }

    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => maxHealth;
    public void Heal(int amount) => _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);

    private void Die()
    {
        TransitionToState(BossState.Dead);
        _rb.linearVelocity = Vector2.zero;

        ClearAnimatorTriggers();

        if (_anim != null) _anim.SetTrigger("Die");
        if (_mainCameraShaker != null) _mainCameraShaker.Shake();

        GetComponent<Collider2D>().enabled = false;
        _rb.bodyType = RigidbodyType2D.Kinematic;

        Destroy(gameObject, 2.1f);
    }

    private void UpdateAnimations()
    {
        if (groundCheckPoint != null)
            _isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        else
            _isGrounded = true;

        if (_anim != null)
        {
            bool isMovingX = Mathf.Abs(_rb.linearVelocity.x) > 0.1f;

            _anim.SetBool("IsRunning", isMovingX && _isGrounded);
            _anim.SetBool("IsMoving", _isGrounded);

            _anim.SetBool("IsJumping", !_isGrounded && _rb.linearVelocity.y > 0.1f);
            _anim.SetBool("IsFalling", !_isGrounded && _rb.linearVelocity.y < -0.1f);
        }
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
            Gizmos.color = Color.green;
            float currentHitRadius = meleeHitRadius * (Application.isPlaying && _isEnraged ? enragedSizeMultiplier : 1f);
            Gizmos.DrawWireSphere(attackPoint.position, currentHitRadius);
        }
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.magenta; // Changed to magenta so it doesn't blend with melee red
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}