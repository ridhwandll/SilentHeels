using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int healAmount = 30;

    [Header("Floating Animation")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.15f;
    private float _startY;

    void Start()
    {
        _startY = transform.position.y;
    }

    void Update()
    {
        float newY = _startY + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCombat playerStats = collision.GetComponent<PlayerCombat>();

            if (playerStats != null)
            {
                playerStats.Heal(healAmount);

                var particleSystem = GetComponent<ParticleSystem>();
                particleSystem.Play();

                GetComponent<SpriteRenderer>().enabled = false;
                Destroy(gameObject, particleSystem.main.duration + particleSystem.main.startLifetime.constant);
            }
        }
    }
}