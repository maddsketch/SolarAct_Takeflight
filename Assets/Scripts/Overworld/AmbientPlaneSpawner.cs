using System.Collections.Generic;
using UnityEngine;

// Place one instance in the Overworld scene.
// Assign AmbientPlaneDefinition assets and tune spawn settings in the Inspector.
public class AmbientPlaneSpawner : MonoBehaviour
{
    [SerializeField] private List<AmbientPlaneDefinition> planeTypes = new();
    [SerializeField] private int spawnCount = 20;

    [Tooltip("Horizontal area planes are scattered across (X/Z).")]
    [SerializeField] private Vector2 areaX = new Vector2(-50f, 50f);
    [SerializeField] private Vector2 areaZ = new Vector2(-50f, 50f);

    void Start()
    {
        if (planeTypes.Count == 0)
        {
            Debug.LogWarning("[AmbientPlaneSpawner] No plane types assigned.");
            return;
        }

        float totalWeight = 0f;
        foreach (var def in planeTypes) totalWeight += def.weight;

        Bounds loopBounds = new Bounds(
            new Vector3((areaX.x + areaX.y) * 0.5f, 0f, (areaZ.x + areaZ.y) * 0.5f),
            new Vector3(areaX.y - areaX.x, 1000f, areaZ.y - areaZ.x)
        );

        for (int i = 0; i < spawnCount; i++)
        {
            var def = PickWeighted(totalWeight);
            if (def?.prefab == null) continue;

            Vector3 pos = new Vector3(
                Random.Range(areaX.x, areaX.y),
                Random.Range(def.altitudeRange.x, def.altitudeRange.y),
                Random.Range(areaZ.x, areaZ.y)
            );

            var go = Instantiate(def.prefab, pos, Quaternion.identity, transform);
            go.GetComponent<AmbientPlaneMover>()?.Init(loopBounds);
        }
    }

    private AmbientPlaneDefinition PickWeighted(float totalWeight)
    {
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var def in planeTypes)
        {
            cumulative += def.weight;
            if (roll <= cumulative) return def;
        }

        return planeTypes[planeTypes.Count - 1];
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.2f);
        Vector3 center = new Vector3((areaX.x + areaX.y) * 0.5f, 0f, (areaZ.x + areaZ.y) * 0.5f);
        Vector3 size   = new Vector3(areaX.y - areaX.x, 1f, areaZ.y - areaZ.x);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
