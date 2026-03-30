using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level Configuration")]
    public LevelData[] levelDataArray; // Length = 3, assigned in Inspector
    public GameObject sieraPrefab;

    [Header("Narrative")]
    public NarrativeSchedule narrativeSchedule;

    private GameState currentState;
    public GameState CurrentState => currentState;

    private int currentLevelIndex = 1; // 1-3
    private int livesLostThisLevel = 0;
    private int roundNPCIndex = 0;

    private GameObject sieraInstance;

    public event System.Action<GameState> OnStateChanged;
    public event System.Action OnPlayerCaught;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ─── Session Lifecycle ────────────────────────────────────────────────────

    public void NewGame()
    {
        SuspicionManager.Instance.Reset();
        GameStateManager.Instance.Reset();
        ClueDatabaseManager.Instance.InitialisePlaythrough();

        currentLevelIndex = 1;
        livesLostThisLevel = 0;
        roundNPCIndex = 0;

        SetState(GameState.Prologue);
        SceneManager.LoadScene("Prologue");
    }

    public void OnPrologueComplete()
    {
        SetState(GameState.LevelPlaying);
        SceneManager.LoadScene("Level1_Catacombs");
    }

    // Called by CutsceneController when the between-level cutscene ends.
    // nextSceneName is set in the Inspector on each CutsceneController.
    public void OnCutsceneComplete(string nextSceneName)
    {
        SetState(GameState.LevelPlaying);
        SceneManager.LoadScene(nextSceneName);
    }

    public void Restart()
    {
        NewGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        SetState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    // ─── Debug ────────────────────────────────────────────────────────────────

    public void DebugStartLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        roundNPCIndex = 0;
        ClueDatabaseManager.Instance.InitialisePlaythrough();
        SetState(GameState.LevelPlaying);
        WireLevel();
    }

    public void DebugStartConclusion()
    {
        ClueDatabaseManager.Instance.InitialisePlaythrough();
        SetState(GameState.DialogueSequence);
    }

    // ─── State Machine ────────────────────────────────────────────────────────

    public void SetState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    // ─── Scene Loading & Spatial Wiring ──────────────────────────────────────

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only wire up gameplay scenes
        if (currentLevelIndex < 1 || currentLevelIndex > 3) return;
        if (currentState != GameState.LevelPlaying) return;

        string[] levelSceneNames = { "Level1_Catacombs", "Level2_Gardens", "Level3_Corridors" };
        if (!System.Array.Exists(levelSceneNames, s => s == scene.name)) return;

        WireLevel();
    }

    void WireLevel()
    {
        if (levelDataArray == null || currentLevelIndex - 1 >= levelDataArray.Length)
        {
            Debug.LogError("levelDataArray not configured in GameManager!");
            return;
        }

        LevelData data = levelDataArray[currentLevelIndex - 1];

        // Spawn or reposition Siera
        GameObject spawnObj = GameObject.FindGameObjectWithTag(data.sieraSpawnPointTag);
        if (spawnObj == null)
        {
            Debug.LogError($"Spawn point tag '{data.sieraSpawnPointTag}' not found in scene!");
            return;
        }

        if (sieraInstance == null)
        {
            sieraInstance = Instantiate(sieraPrefab, spawnObj.transform.position, Quaternion.identity);
        }
        else
        {
            Rigidbody2D sieraRb = sieraInstance.GetComponent<Rigidbody2D>();
            if (sieraRb != null)
            {
                sieraRb.linearVelocity = Vector2.zero;
                sieraRb.position = spawnObj.transform.position;
            }

            sieraInstance.transform.position = spawnObj.transform.position;
            Physics2D.SyncTransforms();
        }

        // Assign combo IDs to hidden NPCs
        for (int i = 0; i < data.hiddenNPCSlotTags.Length; i++)
        {
            string npcTag = data.hiddenNPCSlotTags[i];
            GameObject npcObj = GameObject.FindGameObjectWithTag(npcTag);
            if (npcObj == null)
            {
                Debug.LogError($"Hidden NPC tag '{npcTag}' not found in scene!");
                continue;
            }

            EmberNPC npc = npcObj.GetComponent<EmberNPC>();
            if (npc == null)
            {
                Debug.LogError($"EmberNPC component missing on '{npcTag}'!");
                continue;
            }

            var combos = GameStateManager.Instance.GetSelectedCombos();
            string[] keys = new System.Collections.Generic.List<string>(combos.Keys).ToArray();
            if (roundNPCIndex < keys.Length)
            {
                npc.assignedComboID = keys[roundNPCIndex];
                roundNPCIndex++;
            }
        }

        // Wire NPCDialogue and HUDController to all EmberNPCs in the scene
        GameObject npcDialogueObj = GameObject.FindGameObjectWithTag("NPCDialogue");
        HUDController hudController = FindFirstObjectByType<HUDController>();
        ManaUI manaUI = FindFirstObjectByType<ManaUI>();
        EmberNPC[] emberNPCs = FindObjectsByType<EmberNPC>(FindObjectsSortMode.None);

        if (npcDialogueObj != null)
        {
            DialogueUI npcDialogueUI = npcDialogueObj.GetComponent<DialogueUI>();
            foreach (EmberNPC emberNPC in emberNPCs)
            {
                emberNPC.dialogueUI = npcDialogueUI;
                emberNPC.hudController = hudController;
                emberNPC.manaUI = manaUI;
            }
        }
        else
        {
            Debug.LogWarning("NPCDialogue tag not found in scene — EmberNPC dialogue will not work.");
        }

        // Wire enemy AI — confirm waypoints are assigned
        foreach (string enemyTag in data.enemyTags)
        {
            GameObject enemyObj = GameObject.FindGameObjectWithTag(enemyTag);
            if (enemyObj == null)
            {
                Debug.LogWarning($"Enemy tag '{enemyTag}' not found in scene.");
                continue;
            }

            EnemyPatrol enemy = enemyObj.GetComponent<EnemyPatrol>();
            if (enemy != null && enemy.waypointsParent == null)
                Debug.LogError($"Enemy '{enemyTag}' has no waypointsParent assigned!");

            // Give enemy a reference to Siera
            if (enemy != null && sieraInstance != null)
                enemy.sieraTransform = sieraInstance.transform;
        }

        // Register level exit trigger
        GameObject exitObj = GameObject.FindGameObjectWithTag(data.levelExitTriggerTag);
        if (exitObj != null)
        {
            LevelExitTrigger exitTrigger = exitObj.GetComponent<LevelExitTrigger>();
            if (exitTrigger != null)
            {
                exitTrigger.OnExitReached -= OnRoundComplete; // prevent double-subscribe
                exitTrigger.OnExitReached += OnRoundComplete;
            }
        }
        else
        {
            Debug.LogError($"Level exit tag '{data.levelExitTriggerTag}' not found!");
        }

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        Debug.Log($"CameraFollow found: {cam != null} in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        if (cam != null) cam.SetTarget(sieraInstance.transform);

        SetState(GameState.LevelPlaying);
    }

    // ─── Level Flow ───────────────────────────────────────────────────────────

    // Called by LevelExitTrigger when Siera reaches the exit.
    // One maze per level — exit means level is complete.
    public void OnRoundComplete()
    {
        OnLevelComplete();
    }

    public void OnLevelComplete()
    {
        SetState(GameState.LevelComplete);

        // Lives carry: +1 per level complete, capped at 5
        int newLives = GameStateManager.Instance.GetLives() + 1;
        GameStateManager.Instance.SetLives(Mathf.Min(newLives, 5));

        // Clean level bonus
        if (livesLostThisLevel == 0)
        {
            SuspicionManager.Instance.AddSuspicion(-5f);
        }

        // Reset for next level
        livesLostThisLevel = 0;
        currentLevelIndex++;

        if (currentLevelIndex > 3)
        {
            SceneManager.LoadScene("Conclusion");
        }
        else
        {
            // Route through cutscenes after levels 1 and 2
            string nextScene = currentLevelIndex switch
            {
                2 => "Cutscene1_AfterCatacombs",
                3 => "Cutscene2_AfterGardens",
                _ => "Level3_Corridors"
            };
            SceneManager.LoadScene(nextScene);
        }
    }

    // ─── Life Lost ────────────────────────────────────────────────────────────

    public void OnLifeLost()
    {
        int currentLives = GameStateManager.Instance.GetLives();
        currentLives--;
        livesLostThisLevel++;
        GameStateManager.Instance.SetLives(currentLives);

        OnPlayerCaught?.Invoke();

        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            RepositionSiera();
        }
    }

    void RepositionSiera()
    {
        if (sieraInstance == null || levelDataArray == null) return;

        LevelData data = levelDataArray[currentLevelIndex - 1];
        GameObject spawnObj = GameObject.FindGameObjectWithTag(data.sieraSpawnPointTag);
        if (spawnObj != null)
        {
            Rigidbody2D sieraRb = sieraInstance.GetComponent<Rigidbody2D>();
            if (sieraRb != null)
            {
                sieraRb.linearVelocity = Vector2.zero;  // stop drifting away from spawn
                sieraRb.position = spawnObj.transform.position;
            }
            sieraInstance.transform.position = spawnObj.transform.position;
            Physics2D.SyncTransforms(); // force physics collider to move immediately
        }
    }

    // ─── Terminal States ──────────────────────────────────────────────────────

    public GameOverContext LastGameOverContext { get; private set; }

    public void TriggerGameOver()
    {
        LastGameOverContext = currentLevelIndex switch
        {
            1 => GameOverContext.Level1,
            2 => GameOverContext.Level2,
            _ => GameOverContext.Level3
        };
        SetState(GameState.GameOver);
        SceneManager.LoadScene("GameOver");
    }

    public void TriggerCaughtAtGate()
    {
        LastGameOverContext = GameOverContext.HeartGate;
        SetState(GameState.CaughtAtGate);
        SceneManager.LoadScene("GameOver");
    }

    public void TriggerCaughtMidDialogue()
    {
        LastGameOverContext = GameOverContext.HeartGate;
        SetState(GameState.CaughtAtGate);
        SceneManager.LoadScene("GameOver");
    }

    public void TriggerWin()
    {
        SetState(GameState.Win);
        SceneManager.LoadScene("ConclusionCutscene");
    }

    public void OnConclusionCutsceneComplete()
    {
        SetState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }
}

