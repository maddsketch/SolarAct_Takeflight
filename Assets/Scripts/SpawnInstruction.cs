using UnityEngine;

[System.Serializable]
public class SpawnInstruction
{
    public GameObject enemyPrefab;
    public Vector3 spawnPosition;
    public float delay;          // seconds after wave start
    public PathData pathData;
}
