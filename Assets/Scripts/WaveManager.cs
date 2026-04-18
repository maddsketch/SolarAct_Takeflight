using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    private WaveDefinition[] waves;

    public int CurrentWave { get; private set; }

    public event System.Action<int> onWaveStart;   // wave number, 1-based
    public event System.Action onAllWavesComplete;

    private readonly List<GameObject> activeEnemies = new();

    // Called by ShmupSceneBootstrap once the level is ready
    public void StartWaves(WaveDefinition[] waveDefinitions)
    {
        waves = waveDefinitions;
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            CurrentWave = i + 1;
            onWaveStart?.Invoke(CurrentWave);
            yield return StartCoroutine(ExecuteWave(waves[i]));
            yield return new WaitForSeconds(waves[i].delayBeforeNextWave);
        }

        onAllWavesComplete?.Invoke();
    }

    IEnumerator ExecuteWave(WaveDefinition wave)
    {
        activeEnemies.RemoveAll(e => e == null);

        // Fire all spawn instructions in parallel (each handles its own delay)
        float maxDelay = 0f;
        foreach (var instruction in wave.spawnInstructions)
        {
            StartCoroutine(SpawnAfterDelay(instruction));
            if (instruction.delay > maxDelay)
                maxDelay = instruction.delay;
        }

        // Wait until the last enemy has actually spawned
        yield return new WaitForSeconds(maxDelay);

        // Now wait for the clear condition
        if (wave.clearCondition == WaveClearCondition.KillAll)
            yield return new WaitUntil(AllEnemiesDead);
        else
            yield return new WaitForSeconds(wave.timerDuration);
    }

    IEnumerator SpawnAfterDelay(SpawnInstruction instruction)
    {
        if (instruction.delay > 0f)
            yield return new WaitForSeconds(instruction.delay);

        if (instruction.enemyPrefab == null) yield break;

        GameObject enemy = Instantiate(
            instruction.enemyPrefab, instruction.spawnPosition, Quaternion.identity);

        var mover = enemy.GetComponent<EnemyMover>();
        if (mover != null && instruction.pathData != null)
            mover.Init(instruction.pathData, instruction.spawnPosition);

        activeEnemies.Add(enemy);

        var health = enemy.GetComponent<Health>();
        if (health != null)
            health.onDeath.AddListener(() => activeEnemies.Remove(enemy));
    }

    bool AllEnemiesDead()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count == 0;
    }
}
