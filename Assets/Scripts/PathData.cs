using UnityEngine;

// Create via Assets > Create > Shmup > Path Data
// Waypoints are offsets relative to the enemy's spawn position.
// Example straight down: waypoints = { (0,0,-20) }
// Example sweep right then down: waypoints = { (4,0,-4), (4,0,-14) }
[CreateAssetMenu(fileName = "PathData", menuName = "Shmup/Path Data")]
public class PathData : ScriptableObject
{
    public Vector3[] waypoints;
    public float moveSpeed = 4f;
}
