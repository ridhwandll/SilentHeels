using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCombat playerStats = collision.gameObject.GetComponent<PlayerCombat>();
            playerStats.TakeDamage(999999); // Kill the player
            Camera.main.GetComponent<CameraShake>().Shake();
        }
    }
}
