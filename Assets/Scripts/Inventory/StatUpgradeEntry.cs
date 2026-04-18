using UnityEngine;

[System.Serializable]
public class StatUpgradeEntry
{
    public int         atLevel;     // level at which this upgrade triggers
    public UpgradeType stat;        // which stat to improve
    public float       value;       // amount to add (fireRate: negative = faster)
}
