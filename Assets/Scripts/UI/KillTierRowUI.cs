using UnityEngine;
using UnityEngine.UI;

// One row in the level-complete kill tier list: a single Image whose sprite reflects tier state.
[RequireComponent(typeof(Image))]
public class KillTierRowUI : MonoBehaviour
{
    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetState(Sprite lockedSprite, Sprite reachedSprite, Sprite rewardAppliedSprite, bool reached, bool rewardApplied)
    {
        if (image == null)
            image = GetComponent<Image>();

        Sprite chosen = lockedSprite;
        if (rewardApplied && rewardAppliedSprite != null)
            chosen = rewardAppliedSprite;
        else if (reached && reachedSprite != null)
            chosen = reachedSprite;
        else if (lockedSprite != null)
            chosen = lockedSprite;

        if (chosen != null)
            image.sprite = chosen;
    }
}
