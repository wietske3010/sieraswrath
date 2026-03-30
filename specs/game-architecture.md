# Ember of the Last Witch - Game Architecture Specification

**Version:** 1.0  
**Engine:** Unity 6  
**Target Platform:** PC  
**Project Name:** SierasWrath  
**Development Timeline:** 4 weeks  
**Team:** Praise (Game Design + Unity Lead), Sarah (Narrative + Art), Wietske (Programming + Systems)

---

## ⚠️ CRITICAL IMPLEMENTATION NOTES

**These are common Unity gotchas that MUST be addressed during implementation:**

### 1. ScriptableObjects Cannot Store Scene References
**Issue:** ScriptableObjects are project assets and **cannot hold references to scene objects** (GameObjects, Transforms, Components). Any such references will be null at runtime.

**Solution Used:** This spec uses **string tags** in LevelData instead of direct references. During spatial wiring, we use `GameObject.FindGameObjectWithTag()` to locate scene objects. All GameObjects that need to be referenced must be tagged appropriately in each scene.

**Example:**
```csharp
// WRONG - This will be null at runtime
public Transform sieraSpawnPoint; 

// CORRECT - Use tags
public string sieraSpawnPointTag = "SieraSpawn_R1";

// Then in code:
GameObject spawn = GameObject.FindGameObjectWithTag(sieraSpawnPointTag);
```

### 2. OnTriggerEnter2D Parameter Confusion
**Issue:** The `other` parameter in `OnTriggerEnter2D(Collider2D other)` represents **the collider that entered YOUR trigger**, not your own colliders. You cannot compare `other == yourTrigger`.

**Solution Used:** This spec uses distance checks or separate child GameObjects with dedicated trigger handler scripts to differentiate between multiple triggers on the same NPC.

**Example:**
```csharp
// WRONG - comparing incoming collider to your own trigger reference
void OnTriggerEnter2D(Collider2D other)
{
    if (other == outerTrigger) // This will never be true!
}

// CORRECT - use distance or separate GameObjects
void OnTriggerEnter2D(Collider2D other)
{
    if (Vector2.Distance(other.transform.position, transform.position) <= outerRadius)
}
```

### 3. State Access Modifiers
**Issue:** If `GameManager.currentState` is declared `private`, other scripts cannot read it with `GameManager.Instance.currentState`.

**Solution Used:** This spec provides a public property `CurrentState` using C# property syntax:
```csharp
private GameState currentState;
public GameState CurrentState => currentState; // Read-only public access
```

---

## Table of Contents

1. [Project File Structure](#project-file-structure)
2. [Architecture Overview](#architecture-overview)
3. [Core Systems Implementation Roadmap](#core-systems-implementation-roadmap)
4. [System Details](#system-details)
5. [Data Structures](#data-structures)
6. [Integration Points](#integration-points)
7. [Implementation Sequence](#implementation-sequence)
8. [Configuration Reference](#configuration-reference)

---

## Project File Structure

### Complete Directory Structure

```
SierasWrath/
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Prologue.unity
│   │   ├── Level1_Catacombs.unity
│   │   ├── Level2_Gardens.unity
│   │   ├── Level3_Corridors.unity
│   │   ├── Conclusion.unity
│   │   ├── GameOver.unity
│   │   └── Victory.unity
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs
│   │   │   ├── GameState.cs
│   │   │   └── SceneTransitionController.cs
│   │   │
│   │   ├── Managers/
│   │   │   ├── SuspicionManager.cs
│   │   │   ├── GameStateManager.cs
│   │   │   └── ClueDatabaseManager.cs
│   │   │
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   └── PlayerAnimationController.cs
│   │   │
│   │   ├── Enemy/
│   │   │   ├── EnemyAI.cs
│   │   │   ├── EnemyDetection.cs
│   │   │   └── EnemyPatrol.cs
│   │   │
│   │   ├── NPC/
│   │   │   ├── EmberFlameNPC.cs
│   │   │   ├── VisibleNPC.cs
│   │   │   └── HiddenNPC.cs
│   │   │
│   │   ├── Dialogue/
│   │   │   ├── DialogueSystem.cs
│   │   │   ├── DialogueUI.cs
│   │   │   └── QuestionAnswerHandler.cs
│   │   │
│   │   ├── Data/
│   │   │   ├── LevelData.cs
│   │   │   ├── ClueDatabase.cs
│   │   │   ├── NarrativeSchedule.cs
│   │   │   ├── ClueCombo.cs
│   │   │   └── DialogueEntry.cs
│   │   │
│   │   └── UI/
│   │       ├── HUDController.cs
│   │       ├── LivesDisplay.cs
│   │       ├── SuspicionBarUI.cs
│   │       ├── ClueInventoryUI.cs
│   │       └── MenuController.cs
│   │
│   ├── Prefabs/
│   │   ├── Player/
│   │   │   └── Siera.prefab
│   │   │
│   │   ├── Enemies/
│   │   │   ├── CatacombCreature.prefab
│   │   │   ├── GardenWolf.prefab
│   │   │   └── CastleGuard.prefab
│   │   │
│   │   ├── NPCs/
│   │   │   ├── VisibleEmberNPC.prefab
│   │   │   └── HiddenEmberNPC.prefab
│   │   │
│   │   ├── UI/
│   │   │   ├── HUD.prefab
│   │   │   ├── DialogueBox.prefab
│   │   │   ├── PauseMenu.prefab
│   │   │   └── FadeCanvas.prefab
│   │   │
│   │   └── Environment/
│   │       └── LevelExitTrigger.prefab
│   │
│   ├── ScriptableObjects/
│   │   ├── LevelData/
│   │   │   ├── LevelData_1.asset
│   │   │   ├── LevelData_2.asset
│   │   │   └── LevelData_3.asset
│   │   │
│   │   ├── ClueDatabase.asset
│   │   └── NarrativeSchedule.asset
│   │
│   ├── Art/
│   │   ├── Sprites/
│   │   │   ├── Characters/
│   │   │   │   ├── Siera/
│   │   │   │   └── Enemies/
│   │   │   │
│   │   │   ├── Environment/
│   │   │   │   ├── Catacombs/
│   │   │   │   ├── Gardens/
│   │   │   │   └── Corridors/
│   │   │   │
│   │   │   └── UI/
│   │   │       ├── Icons/
│   │   │       └── Panels/
│   │   │
│   │   ├── Tilesets/
│   │   │   ├── Catacombs_Tileset.png
│   │   │   ├── Gardens_Tileset.png
│   │   │   └── Corridors_Tileset.png
│   │   │
│   │   ├── TilePalettes/
│   │   │   ├── Catacombs_Palette.prefab
│   │   │   ├── Gardens_Palette.prefab
│   │   │   └── Corridors_Palette.prefab
│   │   │
│   │   └── Animations/
│   │       ├── Siera/
│   │       └── Enemies/
│   │
│   ├── Audio/
│   │   ├── Music/
│   │   │   ├── Catacombs_Ambient.mp3
│   │   │   ├── Gardens_Ambient.mp3
│   │   │   ├── Corridors_Ambient.mp3
│   │   │   ├── Prologue_Theme.mp3
│   │   │   └── Victory_Theme.mp3
│   │   │
│   │   └── SFX/
│   │       ├── EnemyDetection.wav
│   │       ├── LifeLost.wav
│   │       ├── ClueCollected.wav
│   │       ├── SuspicionFill.wav
│   │       ├── CorrectAnswer.wav
│   │       ├── WrongAnswer.wav
│   │       ├── GameOver.wav
│   │       ├── Victory.wav
│   │       ├── Footsteps_Stone.wav
│   │       └── Footsteps_Grass.wav
│   │
│   ├── InputSystem_Actions.inputactions (already exists)
│   ├── Settings/ (already exists)
│   ├── DefaultVolumeProfile.asset (already exists)
│   └── UniversalRenderPipelineGlobalSettings.asset (already exists)
│
├── Packages/
│   ├── manifest.json (already exists)
│   └── packages-lock.json (already exists)
│
└── ProjectSettings/ (already exists)
```

---

## Architecture Overview

### System Hierarchy

```
┌─────────────────────────────────────────────────┐
│              GAME MANAGER                       │
│  (Persistent Singleton - DontDestroyOnLoad)     │
│  - State Machine                                │
│  - Lifecycle Management                         │
│  - Spatial Wiring                               │
│  - Scene Loading                                │
└──────────────────┬──────────────────────────────┘
                   │
        ┌──────────┴──────────┐
        │                     │
┌───────▼────────┐    ┌──────▼──────────────────┐
│ CORE SYSTEMS   │    │ PERSISTENT SINGLETONS   │
│ (Active)       │    │ (Session Data)          │
├────────────────┤    ├─────────────────────────┤
│ PlayerController│    │ SuspicionManager       │
│ Enemy AI       │    │ GameStateManager        │
│                │    │ ClueDatabaseManager     │
└────────────────┘    └─────────────────────────┘
        │
        │
┌───────▼────────────────────────────────────────┐
│         CONTENT & NARRATIVE SYSTEMS            │
│  - Clue/Dictionary System                      │
│  - Dialogue System                             │
│  - Ember Flame NPC System                      │
│  - Maze/LevelData System                       │
└────────────────────────────────────────────────┘
        │
        │
┌───────▼────────────────────────────────────────┐
│         UNITY ENGINE SYSTEMS                   │
│  - Physics2D                                   │
│  - Tilemap                                     │
│  - Input System                                │
│  - UI Toolkit                                  │
│  - SceneManager                                │
└────────────────────────────────────────────────┘
```

### State Machine Flow

```
MainMenu
    │
    ├──[New Game]──> Prologue
    │                    │
    │                    └──> Level 1 (LevelPlaying)
    │                            │
    │                            ├──> Round 1 Complete (RoundComplete)
    │                            │       │
    │                            │       └──> Round 2 (LevelPlaying)
    │                            │               │
    │                            │               └──> Level Complete
    │                            │                       │
    │                            │                       └──> Level 2 (repeat pattern)
    │                            │                               │
    │                            │                               └──> Level 3
    │                            │                                       │
    │                            │                                       └──> Heart Gate Check
    │                            │                                               │
    │                            │                           ┌───────────────────┴───────────────────┐
    │                            │                           │                                       │
    │                            │                   [Suspicion < 100%]                   [Suspicion >= 100%]
    │                            │                           │                                       │
    │                            │                   DialogueSequence                        CaughtAtGate
    │                            │                           │                                       │
    │                            │                   ┌───────┴───────┐                              │
    │                            │                   │               │                              │
    │                            │          [All Q correct]  [Wrong answer hits 100%]               │
    │                            │                   │               │                              │
    │                            │                  Win          GameOver <──────────────────────────┘
    │                            │                                   │
    │                            └──[Lives = 0]──> GameOver <────────┘
    │
    └──[Quit]──> Exit
```

---

## Core Systems Implementation Roadmap

### Phase 1: Foundation (Week 1)

**Goal:** Establish core architecture, Singletons, and basic scene structure

#### 1.1 Game State Enum & Game Manager
- Create `GameState.cs` enum with all 10 states
- Implement `GameManager.cs` as persistent Singleton
- Add DontDestroyOnLoad functionality
- Implement basic state machine with state change methods

#### 1.2 Persistent Singletons
- Implement `SuspicionManager.cs`
  - AddSuspicion(float amount)
  - GetSuspicion()
  - Reset()
- Implement `GameStateManager.cs`
  - Lives tracking
  - Collected clues list
  - Selected combos dictionary
- Implement `ClueDatabaseManager.cs`
  - Reference to ClueDatabase ScriptableObject
  - InitialisePlaythrough() method

#### 1.3 Scene Setup
- Create all 8 scene files (MainMenu through Victory)
- Add GameManager GameObject to MainMenu scene
- Test scene persistence across loads

### Phase 2: Player & Input (Week 1)

**Goal:** Get Siera moving with proper input handling

#### 2.1 Input System Configuration
- Open `InputSystem_Actions.inputactions`
- Create Action Map: "Player"
- Create Action: "Move" (Value, Vector2)
- Add bindings: WASD, Arrow Keys, Gamepad Left Stick

#### 2.2 Player Controller
- Implement `PlayerController.cs`
  - Rigidbody2D velocity-based movement
  - New Input System integration
  - State-gated input (only in LevelPlaying)
  - Slight ease-in on movement start
  - Instant stop on input release
- Create Siera prefab
  - Add Rigidbody2D (Gravity Scale = 0, Interpolate = Interpolate)
  - Add CapsuleCollider2D
  - Add PlayerController component
  - Assign to Player layer

### Phase 3: Maze Architecture (Week 2)

**Goal:** Build tile-based maze system with collision

#### 3.1 Tilemap Setup
- Import first tileset (Catacombs_Tileset.png)
  - Texture Type: Sprite (2D and UI)
  - Sprite Mode: Multiple
  - Pixels Per Unit: 16
  - Filter Mode: Point (no filter)
- Slice sprites: Grid By Cell Size (16x16)
- Create Tile Palette for Catacombs

#### 3.2 Level 1 Scene Construction
- In Level1_Catacombs scene, create Grid GameObject
- Create 4 child Tilemap GameObjects:
  - GroundLayer (Sorting Layer: Ground, Order: 0)
  - WallLayer (Sorting Layer: Walls, Order: 1)
  - DecorationLayer (Sorting Layer: Props, Order: 2)
  - OverheadLayer (Sorting Layer: Ceiling, Order: 3)
- Add to WallLayer only:
  - TilemapCollider2D
  - CompositeCollider2D
- Paint basic test maze

#### 3.3 Physics Layers Setup
- Create Physics2D Layers:
  - Player
  - Enemy
  - SafeZone
- Configure Layer Collision Matrix:
  - Enemy cannot collide with SafeZone
  - Player can collide with SafeZone

### Phase 4: Enemy AI (Week 2)

**Goal:** Implement patrol and detection systems

#### 4.1 Enemy Base Class
- Implement `EnemyAI.cs`
  - Waypoint patrol logic
  - Vision cone parameters (range, angle)
  - Two-stage detection system
  - State-gated behaviour (only patrol in LevelPlaying)

#### 4.2 Detection System
- Implement `EnemyDetection.cs`
  - Vision cone angle check (dot product)
  - Raycast confirmation
  - Detection indicator UI element
  - Fill timer logic
  - Direct call to SuspicionManager on confirmation

#### 4.3 Enemy Prefabs
- Create CatacombCreature prefab
  - Add Rigidbody2D (Kinematic)
  - Add CircleCollider2D
  - Add EnemyAI component
  - Add detection indicator sprite
  - Assign to Enemy layer

### Phase 5: Data Structures (Week 2-3)

**Goal:** Create all ScriptableObject data containers

#### 5.1 ScriptableObject Scripts
- Implement `LevelData.cs`
  ```csharp
  [CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
  public class LevelData : ScriptableObject
  {
      public int levelIndex;
      public int roundCount = 2;
      
      // Use string tags instead of direct Transform references (scene objects can't be stored in ScriptableObjects)
      public string sieraSpawnPointRound1Tag = "SieraSpawn_R1";
      public string sieraSpawnPointRound2Tag = "SieraSpawn_R2";
      public string levelExitTriggerTag = "LevelExit";
      public string[] hiddenNPCSlotTags = new string[2] { "HiddenNPC_R1", "HiddenNPC_R2" };
      public string[] visibleNPCTags = new string[2] { "VisibleNPC_R1", "VisibleNPC_R2" };
      public string[] enemyTags; // e.g., { "Enemy_1", "Enemy_2", "Enemy_3" }
  }
  ```

- Implement `ClueDatabase.cs`
  ```csharp
  [System.Serializable]
  public class ClueCombo
  {
      public string comboID;
      public string questionText;
      public string clueText;
      public string correctAnswer;
  }

  [CreateAssetMenu(fileName = "ClueDatabase", menuName = "Game/Clue Database")]
  public class ClueDatabase : ScriptableObject
  {
      public ClueCombo[] allCombos; // Length = 10
  }
  ```

- Implement `NarrativeSchedule.cs`
  ```csharp
  [System.Serializable]
  public class DialogueEntry
  {
      public int roundIndex; // 1-6
      public string speakerName;
      public string[] dialogueLines;
  }

  [CreateAssetMenu(fileName = "NarrativeSchedule", menuName = "Game/Narrative Schedule")]
  public class NarrativeSchedule : ScriptableObject
  {
      public DialogueEntry[] entries; // Length = 6
  }
  ```

#### 5.2 Create Asset Instances
- Create `ClueDatabase.asset` (Sarah will populate)
- Create `NarrativeSchedule.asset` (Sarah will populate)
- Create `LevelData_1.asset`, `LevelData_2.asset`, `LevelData_3.asset`

### Phase 6: Spatial Wiring (Week 3)

**Goal:** Connect Game Manager to level scenes

#### 6.1 Level Scene Setup
- In each level scene, create empty GameObjects:
  - SieraSpawnPoint_Round1
  - SieraSpawnPoint_Round2
  - LevelExitTrigger (with BoxCollider2D, isTrigger = true)
  - HiddenNPC_Slot1, HiddenNPC_Slot2
  - VisibleNPC_Slot1, VisibleNPC_Slot2
  - Enemy waypoint paths (empty GameObjects)

#### 6.2 LevelData Population
- In Inspector, assign all scene references to LevelData assets
- Drag enemy instances to enemyInstances array
- Assign waypoint transforms to each enemy's waypoint list

#### 6.3 Game Manager Spatial Wiring
- Implement wiring sequence in GameManager:
  ```csharp
  void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
      LevelData currentLevelData = levelDataArray[currentLevelIndex - 1];
      
      // Find scene objects by tag (ScriptableObjects can't store scene references)
      Transform sieraSpawn = GameObject.FindGameObjectWithTag(
          currentRound == 1 ? currentLevelData.sieraSpawnPointRound1Tag : currentLevelData.sieraSpawnPointRound2Tag
      ).transform;
      
      // Position Siera
      siera.transform.position = sieraSpawn.position;
      
      // Assign combo IDs to hidden NPCs
      for (int i = 0; i < currentLevelData.hiddenNPCSlotTags.Length; i++)
      {
          GameObject npcObj = GameObject.FindGameObjectWithTag(currentLevelData.hiddenNPCSlotTags[i]);
          HiddenNPC npc = npcObj.GetComponent<HiddenNPC>();
          npc.assignedComboID = selectedCombos[roundNPCIndex];
          roundNPCIndex++;
      }
      
      // Register exit trigger
      GameObject exitTrigger = GameObject.FindGameObjectWithTag(currentLevelData.levelExitTriggerTag);
      exitTrigger.GetComponent<LevelExitTrigger>().OnExitReached += HandleLevelComplete;
      
      // Confirm enemy waypoints
      foreach (string enemyTag in currentLevelData.enemyTags)
      {
          GameObject enemyObj = GameObject.FindGameObjectWithTag(enemyTag);
          EnemyAI enemy = enemyObj.GetComponent<EnemyAI>();
          
          if (enemy.waypoints.Length == 0)
              Debug.LogError($"Enemy {enemyTag} has no waypoints assigned!");
      }
      
      // Transition to LevelPlaying
      SetState(GameState.LevelPlaying);
  }
  ```

### Phase 7: NPC System (Week 3)

**Goal:** Implement both NPC types and dialogue UI

#### 7.1 Dialogue UI
- Implement `DialogueUI.cs`
  - Bottom-screen cinematic bar
  - Avatar portrait positioning (left/right)
  - Text display with advance-on-button
  - Reusable across all dialogue contexts

#### 7.2 Hidden NPC Implementation
- Implement `HiddenNPC.cs`
  ```csharp
  public class HiddenNPC : MonoBehaviour
  {
      public string assignedComboID; // Set by Game Manager during wiring
      
      private SpriteRenderer spriteRenderer;
      private CircleCollider2D outerTrigger; // Discovery radius
      private CircleCollider2D innerTrigger; // Interaction radius
      private GameObject interactPrompt;
      
      void OnTriggerEnter2D(Collider2D other)
      {
          if (other.CompareTag("Player"))
          {
              if (IsOuterTrigger(other))
                  spriteRenderer.enabled = true; // Reveal NPC
              else if (IsInnerTrigger(other))
                  interactPrompt.SetActive(true); // Show prompt
          }
      }
      
      public void OnInteract()
      {
          DialogueUI.Instance.StartDialogue(GetClueDialogue());
          DialogueUI.Instance.OnDialogueComplete += HandleDialogueComplete;
      }
      
      void HandleDialogueComplete()
      {
          GameStateManager.Instance.CollectClue(assignedComboID);
      }
  }
  ```

#### 7.3 Visible NPC Implementation
- Implement `VisibleNPC.cs`
  - Triggered by Game Manager on RoundComplete state
  - Displays dialogue from NarrativeSchedule
  - Callback to Game Manager on dialogue dismissed

### Phase 8: Clue/Dictionary System (Week 3)

**Goal:** Random combo selection and tracking

#### 8.1 Playthrough Initialization
- In `ClueDatabaseManager.cs`:
  ```csharp
  public void InitialisePlaythrough()
  {
      // Shuffle database
      ClueCombo[] shuffled = database.allCombos.OrderBy(x => Random.value).ToArray();
      
      // Select first 6
      ClueCombo[] selected = shuffled.Take(6).ToArray();
      
      // Store in GameStateManager
      Dictionary<string, string> combos = new Dictionary<string, string>();
      foreach (ClueCombo combo in selected)
      {
          combos.Add(combo.comboID, combo.correctAnswer);
      }
      
      GameStateManager.Instance.SetSelectedCombos(combos);
  }
  ```

#### 8.2 Integration with Game Manager
- Call `InitialisePlaythrough()` in Game Manager's NewGame() and Restart() methods
- Before Level 1 scene loads

### Phase 9: Dialogue System & Conclusion (Week 4)

**Goal:** Heart Gate interrogation scene

#### 9.1 Pre-Dialogue Gate Check
- Implement in `DialogueSystem.cs`:
  ```csharp
  void Start()
  {
      float currentSuspicion = SuspicionManager.Instance.GetSuspicion();
      
      if (currentSuspicion >= 100f)
      {
          GameManager.Instance.TriggerCaughtAtGate();
          return;
      }
      
      StartDialogueSequence();
  }
  ```

#### 9.2 Question-Answer Flow
- Implement `QuestionAnswerHandler.cs`
  ```csharp
  void PresentQuestion(int questionIndex)
  {
      var combos = GameStateManager.Instance.GetSelectedCombos();
      var collected = GameStateManager.Instance.GetCollectedClues();
      
      string comboID = combos.Keys.ElementAt(questionIndex);
      string correctAnswer = combos[comboID];
      
      // Generate 3 options (1 correct + 2 decoys)
      string[] options = GenerateOptions(correctAnswer, comboID);
      
      // Highlight if clue collected
      bool highlight = collected.Contains(comboID);
      
      DialogueUI.Instance.PresentMultipleChoice(
          GetQuestionText(comboID),
          options,
          highlight ? correctAnswer : null,
          OnAnswerSelected
      );
  }
  
  void OnAnswerSelected(string answer)
  {
      if (answer != correctAnswer)
      {
          SuspicionManager.Instance.AddSuspicion(15f);
          
          if (SuspicionManager.Instance.GetSuspicion() >= 100f)
          {
              GameManager.Instance.TriggerCaughtMidDialogue();
              return;
          }
      }
      
      currentQuestionIndex++;
      
      if (currentQuestionIndex >= 6)
          GameManager.Instance.TriggerWin();
      else
          PresentQuestion(currentQuestionIndex);
  }
  ```

### Phase 10: Scene Transitions & UI (Week 4)

**Goal:** Fade transitions, HUD, menus

#### 10.1 Fade Transition
- Create FadeCanvas prefab with full-screen black Image
- Implement `SceneTransitionController.cs`
  ```csharp
  public IEnumerator FadeAndLoadScene(string sceneName)
  {
      GameManager.Instance.SetState(GameState.SceneTransition);
      
      // Fade to black
      yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
      
      // Load scene
      SceneManager.LoadScene(sceneName);
      
      // Wait for spatial wiring
      yield return null;
      
      // Fade from black
      yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
  }
  ```

#### 10.2 HUD Implementation
- Implement `HUDController.cs`
  - Lives display (ember flame icons)
  - Suspicion bar (color-coded: yellow → orange → red)
  - Clue counter (drawstring bag icon)
- Create HUD prefab
- Instantiate in each level scene

#### 10.3 Menu Screens
- Implement `MenuController.cs` for MainMenu, GameOver, Victory
- Wire up button callbacks to Game Manager

---

## System Details

### 1. Game Manager

**File:** `Scripts/Core/GameManager.cs`

**Responsibilities:**
- Maintain game state machine (10 states)
- Manage session lifecycle (New Game, Restart, Quit)
- Perform spatial wiring on scene load
- Handle scene transitions with fade
- Coordinate all major systems

**Key Methods:**
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Level Configuration")]
    public LevelData[] levelDataArray; // Length = 3, assigned in Inspector
    public GameObject sieraPrefab;
    
    [Header("Scene References")]
    public SceneTransitionController sceneTransition;
    
    private GameState currentState;
    private int currentLevelIndex = 1; // 1-3
    private int currentRound = 1; // 1-2
    
    // Public getter for state - other systems need to check this
    public GameState CurrentState => currentState;
    
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
    
    public void NewGame()
    {
        // Reset all Singletons
        SuspicionManager.Instance.Reset();
        GameStateManager.Instance.Reset();
        
        // Initialize playthrough
        ClueDatabaseManager.Instance.InitialisePlaythrough();
        
        // Reset indices
        currentLevelIndex = 1;
        currentRound = 1;
        
        // Load prologue
        SetState(GameState.Prologue);
        sceneTransition.FadeAndLoadScene("Prologue");
    }
    
    public void Restart()
    {
        NewGame(); // Identical to New Game
    }
    
    public void SetState(GameState newState)
    {
        currentState = newState;
        
        // Notify systems of state change if needed
        OnStateChanged?.Invoke(newState);
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Spatial wiring sequence (detailed in Phase 6)
    }
    
    public void OnRoundComplete()
    {
        SetState(GameState.RoundComplete);
        
        // Trigger visible NPC from NarrativeSchedule
        int narrativeIndex = ((currentLevelIndex - 1) * 2) + currentRound - 1;
        DialogueEntry entry = narrativeSchedule.entries[narrativeIndex];
        
        // Display dialogue, wait for dismissal
        VisibleNPC npc = GetVisibleNPCForCurrentRound();
        npc.ShowDialogue(entry, OnVisibleNPCDialogueDismissed);
    }
    
    void OnVisibleNPCDialogueDismissed()
    {
        currentRound++;
        
        if (currentRound > 2)
        {
            OnLevelComplete();
        }
        else
        {
            // Reposition Siera to round 2 spawn
            RepositionSieraForRound2();
            SetState(GameState.LevelPlaying);
        }
    }
    
    public void OnLevelComplete()
    {
        SetState(GameState.LevelComplete);
        
        // Calculate lives carry
        int newLives = GameStateManager.Instance.GetLives() + 1;
        GameStateManager.Instance.SetLives(Mathf.Min(newLives, 5));
        
        // Check clean level bonus
        if (livesLostThisLevel == 0)
        {
            SuspicionManager.Instance.AddSuspicion(-5f);
        }
        
        // Reset round counter
        currentRound = 1;
        livesLostThisLevel = 0;
        
        // Load next level
        currentLevelIndex++;
        
        if (currentLevelIndex > 3)
        {
            // All levels complete, load conclusion
            sceneTransition.FadeAndLoadScene("Conclusion");
        }
        else
        {
            string sceneName = $"Level{currentLevelIndex}_" + GetLevelName(currentLevelIndex);
            sceneTransition.FadeAndLoadScene(sceneName);
        }
    }
    
    public void OnLifeLost()
    {
        int currentLives = GameStateManager.Instance.GetLives();
        currentLives--;
        livesLostThisLevel++;
        
        GameStateManager.Instance.SetLives(currentLives);
        
        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            // Reposition Siera to current round spawn
            RepositionSieraForCurrentRound();
        }
    }
    
    public void TriggerGameOver()
    {
        SetState(GameState.GameOver);
        sceneTransition.FadeAndLoadScene("GameOver");
    }
    
    public void TriggerCaughtAtGate()
    {
        SetState(GameState.CaughtAtGate);
        sceneTransition.FadeAndLoadScene("GameOver"); // Same screen, different context
    }
    
    public void TriggerWin()
    {
        SetState(GameState.Win);
        sceneTransition.FadeAndLoadScene("Victory");
    }
}
```

**State Transitions:**
- Only Game Manager calls `SetState()`
- All other systems query `currentState` and gate behaviour accordingly

---

### 2. Suspicion Manager

**File:** `Scripts/Managers/SuspicionManager.cs`

**Responsibilities:**
- Track cumulative suspicion (0-100%)
- Provide suspicion modification methods
- Persist across scene loads

**Implementation:**
```csharp
public class SuspicionManager : MonoBehaviour
{
    public static SuspicionManager Instance;
    
    private float suspicion = 0f; // 0-100
    
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
    
    public void AddSuspicion(float amount)
    {
        suspicion += amount;
        suspicion = Mathf.Clamp(suspicion, 0f, 100f);
        
        // Notify UI
        OnSuspicionChanged?.Invoke(suspicion);
    }
    
    public float GetSuspicion()
    {
        return suspicion;
    }
    
    public void Reset()
    {
        suspicion = 0f;
        OnSuspicionChanged?.Invoke(suspicion);
    }
    
    // Event for UI updates
    public event System.Action<float> OnSuspicionChanged;
}
```

**Key Values:**
- Life lost in maze: +12%
- Wrong dialogue answer: +15%
- Clean level bonus: -5%
- Threshold: 100%

---

### 3. Game State Manager

**File:** `Scripts/Managers/GameStateManager.cs`

**Responsibilities:**
- Track lives (1-5)
- Track collected clues
- Store selected combo IDs for playthrough
- Persist across scene loads

**Implementation:**
```csharp
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;
    
    private int lives = 5;
    private List<string> collectedClues = new List<string>();
    private Dictionary<string, string> selectedCombos = new Dictionary<string, string>(); // comboID -> correctAnswer
    
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
    
    public int GetLives() => lives;
    
    public void SetLives(int newLives)
    {
        lives = Mathf.Clamp(newLives, 0, 5);
        OnLivesChanged?.Invoke(lives);
    }
    
    public void CollectClue(string comboID)
    {
        if (!collectedClues.Contains(comboID))
        {
            collectedClues.Add(comboID);
            OnClueCollected?.Invoke(comboID);
        }
    }
    
    public List<string> GetCollectedClues() => new List<string>(collectedClues);
    
    public void SetSelectedCombos(Dictionary<string, string> combos)
    {
        selectedCombos = new Dictionary<string, string>(combos);
    }
    
    public Dictionary<string, string> GetSelectedCombos() => new Dictionary<string, string>(selectedCombos);
    
    public void Reset()
    {
        lives = 5;
        collectedClues.Clear();
        selectedCombos.Clear();
        
        OnLivesChanged?.Invoke(lives);
    }
    
    // Events
    public event System.Action<int> OnLivesChanged;
    public event System.Action<string> OnClueCollected;
}
```

---

### 4. Player Controller

**File:** `Scripts/Player/PlayerController.cs`

**Responsibilities:**
- Handle WASD/Arrow/Gamepad input via New Input System
- Apply velocity-based movement with Rigidbody2D
- Slight ease-in on start, instant stop
- Gate input based on game state

**Implementation:**
```csharp
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float easeInDuration = 0.1f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private PlayerInput playerInput;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
    }
    
    void Update()
    {
        // Gate input based on state
        if (GameManager.Instance.CurrentState != GameState.LevelPlaying)
        {
            moveInput = Vector2.zero;
            return;
        }
        
        // Read from Input Action
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
    }
    
    void FixedUpdate()
    {
        // Apply velocity with ease-in
        Vector2 targetVelocity = moveInput.normalized * moveSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, easeInDuration);
        
        // Update animator
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }
}
```

**Physics Setup:**
- Rigidbody2D: Body Type = Dynamic, Gravity Scale = 0, Interpolate = Interpolate
- CapsuleCollider2D: Auto-fitted to sprite
- Layer: Player

---

### 5. Enemy AI

**File:** `Scripts/Enemy/EnemyAI.cs`

**Responsibilities:**
- Waypoint-based patrol
- Vision cone detection with raycast confirmation
- Two-stage detection (suspicion indicator → confirmed)
- Direct call to SuspicionManager on detection

**Implementation:**
```csharp
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waypointPauseDuration = 1f;
    
    [Header("Detection")]
    public float visionRange = 5f;
    public float visionAngle = 90f;
    public float detectionFillTime = 2f;
    
    [Header("References")]
    public GameObject detectionIndicator;
    public Transform sieraTransform;
    
    private int currentWaypointIndex = 0;
    private float detectionProgress = 0f;
    private bool isDetecting = false;
    
    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.LevelPlaying)
            return;
        
        Patrol();
        CheckForPlayer();
    }
    
    void Patrol()
    {
        if (waypoints.Length == 0) return;
        
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetWaypoint.position,
            moveSpeed * Time.deltaTime
        );
        
        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            // Implement pause at waypoint if needed
        }
    }
    
    void CheckForPlayer()
    {
        Vector2 dirToSiera = (sieraTransform.position - transform.position).normalized;
        float angleToSiera = Vector2.Angle(transform.up, dirToSiera);
        float distToSiera = Vector2.Distance(transform.position, sieraTransform.position);
        
        bool inVisionCone = angleToSiera < visionAngle / 2 && distToSiera < visionRange;
        
        if (inVisionCone)
        {
            // Raycast confirmation
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                dirToSiera,
                distToSiera,
                LayerMask.GetMask("Player", "Walls")
            );
            
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                // Start detection
                if (!isDetecting)
                {
                    isDetecting = true;
                    detectionIndicator.SetActive(true);
                }
                
                // Fill detection bar
                detectionProgress += Time.deltaTime / detectionFillTime;
                UpdateDetectionIndicator(detectionProgress);
                
                if (detectionProgress >= 1f)
                {
                    OnDetectionConfirmed();
                }
            }
            else
            {
                // Wall blocking LOS
                ResetDetection();
            }
        }
        else
        {
            // Out of vision cone
            ResetDetection();
        }
    }
    
    void ResetDetection()
    {
        if (isDetecting)
        {
            isDetecting = false;
            detectionProgress = 0f;
            detectionIndicator.SetActive(false);
        }
    }
    
    void OnDetectionConfirmed()
    {
        // Direct call to SuspicionManager (bypasses Game Manager)
        SuspicionManager.Instance.AddSuspicion(12f);
        
        // Notify Game Manager of life lost
        GameManager.Instance.OnLifeLost();
        
        // Reset detection
        ResetDetection();
    }
    
    void UpdateDetectionIndicator(float progress)
    {
        // Update fill bar UI element
        // Implementation depends on UI system
    }
}
```

**Prefab Variants:**
- CatacombCreature.prefab (default parameters)
- GardenWolf.prefab (tuned: higher moveSpeed, wider visionAngle)
- CastleGuard.prefab (tuned: slower moveSpeed, longer visionRange)

---

### 6. Clue/Dictionary System

**File:** `Scripts/Managers/ClueDatabaseManager.cs`

**Responsibilities:**
- Store reference to ClueDatabase ScriptableObject
- Randomly select 6 combos per playthrough
- Store selections in GameStateManager

**Implementation:**
```csharp
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
            Debug.LogError("ClueDatabase not properly configured!");
            return;
        }
        
        // Shuffle and select 6
        ClueCombo[] shuffled = database.allCombos.OrderBy(x => Random.value).ToArray();
        ClueCombo[] selected = shuffled.Take(6).ToArray();
        
        // Convert to dictionary
        Dictionary<string, string> combos = new Dictionary<string, string>();
        foreach (ClueCombo combo in selected)
        {
            combos.Add(combo.comboID, combo.correctAnswer);
        }
        
        // Store in GameStateManager
        GameStateManager.Instance.SetSelectedCombos(combos);
        
        Debug.Log($"Initialized playthrough with {selected.Length} clues");
    }
    
    public ClueCombo GetComboByID(string comboID)
    {
        return database.allCombos.FirstOrDefault(c => c.comboID == comboID);
    }
}
```

**ClueDatabase.asset Structure:**
Sarah will populate this with 10 entries. Each entry contains:
- comboID (e.g., "CATA_01", "GARD_02", etc.)
- questionText (displayed at Heart Gate)
- clueText (displayed when hidden NPC found)
- correctAnswer (string to match against)

---

### 7. Dialogue System

**File:** `Scripts/Dialogue/DialogueSystem.cs`

**Responsibilities:**
- Pre-dialogue suspicion gate check
- Present 6 questions sequentially
- Handle answer validation
- Apply +15% suspicion on wrong answers
- Check threshold after each wrong answer
- Trigger win/loss states

**Implementation:**
```csharp
public class DialogueSystem : MonoBehaviour
{
    [Header("UI")]
    public DialogueUI dialogueUI;
    public QuestionAnswerHandler qaHandler;
    
    private int currentQuestionIndex = 0;
    private Dictionary<string, string> selectedCombos;
    private List<string> collectedClues;
    
    void Start()
    {
        // Pre-dialogue gate check
        float currentSuspicion = SuspicionManager.Instance.GetSuspicion();
        
        if (currentSuspicion >= 100f)
        {
            GameManager.Instance.TriggerCaughtAtGate();
            return;
        }
        
        // Load playthrough data
        selectedCombos = GameStateManager.Instance.GetSelectedCombos();
        collectedClues = GameStateManager.Instance.GetCollectedClues();
        
        // Start sequence
        GameManager.Instance.SetState(GameState.DialogueSequence);
        PresentQuestion(0);
    }
    
    void PresentQuestion(int index)
    {
        if (index >= 6)
        {
            // All questions answered successfully
            GameManager.Instance.TriggerWin();
            return;
        }
        
        string comboID = selectedCombos.Keys.ElementAt(index);
        string correctAnswer = selectedCombos[comboID];
        
        // Get question text
        ClueCombo combo = ClueDatabaseManager.Instance.GetComboByID(comboID);
        string questionText = combo.questionText;
        
        // Generate 3 options
        string[] options = qaHandler.GenerateOptions(correctAnswer, comboID);
        
        // Check if clue collected
        bool highlight = collectedClues.Contains(comboID);
        
        // Present via UI
        dialogueUI.PresentMultipleChoice(
            questionText,
            options,
            highlight ? correctAnswer : null,
            OnAnswerSelected
        );
    }
    
    void OnAnswerSelected(string selectedAnswer)
    {
        string comboID = selectedCombos.Keys.ElementAt(currentQuestionIndex);
        string correctAnswer = selectedCombos[comboID];
        
        if (selectedAnswer != correctAnswer)
        {
            // Wrong answer
            SuspicionManager.Instance.AddSuspicion(15f);
            
            // Check threshold
            if (SuspicionManager.Instance.GetSuspicion() >= 100f)
            {
                GameManager.Instance.TriggerCaughtMidDialogue();
                return;
            }
        }
        
        // Proceed to next question
        currentQuestionIndex++;
        PresentQuestion(currentQuestionIndex);
    }
}
```

**File:** `Scripts/Dialogue/QuestionAnswerHandler.cs`

```csharp
public class QuestionAnswerHandler : MonoBehaviour
{
    public string[] GenerateOptions(string correctAnswer, string excludeComboID)
    {
        ClueDatabase db = ClueDatabaseManager.Instance.database;
        
        // Get 2 random incorrect answers from different combos
        List<string> incorrectAnswers = db.allCombos
            .Where(c => c.comboID != excludeComboID && c.correctAnswer != correctAnswer)
            .OrderBy(x => Random.value)
            .Take(2)
            .Select(c => c.correctAnswer)
            .ToList();
        
        // Combine and shuffle
        List<string> allOptions = new List<string> { correctAnswer };
        allOptions.AddRange(incorrectAnswers);
        
        return allOptions.OrderBy(x => Random.value).ToArray();
    }
}
```

---

### 8. NPC System

**File:** `Scripts/NPC/HiddenNPC.cs`

**Responsibilities:**
- Two-radius trigger system (discovery + interaction)
- Reveal on discovery
- Show interact prompt on inner radius
- Trigger dialogue exchange
- Register clue collection

**Implementation:**
```csharp
public class HiddenNPC : MonoBehaviour
{
    [Header("Assignment")]
    public string assignedComboID; // Set by Game Manager during spatial wiring
    
    [Header("Triggers")]
    public CircleCollider2D outerTrigger; // Discovery radius
    public CircleCollider2D innerTrigger; // Interaction radius
    
    [Header("Visuals")]
    public SpriteRenderer npcSprite;
    public GameObject interactPrompt;
    
    [Header("Dialogue")]
    public DialogueUI dialogueUI;
    
    private bool hasBeenCollected = false;
    
    void Start()
    {
        npcSprite.enabled = false;
        interactPrompt.SetActive(false);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        // Check which trigger the player entered by checking which collider sent the event
        // OnTriggerEnter2D is called ON the GameObject that owns the trigger collider
        // So we need to check which of OUR triggers was involved
        
        // Discovery trigger entered
        if (Vector2.Distance(other.transform.position, transform.position) <= outerTrigger.radius 
            && !npcSprite.enabled)
        {
            npcSprite.enabled = true;
        }
        
        // Interaction trigger entered (player is now close enough)
        if (Vector2.Distance(other.transform.position, transform.position) <= innerTrigger.radius 
            && !hasBeenCollected)
        {
            interactPrompt.SetActive(true);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        // Player exited interaction range
        if (Vector2.Distance(other.transform.position, transform.position) > innerTrigger.radius)
        {
            interactPrompt.SetActive(false);
        }
    }
    
    void Update()
    {
        // Check for interact input
        if (interactPrompt.activeInHierarchy && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }
    }
    
    void OnInteract()
    {
        interactPrompt.SetActive(false);
        
        // Get clue dialogue
        ClueCombo combo = ClueDatabaseManager.Instance.GetComboByID(assignedComboID);
        
        string[] dialogueLines = new string[]
        {
            "Siera: I'm looking for information about Thornwall.",
            $"Ember Member: {combo.clueText}"
        };
        
        dialogueUI.StartDialogue(dialogueLines, OnDialogueComplete);
    }
    
    void OnDialogueComplete()
    {
        if (!hasBeenCollected)
        {
            GameStateManager.Instance.CollectClue(assignedComboID);
            hasBeenCollected = true;
            
            // Play collection SFX
            // AudioManager.Instance.PlaySFX("ClueCollected");
        }
    }
}
```

**Alternative Approach (Separate Trigger GameObjects):**

A cleaner implementation would be to create two child GameObjects with their own trigger colliders:

```csharp
// Attach to outer trigger child GameObject
public class OuterTriggerHandler : MonoBehaviour
{
    public HiddenNPC parentNPC;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            parentNPC.OnDiscoveryEnter();
    }
}

// Attach to inner trigger child GameObject  
public class InnerTriggerHandler : MonoBehaviour
{
    public HiddenNPC parentNPC;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            parentNPC.OnInteractionEnter();
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            parentNPC.OnInteractionExit();
    }
}

// Main HiddenNPC script
public class HiddenNPC : MonoBehaviour
{
    // ... (same header fields)
    
    public void OnDiscoveryEnter()
    {
        npcSprite.enabled = true;
    }
    
    public void OnInteractionEnter()
    {
        if (!hasBeenCollected)
            interactPrompt.SetActive(true);
    }
    
    public void OnInteractionExit()
    {
        interactPrompt.SetActive(false);
    }
    
    // ... (rest of implementation)
}
```

This second approach is cleaner and avoids the distance calculation workaround.

**File:** `Scripts/NPC/VisibleNPC.cs`

```csharp
public class VisibleNPC : MonoBehaviour
{
    [Header("UI")]
    public DialogueUI dialogueUI;
    
    private System.Action onDialogueDismissed;
    
    public void ShowDialogue(DialogueEntry entry, System.Action callback)
    {
        onDialogueDismissed = callback;
        dialogueUI.StartDialogue(entry.dialogueLines, OnDialogueComplete);
    }
    
    void OnDialogueComplete()
    {
        onDialogueDismissed?.Invoke();
    }
}
```

---

## Data Structures

### LevelData ScriptableObject

**Purpose:** Store all spatial references for a level scene

**Fields:**
```csharp
[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelIndex; // 1, 2, or 3
    public string levelName; // "Catacombs", "Gardens", "Corridors"
    
    [Header("Spawn Points - Use GameObject Tags")]
    public string sieraSpawnPointRound1Tag = "SieraSpawn_R1";
    public string sieraSpawnPointRound2Tag = "SieraSpawn_R2";
    
    [Header("Level Exit")]
    public string levelExitTriggerTag = "LevelExit";
    
    [Header("NPCs - Use GameObject Tags")]
    public string[] hiddenNPCSlotTags = new string[2] { "HiddenNPC_R1", "HiddenNPC_R2" };
    public string[] visibleNPCTags = new string[2] { "VisibleNPC_R1", "VisibleNPC_R2" };
    
    [Header("Enemies - Use GameObject Tags")]
    public string[] enemyTags; // e.g., { "Enemy_1", "Enemy_2", "Enemy_3" }
}
```

**Why Tags Instead of Direct References:**
ScriptableObjects are project assets and **cannot store references to scene objects** (GameObjects, Transforms, etc.). These references will always be null at runtime. Instead, we use string tags that are assigned to GameObjects in each scene, then use `GameObject.FindGameObjectWithTag()` during the spatial wiring sequence.

**Asset Instances:**
- LevelData_1.asset (Catacombs)
- LevelData_2.asset (Gardens)
- LevelData_3.asset (Corridors)

---

### ClueDatabase ScriptableObject

**Purpose:** Store all 10 clue/question combos

**Structures:**
```csharp
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

[CreateAssetMenu(fileName = "ClueDatabase", menuName = "Game/Clue Database")]
public class ClueDatabase : ScriptableObject
{
    public ClueCombo[] allCombos; // Length = 10
}
```

**Asset Instance:**
- ClueDatabase.asset

**Sample Entry:**
```
comboID: "CATA_01"
questionText: "When does Thornwall draw its walls inward?"
clueText: "The castle does not sleep — it exhales. Three times a night the walls breathe inward. Do not be standing in the east corridor when it does."
correctAnswer: "Three times a night"
```

---

### NarrativeSchedule ScriptableObject

**Purpose:** Store all visible NPC dialogue for 6 rounds

**Structures:**
```csharp
[System.Serializable]
public class DialogueEntry
{
    public int roundIndex; // 1-6
    public string speakerName;
    [TextArea(5, 10)]
    public string[] dialogueLines;
}

[CreateAssetMenu(fileName = "NarrativeSchedule", menuName = "Game/Narrative Schedule")]
public class NarrativeSchedule : ScriptableObject
{
    public DialogueEntry[] entries; // Length = 6
}
```

**Asset Instance:**
- NarrativeSchedule.asset

**Sample Entry:**
```
roundIndex: 1
speakerName: "Ember Soldier"
dialogueLines: 
[
    "The catacombs shift against you, Siera.",
    "Walls will move, and floor tiles will vanish beneath your feet.",
    "Another Ember waits deeper inside with a clue you will need at the Heart Gate.",
    "Watch the rats, watch the ground, and take care — Thornwall itself is alive."
]
```

---

## Integration Points

### Direct Cross-System Calls

**1. Enemy AI → SuspicionManager**
```csharp
// In EnemyAI.OnDetectionConfirmed()
SuspicionManager.Instance.AddSuspicion(12f);
```
**Reason:** Time-critical feedback, bypasses Game Manager

---

**2. Dialogue System → SuspicionManager**
```csharp
// In DialogueSystem.OnAnswerSelected()
if (selectedAnswer != correctAnswer)
{
    SuspicionManager.Instance.AddSuspicion(15f);
}
```
**Reason:** Immediate consequence

---

**3. Hidden NPC → GameStateManager**
```csharp
// In HiddenNPC.OnDialogueComplete()
GameStateManager.Instance.CollectClue(assignedComboID);
```
**Reason:** Direct state write

---

**4. Enemy AI → Game Manager**
```csharp
// In EnemyAI.OnDetectionConfirmed()
GameManager.Instance.OnLifeLost();
```
**Reason:** Trigger game state check (game over if lives = 0)

---

**5. Dialogue System → Game Manager**
```csharp
// In DialogueSystem.OnAnswerSelected() or PresentQuestion()
GameManager.Instance.TriggerWin();
GameManager.Instance.TriggerCaughtMidDialogue();
```
**Reason:** Trigger win/loss states

---

### One-Way Dependencies (No Circular References)

```
Game Manager → All Systems (activates/deactivates)
    ↓
Enemy AI → Suspicion Manager (adds suspicion)
Enemy AI → Game Manager (reports life lost)
    ↓
Dialogue System → Suspicion Manager (adds suspicion)
Dialogue System → Game State Manager (reads combos/clues)
Dialogue System → Game Manager (reports win/loss)
    ↓
NPC System → Game State Manager (writes collected clues)
    ↓
Player Controller → (no outward calls)
```

---

## Implementation Sequence

### Step-by-Step Build Order

#### **Step 1: Create File Structure**
Run the following in Unity:
1. Create all folder hierarchies as outlined in [Project File Structure](#project-file-structure)
2. Create all 8 scene files
3. Verify structure matches specification

---

#### **Step 2: Implement Core Enums and Interfaces**
1. Create `GameState.cs`:
```csharp
public enum GameState
{
    MainMenu,
    Prologue,
    LevelPlaying,
    RoundComplete,
    LevelComplete,
    Paused,
    DialogueSequence,
    Win,
    GameOver,
    CaughtAtGate
}
```

---

#### **Step 3: Implement Persistent Singletons**
Create in order:
1. `SuspicionManager.cs` (Section 2)
2. `GameStateManager.cs` (Section 3)
3. `ClueDatabaseManager.cs` (Section 6)

Add GameObject "PersistentManagers" to MainMenu scene with all three attached.

---

#### **Step 4: Implement Game Manager**
1. Create `GameManager.cs` (Section 1)
2. Add to MainMenu scene
3. Wire up references to Persistent Managers
4. Test DontDestroyOnLoad functionality

---

#### **Step 5: Configure Input System**
1. Open `InputSystem_Actions.inputactions`
2. Create "Player" Action Map
3. Create "Move" Action (Value, Vector2)
4. Add bindings: WASD, Arrows, Gamepad Left Stick
5. Generate C# class

---

#### **Step 6: Implement Player Controller**
1. Create `PlayerController.cs` (Section 4)
2. Create Siera prefab with:
   - SpriteRenderer (placeholder sprite)
   - Rigidbody2D (Gravity Scale = 0, Interpolate)
   - CapsuleCollider2D
   - PlayerController component
3. Test movement in empty scene

---

#### **Step 7: Create Physics Layers**
1. Edit → Project Settings → Tags and Layers
2. Add layers: Player, Enemy, SafeZone, Walls
3. Edit → Project Settings → Physics 2D
4. Configure collision matrix:
   - Enemy ✗ SafeZone
   - Player ✓ SafeZone

---

#### **Step 8: Implement Tilemap System**
1. Import first tileset asset (Catacombs)
2. Configure import settings (Section Phase 3.1)
3. Slice sprites
4. Create Tile Palette
5. In Level1_Catacombs scene:
   - Create Grid GameObject
   - Create 4 Tilemap children (Ground, Wall, Decoration, Overhead)
   - Configure sorting layers
   - Add TilemapCollider2D + CompositeCollider2D to Wall layer only
6. Paint test maze

---

#### **Step 9: Implement Enemy AI**
1. Create `EnemyAI.cs` (Section 5)
2. Create CatacombCreature prefab:
   - SpriteRenderer (placeholder sprite)
   - Rigidbody2D (Kinematic)
   - CircleCollider2D
   - EnemyAI component
   - Detection indicator UI element
3. Place waypoint GameObjects in Level1_Catacombs
4. Assign waypoints to enemy
5. Test patrol and detection

---

#### **Step 10: Create ScriptableObject Structures**
1. Create `LevelData.cs` (Data Structures section)
2. Create `ClueDatabase.cs` and `ClueCombo.cs`
3. Create `NarrativeSchedule.cs` and `DialogueEntry.cs`
4. Generate asset instances:
   - LevelData_1.asset, LevelData_2.asset, LevelData_3.asset
   - ClueDatabase.asset
   - NarrativeSchedule.asset

---

#### **Step 11: Tag GameObjects and Populate LevelData Assets**
For each level scene:
1. Create empty GameObjects and **assign appropriate tags**:
   - SieraSpawnPoint_Round1 → Tag: "SieraSpawn_R1"
   - SieraSpawnPoint_Round2 → Tag: "SieraSpawn_R2"  
   - LevelExitTrigger (with BoxCollider2D trigger) → Tag: "LevelExit"
   - HiddenNPC_Slot1 → Tag: "HiddenNPC_R1"
   - HiddenNPC_Slot2 → Tag: "HiddenNPC_R2"
   - VisibleNPC_Slot1 → Tag: "VisibleNPC_R1"
   - VisibleNPC_Slot2 → Tag: "VisibleNPC_R2"
   - Enemy instances → Tag: "Enemy_1", "Enemy_2", "Enemy_3", etc.
   - Waypoint paths for enemies (no tags needed - direct waypoint array assignment on enemy component)

2. In LevelData assets, **enter the tag strings** (not GameObject references):
   - Set sieraSpawnPointRound1Tag = "SieraSpawn_R1"
   - Set hiddenNPCSlotTags = { "HiddenNPC_R1", "HiddenNPC_R2" }
   - Set enemyTags = { "Enemy_1", "Enemy_2", "Enemy_3" }
   - etc.

3. **Verify tags are correctly assigned** in Unity:
   - Edit → Project Settings → Tags and Layers
   - Add all custom tags to the Tags list
   - Confirm each GameObject in scene has correct tag assigned

**Why Tags?** ScriptableObjects cannot store scene references - they'll be null at runtime. Tags allow runtime lookup via `GameObject.FindGameObjectWithTag()`.

---

#### **Step 12: Implement Spatial Wiring in Game Manager**
Add `OnSceneLoaded()` method to Game Manager (Phase 6.3):
- Position Siera
- Assign combo IDs to hidden NPCs
- Register exit trigger
- Confirm enemy waypoints
- Transition to LevelPlaying

Test scene load flow.

---

#### **Step 13: Implement Dialogue UI**
1. Create `DialogueUI.cs`
2. Create DialogueBox prefab:
   - Bottom-screen panel
   - Avatar image (left/right)
   - Text display
   - Continue button
3. Test with placeholder dialogue

---

#### **Step 14: Implement NPC System**
1. Create `HiddenNPC.cs` (Section 8)
2. Create HiddenEmberNPC prefab:
   - SpriteRenderer (initially disabled)
   - 2x CircleCollider2D (outer/inner triggers)
   - Interact prompt UI
   - HiddenNPC component
3. Create `VisibleNPC.cs`
4. Create VisibleEmberNPC prefab
5. Place NPCs in level scenes
6. Test discovery and interaction

---

#### **Step 15: Implement Clue Collection**
1. Wire HiddenNPC dialogue completion to `GameStateManager.CollectClue()`
2. Test clue tracking across scene loads

---

#### **Step 16: Populate ClueDatabase**
Sarah populates ClueDatabase.asset with 10 entries (see sample in Data Structures section).

---

#### **Step 17: Implement Playthrough Initialization**
1. Complete `ClueDatabaseManager.InitialisePlaythrough()` (Section 6)
2. Call from `GameManager.NewGame()`
3. Test random selection and combo assignment

---

#### **Step 18: Populate NarrativeSchedule**
Sarah populates NarrativeSchedule.asset with 6 entries.

---

#### **Step 19: Implement Round/Level Completion**
1. Add `OnRoundComplete()` to Game Manager
2. Add `OnLevelComplete()` to Game Manager
3. Test visible NPC dialogue triggering
4. Test lives carry calculation
5. Test clean level bonus

---

#### **Step 20: Implement Conclusion Scene**
1. Create `DialogueSystem.cs` (Section 7)
2. Create `QuestionAnswerHandler.cs`
3. Build Conclusion scene UI
4. Test pre-dialogue gate check
5. Test question sequence
6. Test answer validation and suspicion

---

#### **Step 21: Implement Scene Transitions**
1. Create `SceneTransitionController.cs`
2. Create FadeCanvas prefab (persists via DontDestroyOnLoad)
3. Implement fade coroutine
4. Wire to Game Manager
5. Test all scene transitions

---

#### **Step 22: Implement HUD**
1. Create `HUDController.cs`
2. Create `LivesDisplay.cs`
3. Create `SuspicionBarUI.cs`
4. Create `ClueInventoryUI.cs`
5. Build HUD prefab
6. Wire to Singleton events (OnLivesChanged, OnSuspicionChanged, OnClueCollected)
7. Test real-time updates

---

#### **Step 23: Build All 3 Levels**
Sarah and Wietske paint all mazes:
1. Level 1 - Catacombs (2 rounds)
2. Level 2 - Gardens (2 rounds)
3. Level 3 - Corridors (2 rounds)

Populate all LevelData assets with scene references.

---

#### **Step 24: Implement Menu Screens**
1. Create `MenuController.cs`
2. Build MainMenu scene
3. Build GameOver scene
4. Build Victory scene
5. Wire button callbacks to Game Manager

---

#### **Step 25: Integrate Audio**
1. Import all audio assets (Section 5.2 of TDD)
2. Create AudioManager (optional, or handle via direct AudioSource components)
3. Assign music loops to levels
4. Wire SFX to events:
   - Detection confirmed
   - Life lost
   - Clue collected
   - Suspicion bar fill
   - Correct/wrong answers
   - Victory/game over

---

#### **Step 26: Polish & Tuning**
1. Tune enemy parameters per type
2. Balance suspicion values
3. Test all win/loss conditions
4. Test both playstyles (Ghost vs Hunter)
5. Full playthrough testing

---

#### **Step 27: Build & Deploy**
1. Configure build settings (PC standalone)
2. Test final build
3. Package for submission

---

## Configuration Reference

### Key Tunable Parameters

**Lives Economy:**
- Starting lives: 5
- Lives carry per level: +1 (capped at 5)
- Lives lost triggers game over: 0

**Suspicion Values:**
- Life lost: +12%
- Wrong dialogue answer: +15%
- Clean level bonus: -5%
- Threshold: 100%

**Enemy Detection (default, tune per type):**
- Vision range: 5 units
- Vision angle: 90 degrees
- Detection fill time: 2 seconds

**Player Movement:**
- Move speed: 3 units/second
- Ease-in duration: 0.1 seconds

**Tilemap:**
- Tile size: 16x16 pixels
- Pixels per unit: 16

**Scene Names:**
- MainMenu
- Prologue
- Level1_Catacombs
- Level2_Gardens
- Level3_Corridors
- Conclusion
- GameOver
- Victory

---

## Notes for Claude Code

### Implementation Priorities

**Week 1 Focus:**
- Get Singletons and Game Manager running
- Player movement functional
- Basic tilemap maze playable

**Week 2 Focus:**
- Enemy AI patrol and detection working
- Spatial wiring sequence complete
- Lives and suspicion systems integrated

**Week 3 Focus:**
- All 3 levels built and wired
- Clue collection functional
- NPC dialogue working

**Week 4 Focus:**
- Conclusion scene complete
- Scene transitions polished
- HUD and menus finished

### Testing Checkpoints

After each phase, verify:
1. **Phase 1-4:** Can move Siera in empty scene
2. **Phase 5:** Enemy patrols and detects
3. **Phase 6:** Scene loads preserve game state
4. **Phase 7:** NPCs reveal and trigger dialogue
5. **Phase 8:** Clues track across sessions
6. **Phase 9:** Conclusion questions validate correctly
7. **Phase 10:** Full playthrough from MainMenu → Victory works

### Common Pitfalls to Avoid

1. **ScriptableObject scene reference trap:** Never try to store GameObjects, Transforms, or Components in ScriptableObject fields - use tags instead and look them up at runtime with `FindGameObjectWithTag()`
2. **OnTriggerEnter2D logic error:** The `other` parameter is the collider entering YOUR trigger, not your own trigger. Use distance checks or child GameObjects with separate scripts to differentiate multiple triggers
3. **State access modifiers:** Make sure `currentState` in GameManager is accessible (public property) if other scripts need to read it
4. **Singleton initialization order:** Ensure all Singletons awake before Game Manager needs them
5. **Collision layer confusion:** Double-check Physics2D matrix for SafeZone exclusion
6. **Input during wrong state:** Always gate input on `GameState.LevelPlaying`
7. **Tilemap collision:** Only WallLayer gets colliders, others are visual only

---

## Architecture Principles Summary

**1. Single Responsibility**
- Game Manager = orchestration
- Singletons = persistent data
- Systems = isolated behaviours

**2. Data-Driven Design**
- ScriptableObjects for all content
- Inspector-assignable references
- No hardcoded strings or magic numbers

**3. Explicit State Management**
- All systems defer to Game Manager's state
- No system modifies state directly except Game Manager

**4. Minimal Cross-System Communication**
- Most calls mediated through Game Manager
- Direct calls only for time-critical feedback (Enemy → Suspicion)

**5. Scene Independence**
- Each scene is self-contained
- Spatial wiring reconnects on every load
- No assumptions about GameObject persistence

---

**End of Specification**

This document serves as the complete technical blueprint for implementing *Ember of the Last Witch*. Follow the Implementation Sequence step-by-step, referring to System Details for implementation guidance and Data Structures for asset configuration.
