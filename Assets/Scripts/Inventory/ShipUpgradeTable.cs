using UnityEngine;

// Create via Assets > Create > TakeFlight > Ship Upgrade Table
// One asset for the whole game. Maps player levels to automatic stat upgrades.
[CreateAssetMenu(fileName = "ShipUpgradeTable", menuName = "TakeFlight/Ship Upgrade Table")]
public class ShipUpgradeTable : ScriptableObject
{
    public StatUpgradeEntry[] entries;
}
