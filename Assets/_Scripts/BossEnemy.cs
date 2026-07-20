using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class BossEnemy : MonoBehaviour, IHealth
{
    public enum BossState { Intro, Chasing, Attacking, Evading, Transitioning, Dead }
    public SpriteRenderer animatorSpriteRenderer;

    [Header("Boss Core Stats")]
    public int maxHealth = 100;
    public float moveSpeed = 4f;
    public float aggroRange = 20f;
    public Slider bossHealthBar;
    public TMP_Text bossHealthText;

    [Header("Phase Dynamics")]
    public float normalAttackRange = 2.5f;
    public float enragedAttackRange = 3f;
    public float enragedSpeedMultiplier = 1.5f;
    public float enragedCooldownMultiplier = 0.6f;
    public float enragedSizeMultiplier = 1.35f;

    private bool _isEnraged = false;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float baseAttackCooldown = 1.5f;
    public int attackDamage = 15;
    public float meleeHitRadius = 1f;
    public LayerMask playerLayer;

    [Header("Boss Mobility & Evasion")]
    public float dashSpeed = 25f;
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
    private Vector3 _baseScale;

    private bool _hasTriggeredHitAnim = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        _currentHealth = maxHealth;
        _currentAttackRange = normalAttackRange;
        _baseScale = transform.localScale;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (camObj != null) _mainCameraShaker = camObj.GetComponent<CameraShake>();

        TransitionToState(BossState.Chasing);
        UpdateBossHealthBar();
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
            case BossState.Evading:
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
            _anim.ResetTrigger("FirstAttack");
            _anim.ResetTrigger("SecondAttack");
            _anim.ResetTrigger("TakeDamage");
            _anim.ResetTrigger("Dash");
            _anim.ResetTrigger("Enrage");
        }
    }

    private void HandleChasing()
    {
        if (_damageSinceLastEvasion >= damageThresholdForEvasion)
        {
            StartCoroutine(EvasionRoutine());
            return;
        }

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

        float targetSize = _isEnraged ? enragedSizeMultiplier : 1f;
        Vector3 currentScale = _baseScale * targetSize;

        currentScale.x = Mathf.Abs(currentScale.x) * _facingDirection;
        transform.localScale = currentScale;
    }

    private IEnumerator AttackRoutine()
    {
        TransitionToState(BossState.Attacking);
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);

        ClearAnimatorTriggers(); // Prevent old hits/dashes from interrupting

        if (_anim != null)
        {
            if (!_isEnraged)
                _anim.SetTrigger("FirstAttack");
            else
                _anim.SetTrigger("SecondAttack");
        }

        yield return new WaitForSeconds(0.3f);

        float currentHitRadius = meleeHitRadius * (_isEnraged ? enragedSizeMultiplier : 1f);

        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, currentHitRadius, playerLayer);
        if (hitPlayer != null)
        {
            PlayerCombat playerStats = hitPlayer.GetComponent<PlayerCombat>();
            if (playerStats != null) playerStats.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(0.5f);

        _lastAttackTime = Time.time;
        TransitionToState(BossState.Chasing);
    }

    private IEnumerator EvasionRoutine()
    {
        TransitionToState(BossState.Evading);

        ClearAnimatorTriggers(); // Wipe the memory before forcing the dash

        if (_anim != null) _anim.SetTrigger("Dash");

        float timer = 0f;
        while (timer < dashDuration)
        {
            _rb.linearVelocity = new Vector2(_facingDirection * dashSpeed, _rb.linearVelocity.y);
            timer += Time.deltaTime;
            yield return null;
        }

        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

        yield return new WaitUntil(() => _isGrounded);

        _damageSinceLastEvasion = 0;
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
        _currentAttackRange = enragedAttackRange;

        ClearAnimatorTriggers();

        if (_anim != null) _anim.SetTrigger("Enrage");

        if (_mainCameraShaker != null) _mainCameraShaker.Shake();

        yield return new WaitForSeconds(1.5f);

        _damageSinceLastEvasion = 0;
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
        UpdateBossHealthBar();
        _damageSinceLastEvasion += amount;

        if (animatorSpriteRenderer != null && gameObject.activeInHierarchy)
            StartCoroutine(DamageFlashRoutine());

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        if (_damageSinceLastEvasion >= damageThresholdForEvasion && _currentState == BossState.Chasing)
            StartCoroutine(EvasionRoutine());
    }
    private IEnumerator DamageFlashRoutine()
    {
        animatorSpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        animatorSpriteRenderer.color = Color.white;
    }

    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => maxHealth;
    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
        UpdateBossHealthBar();
    }

    private void Die()
    {
        TransitionToState(BossState.Dead);
        _rb.linearVelocity = Vector2.zero;

        ClearAnimatorTriggers();

        if (_anim != null) _anim.SetTrigger("Die");
        if (_mainCameraShaker != null) _mainCameraShaker.Shake();

        GetComponent<Collider2D>().enabled = false;
        _rb.bodyType = RigidbodyType2D.Kinematic;

        StartCoroutine(DeathSequenceRoutine());
    }
    private IEnumerator DeathSequenceRoutine()
    {
        yield return new WaitForSeconds(2.0f);

        PlayerData.Instance.Data.UnlockedLevels = 2;
        PlayerData.Instance.Save();
        GameManager.Instance.LoadMainMenu();
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
        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            float currentHitRadius = meleeHitRadius * (Application.isPlaying && _isEnraged ? enragedSizeMultiplier : 1f);
            Gizmos.DrawWireSphere(attackPoint.position, currentHitRadius);
        }
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
    void UpdateBossHealthBar()
    {
        bossHealthBar.maxValue = maxHealth;
        bossHealthBar.value = _currentHealth;
        bossHealthText.text = _currentHealth + "/" + maxHealth;
    }
}