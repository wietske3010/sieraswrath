using UnityEngine;

[CreateAssetMenu(fileName = "ClueDatabase", menuName = "Game/Clue Database")]
public class ClueDatabase : ScriptableObject
{
    public ClueCombo[] allCombos; // Length = 10
}
