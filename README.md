# Summer

Summer is a 2D platformer built in Unity with an emphasis on responsive movement, readable feedback, checkpoint-based progression, collectibles, and data-driven level selection.

## Highlights

- Acceleration and deceleration-based horizontal movement.
- Variable-height jumping, coyote time, double jump, wall slide, and wall jump.
- Wall-jump input locking and wall coyote time for more forgiving controls.
- Landing squash, aerial spin, death particles, and animated level exits.
- Room-bounded smooth camera transitions.
- Checkpoints, respawning, collectible coins, and persistent level progress.
- Normal and modified level variants configured through a `LevelCatalog` ScriptableObject.
- Editor utilities for checkpoint placement, room bounds, and automatic repair of missing or duplicate coin IDs.

## Controls

| Input | Action |
| --- | --- |
| `A` / `D` or Left / Right | Move |
| `W`, Up, or Space | Jump; release early for a shorter jump |
| `S` while wall sliding | Fast slide |
| `R` | Respawn at the active checkpoint |
| `Esc` | Quit |

## Project Structure

- `Assets/Code/PlatformerPlayerController.cs` - movement, jump, wall interaction, and animation feedback.
- `Assets/Code/PlayerCheckpointController.cs` - death effects, checkpoint state, and respawning.
- `Assets/Code/CollectableCoin.cs` - collectible identity, follow behavior, and visual feedback.
- `Assets/Code/GameProgressStore.cs` - JSON progress stored through `PlayerPrefs`.
- `Assets/Code/RoomCameraController.cs` - smooth following constrained to room bounds.
- `Assets/Code/Editor/` - custom tools for level-authoring support.

## Run the Project

1. Install Unity `6000.3.10f1`.
2. Clone the repository and open it in Unity Hub.
3. Open `Assets/Scenes/Level Select.unity` or a level scene.
4. Enter Play Mode.

A packaged Windows build is also included as `Build.zip`.

## Tech Stack

Unity, C#, Unity Input System, 2D Physics, ScriptableObject, custom Unity Editor tooling, JSON, and `PlayerPrefs`.

