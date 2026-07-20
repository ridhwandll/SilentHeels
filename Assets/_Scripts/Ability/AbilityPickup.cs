using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AbilityPickup : MonoBehaviour
{
    public Ability AbilityToGrant;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!PlayerData.Instance.Data.UnlockedAbilities.Contains(AbilityToGrant.AbilityName))
            {
                PlayerData.Instance.Data.UnlockedAbilities.Add(AbilityToGrant.AbilityName);
                PlayerData.Instance.Save();
                Debug.Log($"Unlocked ability: {AbilityToGrant.AbilityName}");
            }
            Destroy(gameObject);
        }
    }
}
