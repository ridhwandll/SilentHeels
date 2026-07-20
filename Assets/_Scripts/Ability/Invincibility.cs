using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boosts/Invincibility")]
public class Invincibility : Ability
{
    public override void Activate()
    {
        PlayerData.Instance.Data.IsInvincible = true;
    }

    public override void Deactivate()
    {
        PlayerData.Instance.Data.IsInvincible = false;
    }
}
