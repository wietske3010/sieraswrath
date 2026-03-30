using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelIndex; // 1, 2, or 3
    public string levelName; // "Catacombs", "Gardens", "Corridors"

    [Header("Spawn Point - Use GameObject Tag")]
    public string sieraSpawnPointTag = "SieraSpawn";

    [Header("Level Exit")]
    public string levelExitTriggerTag = "LevelExit";

    [Header("NPCs - Use GameObject Tags")]
    public string[] hiddenNPCSlotTags = new string[2] { "HiddenNPC_1", "HiddenNPC_2" };

    [Header("Enemies - Use GameObject Tags")]
    public string[] enemyTags; // e.g., { "Enemy_1", "Enemy_2", "Enemy_3" }
}
