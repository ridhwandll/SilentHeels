using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public GameObject BossHealthBar;
    public GameObject BossGameObject;
    public LevelBGMPlayer BGMPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BossHealthBar.SetActive(true);
            BossGameObject.SetActive(true);
            BGMPlayer.PlayBossMusic();
            Camera.main.GetComponent<CameraShake>().Shake();
            Destroy(gameObject);
        }
    }
}