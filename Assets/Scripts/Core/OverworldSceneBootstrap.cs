using System.Collections.Generic;
using UnityEngine;

// Place one instance of this in the Overworld scene.
// Restores the player to their saved position, or a named spawn point if one was requested.
public class OverworldSceneBootstrap : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnPoint
    {
        public string id;           // e.g. "hub_sector01_exit"
        public Transform transform;
    }

    [SerializeField] private OverworldPlayerController player;
    [SerializeField] private Vector3 defaultSpawnPosition;
    [SerializeField] private List<SpawnPoint> spawnPoints;  // designer-defined named spawn points

    void Start()
    {
        if (GameStateManager.Instance == null) return;

        // Check if a specific spawn was requested (e.g. returning from a hub)
        string requestedSpawn = SceneTransitionManager.Instance?.RequestedSpawnID;

        if (!string.IsNullOrEmpty(requestedSpawn) && spawnPoints != null)
        {
            foreach (var sp in spawnPoints)
            {
                if (sp.id == requestedSpawn && sp.transform != null)
                {
                    player.transform.position = sp.transform.position;
                    SceneTransitionManager.Instance.RequestedSpawnID = null;
                    return;
                }
            }
        }

        var data = GameStateManager.Instance.Current;
        bool useSavedPosition = data.hasOverworldPosition
            || (!data.hasOverworldPosition && data.overworldPosition != Vector3.zero);

        if (!useSavedPosition)
        {
            player.transform.position = defaultSpawnPosition;
            return;
        }

        player.transform.position = data.overworldPosition;
        if (data.hasOverworldPosition)
            player.transform.eulerAngles = data.overworldEulerAngles;
    }
}
