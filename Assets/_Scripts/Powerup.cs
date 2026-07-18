using UnityEngine;
// (Rid) Replace this with actual powerup(shoe) names
public enum PowerupType { Level1, Level2, Level3 }

public class Powerup : MonoBehaviour
{
    public PowerupType _type;

    void Setup(PowerupType type)
    {
        _type = type;
    }

    public void UpgradePlayerData(PowerupType type)
    {
        // First boss defeat high speed dash and high jump(fly rakhar iccha ase but pore)
        // 2nd boss(projectile shooting after certain amount of melee damage done)
        // Third boss(brief moment of invincibility)

        // Modify PlayerData.Instance.Data instances here Arat
        switch (type)
        {
            case PowerupType.Level1:
                PlayerData.Instance.Data.CanDash = true;
                PlayerData.Instance.Data.ExtraJumps = 1;
                break;
            case PowerupType.Level2:
                PlayerData.Instance.Data.CanRangeAttack = true;
                PlayerData.Instance.Data.ExtraJumps = 2;
                PlayerData.Instance.Data.RangeAttackRate = 1;
                break;
            case PowerupType.Level3:
                PlayerData.Instance.Data.ExtraJumps = 3;
                //TODO
                break;
        }

        PlayerData.Instance.Save();
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCombat playerCombat = collision.gameObject.GetComponent<PlayerCombat>();
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();

            UpgradePlayerData(_type);

            Destroy(gameObject);
        }
    }
}
