using UnityEngine;

// Place one instance in the Hub scene.
// Positions the player at the correct spawn point on scene load.
public class HubSceneBootstrap : MonoBehaviour
{
    [SerializeField] private Transform defaultSpawnPoint;
    [SerializeField] private OverworldPlayerController playerPrefab;

    private OverworldPlayerController _player;

    void Start()
    {
        _player = FindAnyObjectByType<OverworldPlayerController>();

        if (_player == null && playerPrefab != null)
            _player = Instantiate(playerPrefab);

        if (_player == null)
        {
            Debug.LogWarning("[HubSceneBootstrap] No player found or assigned.");
            return;
        }

        // Use saved overworld position as a proxy for which entry was used,
        // otherwise fall back to the default spawn.
        Vector3 spawnPos = defaultSpawnPoint != null
            ? defaultSpawnPoint.position
            : Vector3.zero;

        _player.transform.position = spawnPos;
    }
}
