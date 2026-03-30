using UnityEngine;

/// <summary>
/// Place in any scene. When enableDebugBootstrap is ticked and you run
/// directly from the scene (no MainMenu), this spins up the persistent managers
/// and starts the appropriate mode. Disable before shipping.
/// </summary>
public class DebugBootstrap : MonoBehaviour
{
    public enum DebugMode { Level, Conclusion }

    [Header("Debug — untick before shipping")]
    public bool enableDebugBootstrap = false;

    public DebugMode debugMode = DebugMode.Level;

    [Tooltip("Only used in Level mode. 1 = Catacombs, 2 = Gardens, 3 = Corridors")]
    public int debugLevelIndex = 1;

    [Tooltip("Only used in Conclusion mode. Tick to simulate having collected all 6 clues.")]
    public bool simulateCluesCollected = false;

    [Tooltip("Drag the PersistentManagers prefab here")]
    public GameObject persistentManagersPrefab;

    void Awake()
    {
        if (!enableDebugBootstrap) return;

        // Spin up persistent managers only if not already present (e.g. first play in editor)
        if (GameManager.Instance == null)
        {
            if (persistentManagersPrefab == null)
            {
                Debug.LogError("DebugBootstrap: PersistentManagers prefab not assigned.");
                return;
            }

            Instantiate(persistentManagersPrefab);

            if (debugMode == DebugMode.Conclusion)
                GameManager.Instance.DebugStartConclusion();
            else
                GameManager.Instance.DebugStartLevel(debugLevelIndex);
        }

        // Simulate clues independently — runs whether or not managers were just created,
        // so re-entering play mode with domain reload off still seeds clues correctly.
        if (debugMode == DebugMode.Conclusion && simulateCluesCollected)
        {
            ClueDatabaseManager.Instance.InitialisePlaythrough();
            var combos = GameStateManager.Instance.GetSelectedCombos();
            foreach (string key in combos.Keys)
                GameStateManager.Instance.CollectClue(key);
        }
    }
}
