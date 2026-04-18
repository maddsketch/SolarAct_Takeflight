using System.Collections;
using UnityEngine;

// Place one instance in the shmup scene alongside ShmupSceneBootstrap.
// Assign a BackgroundEventDefinition and call Play() when the level starts.
public class BackgroundEventPlayer : MonoBehaviour
{
    [SerializeField] private BackgroundEventDefinition definition;
    [SerializeField] private Transform spawnParent; // optional, keeps hierarchy tidy

    private bool isPlaying;

    public void Play()
    {
        if (isPlaying) return;
        if (definition == null || definition.entries == null || definition.entries.Length == 0)
            return;

        isPlaying = true;
        StartCoroutine(RunEvents());
    }

    private IEnumerator RunEvents()
    {
        float elapsed = 0f;
        int index = 0;
        var entries = definition.entries;

        // Sort by triggerTime so authoring order doesn't matter
        System.Array.Sort(entries, (a, b) => a.triggerTime.CompareTo(b.triggerTime));

        while (index < entries.Length)
        {
            elapsed += Time.deltaTime;

            while (index < entries.Length && elapsed >= entries[index].triggerTime)
            {
                Spawn(entries[index]);
                index++;
            }

            yield return null;
        }
    }

    private void Spawn(BackgroundEventEntry entry)
    {
        if (entry.prefab == null) return;

        Quaternion rotation = Quaternion.Euler(entry.spawnRotation);
        Instantiate(entry.prefab, entry.spawnPosition, rotation, spawnParent);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (definition == null || definition.entries == null) return;

        Gizmos.color = new Color(0.8f, 0.4f, 1f, 0.8f);
        foreach (var entry in definition.entries)
            Gizmos.DrawWireSphere(entry.spawnPosition, 0.4f);
    }
#endif
}
