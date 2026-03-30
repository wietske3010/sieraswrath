# Tileset Instructions
## Ember of the Last Witch — Unity 6

> Follow this guide whenever setting up a new tileset and painting a level scene.
> Work alongside `game-architecture.md` for level layout requirements and `status.md` to track progress.

---

## Overview

Each level scene has its own painted tilemap. The maze lives directly inside the scene — it is not a prefab or external asset. Tilesets are stored in `Assets/Art/Tilesets/Downloads/` (originals) and copied into `Assets/Art/Tilesets/` (working copies only).

**Current tilesets:**
| Level | Tileset | Source |
|-------|---------|--------|
| Level1_Catacombs | RF_Catacombs_v1.0 | szadiart.itch.io/rogue-fantasy-catacombs |
| Level2_Gardens | TBD | — |
| Level3_Corridors | TBD | — |

---

## Step A — Create Sorting Layers (once, project-wide)

> Only needs to be done once. Skip if already done.

1. **Edit → Project Settings → Tags and Layers → Sorting Layers**
2. Add these in order (Unity renders top of list first):

| Order | Name | Used For |
|-------|------|----------|
| 0 | Ground | Floor tiles |
| 1 | Walls | Wall tiles |
| 2 | Props | Decoration tiles |
| 3 | Ceiling | Overhead tiles |
| 4 | Characters | Siera, enemies, NPCs |
| 5 | UI | HUD, dialogue |

---

## Step B — Import a Tileset

1. Copy the tileset PNG from `Assets/Art/Tilesets/Downloads/` into `Assets/Art/Tilesets/`
   - Example: copy `RF_Catacombs_v1.0/mainlevbuild.png` → `Assets/Art/Tilesets/Catacombs_Tileset.png`
   - Keep the Downloads folder untouched — it's the original

2. Click the copied PNG in the **Project window**

3. In the **Inspector**, set:
   - Texture Type: **Sprite (2D and UI)**
   - Sprite Mode: **Multiple**
   - Pixels Per Unit: **16**
   - Filter Mode: **Point (no filter)**
   - Click **Apply**

4. Click **Sprite Editor** → **Slice** → **Grid By Cell Size** → set to **16 × 16** → **Apply** → close Sprite Editor

---

## Step C — Create a Tile Palette

1. **Window → 2D → Tile Palette** (opens the painting panel)
2. Click **Create New Palette**
   - Name: `Catacombs_Palette` (or `Gardens_Palette` etc.)
   - Save location: `Assets/Art/TilePalettes/`
3. Drag your sliced PNG from the Project window into the Palette panel
4. Tiles populate the palette — you can now paint with them

---

## Step D — Build the Grid in a Level Scene

> Do this once per level scene.

1. Open the target level scene (e.g. `Level1_Catacombs`)

2. In the **Hierarchy**, right-click → **2D Object → Tilemap → Rectangular**
   - This creates a `Grid` parent with one `Tilemap` child

3. Rename that first Tilemap child to `GroundLayer`
   - In the Inspector, set **Sorting Layer** to `Ground`, **Order in Layer** to `0`

4. Right-click the `Grid` → **2D Object → Tilemap → Rectangular** — do this **3 more times**, creating:

| Name | Sorting Layer | Order in Layer |
|------|--------------|----------------|
| WallLayer | Walls | 1 |
| DecorationLayer | Props | 2 |
| OverheadLayer | Ceiling | 3 |

5. Select **WallLayer** and add these two components (**Add Component** in Inspector):
   - **Tilemap Collider 2D**
     - Set **Composite Operation** to **Merge** *(Unity 6 — replaces the old "Used By Composite" checkbox)*
   - **Composite Collider 2D**
     - This auto-adds a Rigidbody2D — set its **Body Type** to **Static**

> Only WallLayer gets colliders. The other three layers are visual only.

---

## Step E — Painting the Maze

### The 3 Tools You'll Use

| Tool | Shortcut | Use For |
|------|----------|---------|
| Pencil | **B** | Painting tiles one at a time, click and drag |
| Box Fill | **U** | Drag a rectangle to fill an area instantly |
| Eraser | **D** | Remove tiles |

---

### What's in the Catacombs Palette

| Area of the sheet | What it is | Which layer to paint on |
|-------------------|------------|------------------------|
| Right side — grid of small dark squares | Floor tiles (stone floor, varies slightly in shade) | GroundLayer |
| Left side — large stone block segments with pillars | Wall tiles (solid stone, use the inner 16×16 blocks) | WallLayer |
| Top row — tiny single-tile pieces | Wall cap / top detail pieces | WallLayer or DecorationLayer |
| Centre — arch and doorway pieces | Doorway decorations | DecorationLayer |
| `decorative.png` — urns, braziers, barrels, rocks | Props | DecorationLayer |

> The animated files (torch_1-4.png, candleA/B_01-04.png) are **not** painted as tiles. They are placed as animated sprite GameObjects on top of the tilemap separately.

---

### Painting a Room — Step by Step

This walks through one complete room. Repeat this pattern to build the full maze.

#### 1. Floor (GroundLayer)

1. In the Tile Palette panel, set the **Active Tilemap** dropdown to `GroundLayer`
2. Select the **Box Fill** tool **(U)**
3. In the palette, click one of the dark floor tiles from the right side of the sheet
4. In the Scene view, drag a rectangle — roughly **15 tiles wide × 10 tiles tall**
5. Release — the floor fills in solid

#### 2. Walls (WallLayer)

1. Switch Active Tilemap to `WallLayer`
2. Select the **Pencil** tool **(B)**
3. In the palette, select a solid stone wall tile from the left section
4. Paint a **1-tile-thick border** around the outside edge of your floor:
   - Bottom row → drag across
   - Top row → drag across
   - Left column → drag down
   - Right column → drag down
5. **Leave a 2-tile gap** in one wall — this is the corridor exit

Collision is automatic on this layer because of the Tilemap Collider 2D setup in Step D.

#### 3. Corridor (WallLayer)

From the gap in your wall, extend two parallel wall lines outward:
- **1 tile on each side**, **2 tiles apart**, extending **~10 tiles**
- Leave the far end open — this connects to the next room

```
######  WALL  ######
#                  #
#      ROOM        #
#                  #
######  ######  ####
        ||  ||         ← corridor walls (1 tile each side)
        ||  ||
```

#### 4. Doorway Decoration (DecorationLayer)

1. Switch Active Tilemap to `DecorationLayer`
2. Select the **Pencil** tool **(B)**
3. In the palette, pick one of the arch/doorway pieces from the centre of the sheet
4. Paint it **over the exit gap** in your wall

This is visual only — no collision on this layer.

#### 5. Props (DecorationLayer)

Still on `DecorationLayer`, scatter some props from `decorative.png` inside the room:
- Urns against walls
- Brazier near a corner
- Barrels or rocks as obstacles (purely visual — not physics obstacles unless you add a separate collider GameObject)

#### 6. OverheadLayer

**Skip for now** — this pack does not have clear overhead/ceiling tiles. Leave OverheadLayer empty until a suitable asset is available.

---

### The Full Layer Stack (what it looks like when done)

```
OverheadLayer  ← empty for now
DecorationLayer ← arches, urns, props
WallLayer      ← stone walls + collision
GroundLayer    ← stone floor tiles
```

Siera and enemies render on the `Characters` sorting layer above all of these.

---

## Repeating for Other Levels

When it's time to set up Level2_Gardens or Level3_Corridors:

1. Get the new tileset PNG
2. Copy it to `Assets/Art/Tilesets/`
3. Follow **Step B → Step C** with the new file and a new palette name
4. Open the target level scene
5. The Grid + Tilemap structure from **Step D** is the same every time
6. Paint using **Step E** with the new palette selected

Sorting Layers (**Step A**) do not need to be redone — they are project-wide settings.

---

## Common Mistakes to Avoid

| Mistake | Fix |
|---------|-----|
| Painting walls on GroundLayer (no collision) | Always check the Active Tilemap dropdown before painting |
| Collision not working after painting | Confirm WallLayer has both Tilemap Collider 2D (Merge) and Composite Collider 2D |
| Tiles look blurry | Filter Mode must be **Point (no filter)** on the texture import |
| Tiles are wrong size | Pixels Per Unit must be **16**, slice must be **16 × 16** |
| Rigidbody2D on WallLayer moves | Set the auto-added Rigidbody2D Body Type to **Static** |
