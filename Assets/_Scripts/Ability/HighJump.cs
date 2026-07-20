using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boosts/HighJump")]
public class HighJump : Ability
{
    [Header("Boost Stats")]
    public int ExtraJumpsToGrant = 1;

    public override void Activate()
    {
        PlayerData.Instance.Data.ExtraJumps += ExtraJumpsToGrant;
    }

    public override void Deactivate()
    {
        PlayerData.Instance.Data.ExtraJumps -= ExtraJumpsToGrant;
    }
}