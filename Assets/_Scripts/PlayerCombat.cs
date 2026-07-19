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

        // --- NEW: Stop reading inputs if the player is dead ---
        if (_isDead) return;

        if (Time.time >= _NextMeleeTime)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J))
            {
                ExecuteMeleeAttack();
                _NextMeleeTime = Time.time + 1f / MeleeAttackRate;
            }
        }

        if (PlayerData.Instance.Data.CanRangeAttack && Time.time >= _NextRangedTime)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.K))
            {
                ExecuteRangedAttack();
                _NextRangedTime = Time.time + 1f / PlayerData.Instance.Data.RangeAttackRate;
            }
        }

        // ==========================================
        // NOTE: SPECIAL ATTACK & TRANSFORMATION 
        // ==========================================

        // if (Input.GetKeyDown(KeyCode.L)) 
        // {
        //     _Anim.SetTrigger("SpecialAttack");
        // }

        // if (Input.GetKeyDown(KeyCode.T)) 
        // {
        //     _Anim.SetTrigger("Transformation");
        // }
    }

    private void ExecuteMeleeAttack()
    {
        _Anim.SetTrigger("MeleeAttack"); 

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(MeleeAttackPoint.position, MeleeAttackRadius, EnemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            IHealth healthTarget = enemy.GetComponent<IHealth>();

            if (healthTarget != null)
            {
                healthTarget.TakeDamage(MeleeDamage);
            }
        }
    }

    private void ExecuteRangedAttack()
    {
        _Anim.SetTrigger("RangedAttack"); 

        GameObject projectile = Instantiate(ProjectilePrefab, FirePoint.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().Setup(new Vector2(_PlayerMovement.GetFacingDirection(), 0f), ProjectileDamage * PlayerData.Instance.Data.RangeAttackDamageMultiplier, ProjectileSpeed * PlayerData.Instance.Data.RangeAttackSpeedMultiplier);
        Destroy(projectile, 10.0f);
    }

    public int GetCurrentHealth() => _Health;
    public int GetMaxHealth() => PlayerData.Instance.Data.MaxHealth;

    public void TakeDamage(int amount)
    {
        // --- NEW: Prevent taking further damage if already dead ---
        if (_isDead) return;

        _Health = Mathf.Max(0, _Health - amount);
        OnHealthChanged?.Invoke(_Health, PlayerData.Instance.Data.MaxHealth);

        if (_Health > 0)
        {
            _Anim.SetTrigger("TakeDamage");
        }
        else
        {
            // --- NEW: Death Logic ---
            _isDead = true;
            _Anim.SetBool("IsDead", true);

            // 1. Disable the movement script so the player can't run or jump
            if (_PlayerMovement != null)
                _PlayerMovement.enabled = false;

            // 2. Stop horizontal sliding momentum, but allow them to fall to the floor
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
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
        if (MeleeAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(MeleeAttackPoint.position, MeleeAttackRadius);
        }
    }
}