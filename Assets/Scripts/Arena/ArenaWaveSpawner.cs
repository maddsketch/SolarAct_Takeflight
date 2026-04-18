using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns enemies in waves around the CircleBoundary perimeter.
// Waits for all enemies to be killed before starting the next wave.
public class ArenaWaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ArenaWave
    {
        public GameObject enemyPrefab;
        public int count = 5;
        public float spawnInterval = 0.4f;  // delay between each enemy in the wave
        public float delayBeforeWave = 1.5f;
    }

    [SerializeField] private List<ArenaWave> waves = new();
    [SerializeField] private Transform spawnParent;

    public int CurrentWave { get; private set; }

    public event System.Action<int> onWaveStart;   // wave number, 1-based
    public event System.Action onAllWavesComplete;

    void Start()
    {
        ArenaGameManager.Instance.onAllEnemiesKilled += OnWaveCleared;
        StartCoroutine(RunWaves());
    }

    void OnDestroy()
    {
        if (ArenaGameManager.Instance != null)
            ArenaGameManager.Instance.onAllEnemiesKilled -= OnWaveCleared;
    }

    private bool waitingForClear = false;

    private IEnumerator RunWaves()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            var wave = waves[i];
            CurrentWave = i + 1;

            yield return new WaitForSeconds(wave.delayBeforeWave);

            onWaveStart?.Invoke(CurrentWave);
            ArenaGameManager.Instance.RegisterEnemies(wave.count);

            for (int e = 0; e < wave.count; e++)
            {
                SpawnEnemy(wave.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            // Wait until ArenaGameManager reports all dead
            waitingForClear = true;
            yield return new WaitUntil(() => !waitingForClear);
        }

        onAllWavesComplete?.Invoke();
    }

    private void OnWaveCleared()
    {
        waitingForClear = false;
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || CircleBoundary.Instance == null) return;

        Vector3 pos = CircleBoundary.Instance.RandomPerimeterPoint();
        Instantiate(prefab, pos, Quaternion.identity, spawnParent);
    }
}
