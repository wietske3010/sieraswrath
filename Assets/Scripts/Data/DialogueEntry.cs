using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public int roundIndex; // 1-6
    public string speakerName;
    [TextArea(5, 10)]
    public string[] dialogueLines;
}
