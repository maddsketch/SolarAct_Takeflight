using UnityEngine;

// Attach to any GameObject with a Renderer.
// Fades the material from transparent (far) to opaque (near) based on player distance.
// Requires the material to use a URP shader with Surface Type set to Transparent.
[RequireComponent(typeof(Renderer))]
public class ProximityOpacity : MonoBehaviour
{
    [SerializeField] private float nearDistance = 3f;   // fully opaque within this distance
    [SerializeField] private float farDistance  = 8f;   // fully transparent beyond this distance
    [SerializeField] private float minAlpha     = 0f;   // alpha when far
    [SerializeField] private float maxAlpha     = 1f;   // alpha when near
    [SerializeField] private float smoothSpeed  = 5f;   // how fast the fade transitions

    private Transform player;
    private Material  matInstance;
    private float     currentAlpha;

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        // Create a material instance so we don't affect the shared asset
        matInstance = GetComponent<Renderer>().material;

        player = GameObject.FindWithTag("Player")?.transform;

        // Start fully transparent
        currentAlpha = minAlpha;
        ApplyAlpha(currentAlpha);
    }

    void Update()
    {
        if (player == null) return;

        float dist        = Vector3.Distance(transform.position, player.position);
        float t           = 1f - Mathf.InverseLerp(nearDistance, farDistance, dist);
        float targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, smoothSpeed * Time.deltaTime);
        ApplyAlpha(currentAlpha);
    }

    private void ApplyAlpha(float alpha)
    {
        if (matInstance == null) return;
        Color col = matInstance.GetColor(BaseColorID);
        col.a = alpha;
        matInstance.SetColor(BaseColorID, col);
    }

    void OnDestroy()
    {
        // Clean up the material instance to avoid memory leaks
        if (matInstance != null) Destroy(matInstance);
    }
}
