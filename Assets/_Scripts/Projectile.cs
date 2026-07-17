using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5.0f;

    private int _damage = 1;
    public float _speed = 10f;
    public Color playerProjectileColor = Color.orangeRed;
    public Color enemyProjectileColor = Color.darkGreen;

    private Vector2 _direction;
    private bool _isEnemyBullet;

    public void Setup(Vector2 shootDirection, int damage, float speed, bool isEnemyBullet = false)
    {
        _isEnemyBullet = isEnemyBullet;
        _damage = damage;
        _speed = speed;

        Color bulletColor = _isEnemyBullet ? enemyProjectileColor : playerProjectileColor;
        gameObject.GetComponent<SpriteRenderer>().color = bulletColor;

        _direction = shootDirection.normalized;

        Destroy(gameObject, lifetime);
        SetupTrailRenderer();
    }

    void Update()
    {
        transform.position += (Vector3)_direction * _speed * Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        IHealth health = collision.gameObject.GetComponent<IHealth>();
        if (health == null)
            return;

        if ((collision.gameObject.CompareTag("Enemy") && !_isEnemyBullet) || (collision.gameObject.CompareTag("Player") && _isEnemyBullet))
        {
            health.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }

    //void OnTriggerEnter2D(Collider2D collision)
    //{
    //    IHealth health = collision.gameObject.GetComponent<IHealth>();
    //    if (collision.gameObject.CompareTag("Player") && _isEnemyBullet)
    //    {
    //        health.TakeDamage(_damage);
    //        Destroy(gameObject);
    //    }
    //}

    private void SetupTrailRenderer()
    {
        Color bulletColor = _isEnemyBullet ? enemyProjectileColor : playerProjectileColor;

        // Set the color of the trail
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(bulletColor, 0.0f);
        colorKeys[1] = new GradientColorKey(bulletColor, 1.0f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(bulletColor.a, 1.0f);
        alphaKeys[1] = new GradientAlphaKey(bulletColor.a, 0.0f);

        gradient.SetKeys(colorKeys, alphaKeys);

        GetComponent<TrailRenderer>().colorGradient = gradient;
    }
}