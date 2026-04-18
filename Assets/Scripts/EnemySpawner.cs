using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnZ = 10f;
    [SerializeField] private Vector2 spawnXRange = new Vector2(-3.5f, 3.5f);

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Vector3 pos = new Vector3(Random.Range(spawnXRange.x, spawnXRange.y), 0f, spawnZ);
        Instantiate(prefab, pos, Quaternion.identity);
    }
}
