using UnityEngine;

[CreateAssetMenu(fileName = "NarrativeSchedule", menuName = "Game/Narrative Schedule")]
public class NarrativeSchedule : ScriptableObject
{
    public DialogueEntry[] entries; // Length = 6
}
