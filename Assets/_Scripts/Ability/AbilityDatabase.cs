using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Database")]
public class AbilityDatabase : ScriptableObject
{
    public List<Ability> AllAbilities = new List<Ability>();

    public Ability GetAbilityByName(string name)
    {
        foreach (var ability in AllAbilities)
        {
            if (ability.AbilityName == name)
                return ability;
        }
        return null;
    }
}