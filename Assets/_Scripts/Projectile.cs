using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5.0f;

    private int _damage = 1;
    public float _speed = 10f;

    private Vector2 _direction;
    private bool _isEnemyBullet;

    public void Setup(Vector2 shootDirection, int damage, float speed, bool isEnemyBullet = false, Sprite customSprite = null)
    {
        _isEnemyBullet = isEnemyBullet;
        _damage = damage;
        _speed = speed;

        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();

        // If a sprite was passed in, swap it out dynamically!
        if (customSprite != null)
            sr.sprite = customSprite;

        sr.color = Color.white;

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
        if ((collision.gameObject.CompareTag("Enemy") && !_isEnemyBullet) || (collision.gameObject.CompareTag("Player") && _isEnemyBullet))
        {
            IHealth health = collision.gameObject.GetComponent<IHealth>();

            // Safety check: Only deal damage if the object actually has a health script
            if (health != null)
                health.TakeDamage(_damage);

            Destroy(gameObject);
        }
    }

    private void SetupTrailRenderer()
    {
        Color bulletColor = Color.white;

        // Set the color of the trail
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(bulletColor, 0.0f);
        colorKeys[1] = new GradientColorKey(bulletColor, 1.0f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(bulletColor.a, 1.0f);
        alphaKeys[1] = new GradientAlphaKey(bulletColor.a, 0.0f);

        gradient.SetKeys(colorKeys, alphaKeys);

        TrailRenderer tr = GetComponent<TrailRenderer>();
        if (tr != null)
        {
            tr.colorGradient = gradient;
        }
    }
}