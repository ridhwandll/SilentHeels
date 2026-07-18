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

    void Start()
    {
        _PlayerMovement = GetComponent<PlayerMovement>();
        _Health = PlayerData.Instance.Data.MaxHealth;
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

        if (PlayerData.Instance.Data.CanRangeAttack && Time.time >= _NextRangedTime)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.K))
            {
                ExecuteRangedAttack();
                _NextRangedTime = Time.time + 1f / PlayerData.Instance.Data.RangeAttackRate;
            }
        }
    }

    private void ExecuteMeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(MeleeAttackPoint.position, MeleeAttackRadius, EnemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            // This grabs ANY script attached to the target that uses IHealth!
            IHealth healthTarget = enemy.GetComponent<IHealth>();

            if (healthTarget != null)
            {
                healthTarget.TakeDamage(MeleeDamage);
            }
        }
    }

    private void ExecuteRangedAttack()
    {
        GameObject projectile = Instantiate(ProjectilePrefab, FirePoint.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().Setup(new Vector2(_PlayerMovement.GetFacingDirection(), 0f), ProjectileDamage * PlayerData.Instance.Data.RangeAttackDamageMultiplier, ProjectileSpeed * PlayerData.Instance.Data.RangeAttackSpeedMultiplier);
        Destroy(projectile, 10.0f);
    }

    public int GetCurrentHealth() => _Health;
    public int GetMaxHealth() => PlayerData.Instance.Data.MaxHealth;

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
        _Health = Mathf.Min(PlayerData.Instance.Data.MaxHealth, _Health + amount);

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