using UnityEngine;

[CreateAssetMenu(fileName = "New Shop", menuName = "TakeFlight/Shop Definition")]
public class ShopDefinition : ScriptableObject
{
    public string shopName = "Shop";

    // Item IDs available in this shop. Order determines display order.
    public string[] itemIDs;
}
