# Ember of the Last Witch — Project Status

> Reference `game-architecture.md` for full implementation details on any item below.
> **Last updated:** 2026-03-25

---

## ⚠️ Known Temporary Bypasses

None — all bypasses resolved.

---

## Legend

- ✅ Done
- 🔧 In Progress
- ⬜ Not Started
- 👤 Needs Unity Editor (manual)

---

## Phase 1 — Foundation Scripts

| #   | Task                                      | Status | Notes                                                                                |
| --- | ----------------------------------------- | ------ | ------------------------------------------------------------------------------------ |
| 1   | Folder structure created                  | ✅     | All `Assets/Scripts/`, `Prefabs/`, `Art/`, `Audio/` folders exist                    |
| 2   | `GameState.cs` enum                       | ✅     | 10 states                                                                            |
| 3   | `SuspicionManager.cs`                     | ✅     | Persistent singleton, events wired                                                   |
| 4   | `GameStateManager.cs`                     | ✅     | Lives, clues, combos — persistent singleton                                          |
| 5   | `ClueDatabaseManager.cs`                  | ✅     | Shuffles & selects 6 combos per run                                                  |
| 6   | `GameManager.cs`                          | ✅     | State machine, spatial wiring, lifecycle                                             |
| 7   | Create 8 scenes in Unity                  | ✅     | All 8 scenes created                                                                 |
| 8   | Add scenes to Build Profiles              | 🔧     | MainMenu, Level1_Catacombs, Level2_Gardens, Cutscene1_AfterCatacombs, Cutscene2_AfterGardens added; Prologue, Conclusion, GameOver, Victory still needed |
| 9   | PersistentManagers GameObject in MainMenu | ✅     | GameManager, SuspicionManager, GameStateManager, ClueDatabaseManager attached        |

---

## Phase 2 — Player & Input

| #   | Task                           | Status | Notes                                                                                       |
| --- | ------------------------------ | ------ | ------------------------------------------------------------------------------------------- |
| 10  | `PlayerController.cs`          | ✅     | Velocity-based, state-gated, ease-in                                                        |
| 11  | `PlayerAnimationController.cs` | ✅     | Speed + direction params for Animator                                                       |
| 12  | Configure Input System         | ✅     | Action Map "Siera Player", Move Vector2, WASD bound                                         |
| 13  | Create Siera prefab            | ✅     | Placeholder white circle — Rigidbody2D + CapsuleCollider2D + PlayerController + PlayerInput |
| 14  | Test Siera movement in scene   | ✅     | Moving and colliding with walls correctly                                                   |

---

## Phase 3 — Maze & Physics

| #   | Task                                | Status | Notes                                                           |
| --- | ----------------------------------- | ------ | --------------------------------------------------------------- |
| 15  | Create Physics layers               | ✅     | Player, Enemy, SafeZone, Walls                                  |
| 16  | Configure collision matrix          | ✅     | Enemy ✗ SafeZone                                                |
| 17  | Import Catacombs tileset            | ✅     | `mainlevbuild.png` + `decorative.png`, PPU=16, Point filter     |
| 18  | Slice sprites & create Tile Palette | ✅     | Catacombs_Palette created                                       |
| 19  | Build Level1_Catacombs tilemap      | 🔧     | Test maze painted (Ground + Wall layers); full maze in progress |
| 19b | Build Level2_Gardens tilemap        | 🔧     | Maze painted; tileset TBD                                       |
| 19c | Build Level3_Corridors tilemap      | 👤⬜   | In progress by team                                             |

---

## Phase 4 — Enemy AI

| #   | Task                           | Status | Notes                                                                                                                                      |
| --- | ------------------------------ | ------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| 20  | `EnemyAI.cs`                   | ✅     | Original — patrol + vision cone + raycast + two-stage detection (fully commented, kept as fallback)                                        |
| 21  | `EnemyDetection.cs`            | ✅     | Standalone alternative component                                                                                                           |
| 22  | `EnemyPatrol.cs`               | ✅     | **Wietske** — full rewrite: RB2D physics movement, WaypointMarker system, FOV scan sweep at Search waypoints, animator + sound integration |
| 22b | `FieldOfView.cs`               | ✅     | **Wietske** — rendered FOV mesh, 20-ray cast, deforms around walls, material swap on detection                                             |
| 22c | `WaypointMarker.cs`            | ✅     | **Wietske** — waypoint component with type enum: Navigation (pass-through) or Search (pause + sweep)                                       |
| 22d | `EnemySoundManager.cs`         | ✅     | **Wietske** — footstep audio (random clips, timed interval) + atmospheric breathing sounds                                                 |
| 23  | Create CatacombCreature prefab | ✅     | **Wietske** - RB2D (kinematic) + CircleCollider2D + EnemyPatrol + EnemySoundManager + FOV prefab assigned                                  |
| 24  | Place waypoints in Levels      | 👤⬜   | Create WaypointMarker GameObjects under a parent; assign parent to EnemyPatrol.waypointsParent                                             |
| 25  | Test enemy patrol & detection  | 👤⬜   | Use ignoreGameStateForTesting flag to test without full game loop                                                                          |

---

## Phase 5 — Data Structures

| #   | Task                                        | Status | Notes                                                                      |
| --- | ------------------------------------------- | ------ | -------------------------------------------------------------------------- |
| 26  | `LevelData.cs`                              | ✅     | Tag-based (not Transform refs)                                             |
| 27  | `ClueDatabase.cs` + `ClueCombo.cs`          | ✅     |                                                                            |
| 28  | `NarrativeSchedule.cs` + `DialogueEntry.cs` | ✅     |                                                                            |
| 29  | Create ClueDatabase.asset                   | 👤⬜   | Right-click → Create → Game → Clue Database                                |
| 30  | Populate ClueDatabase with 10 entries       | 👤⬜   | **Sarah** — see sample entry in `game-architecture.md` §ClueDatabase       |
| 31  | Create NarrativeSchedule.asset              | 👤⬜   | Right-click → Create → Game → Narrative Schedule                           |
| 32  | Populate NarrativeSchedule with 6 entries   | 👤⬜   | **Sarah**                                                                  |
| 33  | Create LevelData_1/2/3.asset                | ✅     | LevelData_1 and LevelData_2 created; LevelData_3 pending Level3 completion |

---

## Phase 6 — Spatial Wiring

| #   | Task                                            | Status | Notes                                                                     |
| --- | ----------------------------------------------- | ------ | ------------------------------------------------------------------------- |
| 34  | Spatial wiring in `GameManager.OnSceneLoaded()` | ✅     | Siera spawn, NPC combo assign, exit trigger, enemy confirm                |
| 35  | Tag all scene GameObjects per level             | 🔧     | SieraSpawn + LevelExit placed in Level1; Level2 + Level3 tags still needed |
| 36  | Register all custom tags in Project Settings    | ✅     | All tags registered                                                        |
| 37  | Populate LevelData assets with tag strings      | 🔧     | LevelData_1 updated (sieraSpawnPointTag, hiddenNPCSlotTags); LevelData_2 + LevelData_3 pending |
| 38  | Assign LevelData array in GameManager Inspector | ✅     | LevelData_1 assigned; LevelData_2 + LevelData_3 pending                   |
| 39  | Test scene load + spatial wiring                | 🔧     | MainMenu → Level1 working; Siera spawning confirmed                       |

---

## Phase 7 — NPC System

> ⚠️ VisibleNPC scrapped for v1 — simplified to single EmberNPC (always present, sprite hidden until player in range, E to talk)

| #   | Task                                                 | Status | Notes                                                                                           |
| --- | ---------------------------------------------------- | ------ | ----------------------------------------------------------------------------------------------- |
| 40  | `EmberNPC.cs`                                        | ✅     | **Wietske** — single script, always visible, trigger-based interaction, E to talk, clue collect |
| 41  | Original NPC scripts (Hidden/Visible/Trigger)        | ✅     | Replaced by EmberNPC — kept in codebase but not used                                            |
| 44  | Create EmberNPC prefab                               | ✅     | **Wietske** — prefab ready with sprite hide/show + interact prompt                              |
| 45  | Place NPCs in all level scenes                       | 👤⬜   | **Praise** — place prefab, tag HiddenNPC_1/HiddenNPC_2, leave assignedComboID blank (GameManager fills) |
| 46  | Create ClueDatabase.asset + populate 10 entries      | 👤⬜   | **Sarah** — Right-click → Create → Game → Clue Database; fill comboID, clueText, questionText, correctAnswer |
| 47  | Test NPC interaction → clue collected                | 👤⬜   | **Praise** — confirm dialogue fires + GameStateManager.collectedClues updates                   |

---

## Phase 8 — Dialogue System

| #   | Task                                          | Status | Notes                                                                          |
| --- | --------------------------------------------- | ------ | ------------------------------------------------------------------------------ |
| 48  | `DialogueUI.cs`                               | ✅     | Linear + multiple choice, speaker parsing                                      |
| 49  | `DialogueSystem.cs`                           | ✅     | Heart Gate Q&A, suspicion gate check                                           |
| 50  | `QuestionAnswerHandler.cs`                    | ✅     | Generates 3 options (1 correct + 2 decoys)                                     |
| 51  | Build DialogueBox prefab                      | 👤⬜   | Canvas → Panel → Speaker text + Body text + Continue button + 3 choice buttons |
| 52  | Wire DialogueUI to prefab fields in Inspector | 👤⬜   |                                                                                |
| 53  | Test linear dialogue (HiddenNPC flow)         | 👤⬜   |                                                                                |
| 54  | Test multiple-choice (Conclusion scene)       | 👤⬜   |                                                                                |

---

## Phase 9 — Scene Transitions & HUD

| #   | Task                           | Status | Notes                                                                                 |
| --- | ------------------------------ | ------ | ------------------------------------------------------------------------------------- |
| 55  | `SceneTransitionController.cs` | ✅     | Fade coroutine                                                                        |
| 56  | Create FadeCanvas prefab       | 👤⬜   | Full-screen black Image on a Canvas (DontDestroyOnLoad via SceneTransitionController) |
| 57  | `HUDController.cs`             | ✅     | Wires singleton events to sub-controllers                                             |
| 58  | `LivesDisplay.cs`              | ✅     | 5 icon images, full/empty sprites                                                     |
| 59  | `SuspicionBarUI.cs`            | ✅     | Color-coded fill bar                                                                  |
| 60  | `ClueInventoryUI.cs`           | ✅     | Counter text                                                                          |
| 61  | Build HUD prefab               | 👤⬜   | Assign all UI references in Inspector                                                 |
| 62  | Test HUD updates in real time  | 👤⬜   |                                                                                       |

---

## Phase 10 — Menus & End Screens

| #   | Task                              | Status | Notes                                                                                     |
| --- | --------------------------------- | ------ | ----------------------------------------------------------------------------------------- |
| 63  | `MenuController.cs`               | ✅     | NewGame, Restart, Quit, PlayAgain                                                         |
| 64  | Build MainMenu scene UI           | ✅     | Background, title logo, ember flame, styled buttons with Cinzel font — complete           |
| 65  | Build GameOver scene UI           | 👤⬜   | **Praise** — same style as MainMenu; Restart button → MenuController                     |
| 66  | Build Victory scene UI            | 👤⬜   | **Praise** — same style as MainMenu; Play Again + Quit buttons → MenuController           |
| 67  | Build Prologue scene              | ✅     | 6 panels, Siera portrait, dialogue box, fade transitions, routes to Level1 — complete     |
| 67b | Between-level cutscene scenes     | ✅     | `Cutscene1_AfterCatacombs` + `Cutscene2_AfterGardens` built — Canvas, dialogue box, portrait, fade, CutsceneController.cs wired; panels empty awaiting Sarah's art + text |
| 67c | Conclusion / Heart Gate scene     | 👤⬜   | **Praise** — DialogueSystem.cs already written; build scene UI, wire to GameManager.TriggerWin/CaughtAtGate |

---

## Phase 11 — Level Building

| #   | Task                                                | Status | Notes                                                                                   |
| --- | --------------------------------------------------- | ------ | --------------------------------------------------------------------------------------- |
| 68  | Paint Level1_Catacombs                              | 🔧     | **Sarah** — close off open edges, 2-tile corridors throughout                           |
| 69  | Paint Level2_Gardens                                | 🔧     | **Sarah** — close off open edges                                                        |
| 70  | Paint Level3_Corridors                              | 🔧     | **Sarah** — in progress; corridors maze deemed large enough                             |
| 71  | Import Garden & Corridor tilesets                   | 🔧     | **Sarah**                                                                               |
| 71b | Decorate all levels                                 | 👤⬜   | **Sarah** — props, decoration layer, final polish                                       |
| 71c | Add music per scene                                 | 👤⬜   | **Sarah** — ambient per level + menu/prologue/victory themes                            |
| 72  | Place enemies + waypoints in all levels             | 👤⬜   | **Wietske** — Catacombs ready for enemies now; Gardens/Corridors when mazes are closed  |
| 72b | Finish Siera prefab + movement animations           | 👤⬜   | **Wietske**                                                                             |
| 72c | Finish NPC prefab + idle animations                 | 👤⬜   | **Wietske**                                                                             |
| 73  | Place SieraSpawn + LevelExit in all scenes          | 👤⬜   | **Praise** — empty GameObjects tagged SieraSpawn + LevelExit in each level              |
| 74  | Populate LevelData_2 and LevelData_3 assets         | 👤⬜   | **Praise** — tag strings in Inspector                                                   |
| 75  | Camera follow Siera                                 | 👤⬜   | **Praise** — Cinemachine Virtual Camera or simple follow script                         |

---

## Phase 12 — HUD

| #   | Task                              | Status | Notes                                                                                    |
| --- | --------------------------------- | ------ | ---------------------------------------------------------------------------------------- |
| 76  | Build suspicion bar UI in scenes  | 👤⬜   | **Praise** — SuspicionBarUI.cs exists; build Canvas overlay, assign Image fill          |
| 77  | Build lives display UI in scenes  | 👤⬜   | **Praise** — LivesDisplay.cs exists; 5 icon images, full/empty sprites                  |
| 78  | Build clue counter UI in scenes   | 👤⬜   | **Praise** — ClueInventoryUI.cs exists; TMP text showing X/6                            |
| 79  | Wire HUDController in each scene  | 👤⬜   | **Praise** — assign sub-controllers in Inspector, events auto-wire on enable            |

---

## Phase 13 — Polish & Ship

| #   | Task                                              | Status | Notes                                                                 |
| --- | ------------------------------------------------- | ------ | --------------------------------------------------------------------- |
| 80  | Tune enemy parameters per type                    | ⬜     | **Wietske** — Garden Wolf: faster + wider; Castle Guard: slower + longer range |
| 81  | Balance suspicion values                          | ⬜     | **Praise** — +12% detection, +15% wrong answer, -5% clean level      |
| 82  | Full playthrough test (MainMenu → Victory)        | ⬜     | **All**                                                               |
| 83  | Test all loss conditions (lives=0, suspicion≥100) | ⬜     | **All**                                                               |
| 84  | Add scenes to Build Profiles (remaining)          | 👤⬜   | **Praise** — Prologue, Conclusion, GameOver, Victory still needed     |
| 85  | Configure PC build settings                       | 👤⬜   | **Praise** — File → Build Profiles → PC Standalone                   |
| 86  | Final build + package for submission              | 👤⬜   | **All**                                                               |

---

## Summary

| Category           | Total  | Done   | In Progress | Left   |
| ------------------ | ------ | ------ | ----------- | ------ |
| Scripts (Claude)   | 30     | 30     | 0           | 0      |
| Scripts (Wietske)  | 4      | 4      | 0           | 0      |
| Unity Editor tasks | 54     | 22     | 3           | 29     |
| Content (Sarah)    | 5      | 1      | 0           | 4      |
| **Overall**        | **93** | **57** | **3**       | **33** |

---

## Today's Sprint — 2026-03-25

### Praise
1. ✅ Build Cutscene1_AfterCatacombs + Cutscene2_AfterGardens scenes (canvas, dialogue box, portrait, fade, CutsceneController wired)
2. ✅ Wire level flow: Level1 → Cutscene1 → Level2 → Cutscene2 → Level3 → Conclusion (GameManager updated)
3. Add remaining scenes to Build Profiles (Prologue, Conclusion, GameOver, Victory)
4. Camera follow Siera in all 3 level scenes
5. Build HUD — suspicion bar + lives display + clue counter, wire in all 3 levels
6. Build DialogueBox prefab + wire DialogueUI
7. GameOver + Victory scene UI (same MainMenu style)
8. Build Options/Pause panel (Resume + Restart + Quit), accessible from HUD in every level
9. Conclusion / Heart Gate scene — DialogueSystem.cs already written, just needs scene UI
10. Place SieraSpawn + LevelExit GameObjects in Level2 + Level3
11. Populate LevelData_2 + LevelData_3 in Inspector
12. Place EmberNPC prefab in scenes, tag HiddenNPC_1 / HiddenNPC_2 (after DialogueUI prefab is ready)
13. Fill cutscene panels once Sarah delivers art + text

### Wietske
1. Place enemies + waypoints in Level1_Catacombs (ready now)
2. Place enemies in Level2_Gardens + Level3_Corridors when Sarah closes mazes
3. Finish Siera prefab + movement animations
4. Finish NPC prefab + idle animations
5. Tune enemy parameters per level type

### Sarah
1. Close off all maze edges (no open exits outside ground layer)
2. Finish Level3_Corridors
3. Decorate all 3 levels (props, decoration layer)
4. Music — ambient per level + menu/prologue/victory themes
5. ✅ Cutscene story outline delivered (after Level1 + after Level2)
6. Deliver cutscene panel assets (backgrounds + character portraits) + final text per panel
7. Fill ClueDatabase.asset — 10 entries (comboID, clueText, questionText, correctAnswer)

### Claude (code ready, waiting)
- All HUD scripts exist, just need scene wiring
- DialogueSystem + Heart Gate fully scripted
- CutsceneController.cs ready — just needs Sarah's panels filled in Inspector
