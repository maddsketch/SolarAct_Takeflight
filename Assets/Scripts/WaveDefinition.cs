using UnityEngine;

public enum WaveClearCondition { KillAll, Timer }

// Create via Assets > Create > Shmup > Wave Definition
[CreateAssetMenu(fileName = "WaveDefinition", menuName = "Shmup/Wave Definition")]
public class WaveDefinition : ScriptableObject
{
    public SpawnInstruction[] spawnInstructions;
    public WaveClearCondition clearCondition = WaveClearCondition.KillAll;
    public float timerDuration = 20f;       // only used when clearCondition == Timer
    public float delayBeforeNextWave = 2f;
}
