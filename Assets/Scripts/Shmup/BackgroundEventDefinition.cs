using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundEventDef_", menuName = "Shmup/Background Event Definition")]
public class BackgroundEventDefinition : ScriptableObject
{
    public BackgroundEventEntry[] entries;
}
