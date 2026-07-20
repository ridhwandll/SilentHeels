using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour, IHealth
{
    [Header("Melee (Kicks)")]
    public Transform MeleeAttackPoint;
    public float MeleeAttackRadius = 0.6f;
    public int MeleeDamage = 10;
    public float MeleeAttackRate = 2f;
    private float _NextMeleeTime = 0f;

    [Header("Ranged")]
    public GameObject ProjectilePrefab;
    public Transform FirePoint;
    public int ProjectileDamage = 10;
    public float ProjectileSpeed = 15f;
    private float _NextRangedTime = 0f;

    [Header("Target")]
    public LayerMask EnemyLayer;

    private int _Health;
    private PlayerMovement _PlayerMovement;
    private Animator _Anim;

    public Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public Action OnPlayerDied; // Called when the player dies
    private bool _healthUpdatedOnce = false;
    private bool _isDead = false;

    void Start()
    {
        _PlayerMovement = GetComponent<PlayerMovement>();
        _Anim = GetComponentInChildren<Animator>();
        _Health = PlayerData.Instance.Data.MaxHealth;

        if (_Anim == null)
            Debug.Log("Animation is null in PlayerCombat!");
    }

    void Update()
    {
        if (!_healthUpdatedOnce)
        {
            OnHealthChanged?.Invoke(_Health, PlayerData.Instance.Data.MaxHealth);
            _healthUpdatedOnce = true;
        }

        if (_isDead)
            return;

        if (Time.time >= _NextMeleeTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ExecuteMeleeAttack();
                _NextMeleeTime = Time.time + 1f / MeleeAttackRate;
            }
        }

        if (PlayerData.Instance.Data.CanRangeAttack && Time.time >= _NextRangedTime)
        {
            if (Input.GetMouseButtonDown(1))
            {
                ExecuteRangedAttack();
                _NextRangedTime = Time.time + 1f / PlayerData.Instance.Data.RangeAttackRate;
            }
        }

        // ==========================================
        // NOTE: SPECIAL ATTACK & TRANSFORMATION 
        // ==========================================

        if (Input.GetKeyDown(KeyCode.L))
        {
            _Anim.SetTrigger("SpecialAttack");
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            _Anim.SetTrigger("Transformation");
        }
    }

    private void ExecuteMeleeAttack()
    {
        _Anim.SetTrigger("MeleeAttack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(MeleeAttackPoint.position, MeleeAttackRadius, EnemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            IHealth healthTarget = enemy.GetComponent<IHealth>();

            if (healthTarget != null)
                healthTarget.TakeDamage(MeleeDamage * PlayerData.Instance.Data.MeleeAttackDamageMultiplier);
        }
    }

    private void ExecuteRangedAttack()
    {
        if (!PlayerData.Instance.Data.CanRangeAttack)
            return;

        _Anim.SetTrigger("RangedAttack");

        GameObject projectile = Instantiate(ProjectilePrefab, FirePoint.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().Setup(new Vector2(_PlayerMovement.GetFacingDirection(), 0f), ProjectileDamage * PlayerData.Instance.Data.RangeAttackDamageMultiplier, ProjectileSpeed * PlayerData.Instance.Data.RangeAttackSpeedMultiplier);
        Destroy(projectile, 10.0f);
    }

    public int GetCurrentHealth() => _Health;
    public int GetMaxHealth() => PlayerData.Instance.Data.MaxHealth;

    public void TakeDamage(int amount)
    {
        if (_isDead || PlayerData.Instance.Data.IsInvincible)
            return;

        _Health = Mathf.Max(0, _Health - amount);
        OnHealthChanged?.Invoke(_Health, PlayerData.Instance.Data.MaxHealth);

        if (_Health > 0)
        {
            _Anim.SetTrigger("TakeDamage");
        }
        else
        {
            // Death Logic
            _isDead = true;
            _Anim.SetBool("IsDead", true);

            if (_PlayerMovement != null)
                _PlayerMovement.enabled = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            OnPlayerDied?.Invoke();
        }

        // (Rid) TODO
        //if (_Health <= 15 && _chromaticAberration)
        //{
        //    _chromaticAberration.active = true;
        //    _vignette.color.value = Color.red;
        //}
        //if (_Health == 0)
        //{
        //    Destroy(gameObject);
        //    GameManager.Instance.SetPlayerAlive(false);
        //}
        //gameScreenUIManager.UpdatePlayerHealth(_Health);
    }

    public void Heal(int amount)
    {
        if (_isDead) return; // Optionally prevent healing dead players

        _Health = Mathf.Min(PlayerData.Instance.Data.MaxHealth, _Health + amount);
        OnHealthChanged?.Invoke(_Health, PlayerData.Instance.Data.MaxHealth);

        // (Rid) TODO
        //if (_Health > 15 && _chromaticAberration)
        //{
        //    _chromaticAberration.active = false;
        //    _vignette.color.value = Color.black;
        //}
        //gameScreenUIManager.UpdatePlayerHealth(_Health);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(MeleeAttackPoint.position, MeleeAttackRadius);
    }
}