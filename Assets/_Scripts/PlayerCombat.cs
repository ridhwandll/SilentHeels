using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour, IHealth
{
    public int MaxHealth;

    [Header("Melee (Kicks)")]
    public Transform MeleeAttackPoint;
    public float MeleeAttackRadius = 0.6f;
    public int MeleeDamage = 10;
    public float MeleeAttackRate = 2f;
    private float _NextMeleeTime = 0f;

    [Header("Ranged")]
    public GameObject ProjectilePrefab;
    public Transform FirePoint;
    public float ProjectileSpeed = 15f;
    public float RangedFireRate = 1.5f;
    private float _NextRangedTime = 0f;

    [Header("Target")]
    public LayerMask EnemyLayer;

    private int _Health;
    private PlayerMovement _PlayerMovement;

    void Start()
    {
        _PlayerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Time.time >= _NextMeleeTime)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J))
            {
                ExecuteMeleeAttack();
                _NextMeleeTime = Time.time + 1f / MeleeAttackRate;
            }
        }

        if (Time.time >= _NextRangedTime)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.K))
            {
                ExecuteRangedAttack();
                _NextRangedTime = Time.time + 1f / RangedFireRate;
            }
        }
    }

    // TODO: Rid
    private void ExecuteMeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(MeleeAttackPoint.position, MeleeAttackRadius, EnemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // (TODO: Rid) TODO: Call enemy script here
        }
    }

    private void ExecuteRangedAttack()
    {
        if (ProjectilePrefab == null || FirePoint == null)
            return;

        float facingDirection = _PlayerMovement.GetFacingDirection();

        GameObject projectile = Instantiate(ProjectilePrefab, FirePoint.position, Quaternion.identity);

        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
            projRb.linearVelocity = new Vector2(facingDirection * ProjectileSpeed, 0f);

        if (facingDirection < 0)
            projectile.transform.localScale = new Vector3(-1, 1, 1);
    }

    public int GetCurrentHealth() => _Health;
    public int GetMaxHealth() => MaxHealth;

    public void TakeDamage(int amount)
    {
        _Health = Mathf.Max(0, _Health - amount);

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
        _Health = Mathf.Min(MaxHealth, _Health + amount);

        // (Rid) TODO
        //if (_Health > 15 && _chromaticAberration)
        //{
        //    _chromaticAberration.active = false;
        //    _vignette.color.value = Color.black;
        //}
        //gameScreenUIManager.UpdatePlayerHealth(_Health);
    }
}