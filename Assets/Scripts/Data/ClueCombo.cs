using UnityEngine;

[System.Serializable]
public class ClueCombo
{
    public string comboID;
    [TextArea(3, 5)]
    public string questionText;
    [TextArea(3, 5)]
    public string clueText;
    public string correctAnswer;
}
