using UnityEngine;

public class SlowDrift : MonoBehaviour
{
    [SerializeField] private Vector3 direction = Vector3.right;
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private bool useLocalSpace = true;

    void Update()
    {
        Vector3 delta = direction.normalized * speed * Time.deltaTime;
        if (useLocalSpace)
            transform.localPosition += delta;
        else
            transform.position += delta;
    }
}
