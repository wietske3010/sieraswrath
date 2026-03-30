using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClueDatabaseManager : MonoBehaviour
{
    public static ClueDatabaseManager Instance;

    [Header("Database")]
    public ClueDatabase database;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitialisePlaythrough()
    {
        if (database == null || database.allCombos.Length < 6)
        {
            Debug.LogError("ClueDatabase not properly configured! Needs at least 6 entries.");
            return;
        }

        // Shuffle and select 6
        ClueCombo[] shuffled = database.allCombos.OrderBy(x => Random.value).ToArray();
        ClueCombo[] selected = shuffled.Take(6).ToArray();

        // Convert to dictionary (comboID -> correctAnswer)
        Dictionary<string, string> combos = new Dictionary<string, string>();
        foreach (ClueCombo combo in selected)
        {
            combos.Add(combo.comboID, combo.correctAnswer);
        }

        GameStateManager.Instance.SetSelectedCombos(combos);

        Debug.Log($"Initialized playthrough with {selected.Length} clues");
    }

    public ClueCombo GetComboByID(string comboID)
    {
        if (database == null) return null;
        return database.allCombos.FirstOrDefault(c => c.comboID == comboID);
    }
}
