# Stranded Skies

Stranded Skies is an action-packed 2D platformer/RPG built in Unity. Take control of the Hero Knight, battle fierce enemies, challenge powerful bosses, and explore procedurally generated worlds. The game features both single-player and multiplayer modes.

## Features

*   **Dynamic Combat:** Utilize a combo-based attack system, defensive blocking, and dodge rolls to master combat (`PlayerCombat.cs`, `CombatSystem.cs`).
*   **Multiplayer:** Connect with friends and play together online. Powered by a custom socket-based networking system (`SocketClient.cs`, `SocketReceiver.cs`, `MultiplayerBootstrap.cs`).
*   **Epic Boss Fights:** Face off against challenging bosses:
    *   **Golem:** A heavy hitter (`GolemBoss.cs`).
    *   **Giant Crab:** A crustacean menace (`CrabBoss.cs`).
    *   **Skull Guard:** A skeletal warrior (`SkullBoss.cs`).
*   **Procedural Generation:** Experience new levels every time with robust world generation (`WorldGenerator.cs`) and seed management (`SeedManager.cs`).
*   **Diverse Enemies:** Fight a variety of enemies with unique AI (`EnemyAI.cs`, `EnemyController.cs`) and spawner systems (`EnemySpawner.cs`).
*   **Score System:** Track your progress and compete for the best scores using the integrated scoring system (`ScoreManager.cs`, `ScoreReporter.cs`).

## Controls

| Action | Input |
| :--- | :--- |
| **Move** | `A` / `D` or `Left Arrow` / `Right Arrow` |
| **Jump** | `Space` |
| **Attack** | `Left Mouse Button` (Tap for Combo) |
| **Block** | `Right Mouse Button` |
| **Roll** | `Left Shift` |

## Getting Started

### Prerequisites

*   **Unity:** You will need a compatible version of the Unity Editor to open and run this project (check `ProjectSettings/ProjectVersion.txt` for exact version, likely 2021.3+).
*   **Backend (Multiplayer):** The multiplayer functionality relies on a Java Spring Boot backend. Ensure the backend server is running for multiplayer features to work.

### Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/YourUsername/Stranded-Skies.git
    ```
2.  **Open in Unity:**
    *   Launch Unity Hub.
    *   Click "Open" and select the `Stranded-Skies` folder.
3.  **Run the Game:**
    *   Open the `MainMenu` scene located in `Assets/Scenes`.
    *   Press the **Play** button in the Unity Editor.

## Project Structure

*   `Assets/script`: Contains all C# scripts for game logic.
    *   **AI & Enemies**: `EnemyAI.cs`, `BossWall.cs`, various individual boss scripts.
    *   **Player**: `HeroKnight.cs`, `PlayerCombat.cs`, `PlayerHealth.cs`.
    *   **Networking**: `SocketClient.cs`, `RemotePlayerController.cs`.
    *   **World**: `WorldGenerator.cs`, `ParallaxController.cs`, `FloatingPlatform.cs`.
    *   **UI & System**: `MainMenu.cs`, `PauseMenu.cs`, `ScoreUI.cs`.
*   `Assets/Scenes`: Game scenes including MainMenu, Gameplay, and Boss levels.
*   `Assets/Prefabs`: Reusable game objects like the Player, Enemies, and items.
*   `Assets/Sprites`: 2D art assets for characters, environments, and UI.

## Contributing

1.  Fork the project.
2.  Create your feature branch (`git checkout -b feature/AmazingFeature`).
3.  Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4.  Push to the branch (`git push origin feature/AmazingFeature`).
5.  Open a Pull Request.
