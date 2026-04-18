using UnityEngine;

// Place on an NPC or object in the overworld to make it open a shop.
public class ShopInteractable : Interactable
{
    [SerializeField] private ShopDefinition shopDefinition;

    public override void Interact(OverworldPlayerController player)
    {
        if (ShopManager.Instance == null)
        {
            Debug.LogWarning("[ShopInteractable] No ShopManager in scene.");
            return;
        }
        ShopManager.Instance.OpenShop(shopDefinition);
    }
}
