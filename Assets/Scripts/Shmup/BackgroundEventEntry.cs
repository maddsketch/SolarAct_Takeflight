using UnityEngine;

[System.Serializable]
public class BackgroundEventEntry
{
    public float triggerTime;       // seconds from level start
    public GameObject prefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;   // euler angles
}
