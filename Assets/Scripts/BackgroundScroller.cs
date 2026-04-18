using UnityEngine;

// Attach to a large Plane/Quad that covers the play area.
// The material must have a tiling texture (URP Lit or Unlit).
public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.3f;

    private Renderer rend;
    private Material matInstance;
    private float offset;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        matInstance = rend.material;
    }

    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;
        matInstance.SetTextureOffset("_BaseMap", new Vector2(0f, offset));
    }

    void OnDestroy()
    {
        if (matInstance != null)
            Destroy(matInstance);
    }
}
