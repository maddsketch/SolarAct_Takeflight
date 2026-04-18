using System;
using UnityEngine;

public enum DialoguePortraitSide
{
    Left,
    Right,
}

[Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string text;

    [Tooltip("Which side of the dialogue UI shows this speaker for this line.")]
    public DialoguePortraitSide portraitSide = DialoguePortraitSide.Left;

    [Tooltip("Optional portrait for this line. If null, DialogueData default for this side is used.")]
    public Sprite speakerPortrait;
}
