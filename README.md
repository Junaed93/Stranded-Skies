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

## Backend Integration

The game acts as a frontend client that communicates with a Spring Boot backend for multiplayer features and data persistence.

### 1. Score Reporting (POST)
When a player's run ends (Game Over), the client sends a `POST` request to the backend with their performance data.
*   **Endpoint:** `POST /api/scores`
*   **Handler:** `ScoreReporter.cs`
*   **Payload (JSON):**
    ```json
    {
      "finalScore": 1500,
      "gameMode": "SinglePlayer",
      "timestamp": "2023-10-27T10:00:00Z",
      "playerId": "User123" 
    }
    ```
*   **Authentication:** The client includes a Bearer Token in the `Authorization` header. This token is auto-detected from URL parameters (WebGL) or command-line arguments (Desktop).

### 2. Real-Time Multiplayer (WebSockets)
For multiplayer sessions, the client establishes a persistent WebSocket connection to synchronize player movement and state.
*   **Endpoint:** `ws://localhost:8080/game`
*   **Handler:** `SocketClient.cs`
*   **Protocol:**
    *   **JOIN:** Client sends `{"type": "JOIN"}` to enter the lobby.
    *   **MOVE:** Client sends position updates (x, y, velocity) to be broadcast to other players.
    *   **LEAVE:** Server notifies clients when a player disconnects.

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
    *   **Core Systems**:
        *   `GameSession.cs`: Manages global game state.
        *   `ScoreManager.cs`: Handles real-time scoring.
        *   `SeedManager.cs`: Manages procedural generation seeds.
    *   **Entities (Player & Enemy)**:
        *   `HeroKnight.cs`, `PlayerCombat.cs`: Player controller and combat logic.
        *   `EnemyAI.cs`, `EnemyController.cs`: Base behavior for enemies.
        *   `Boss Scripts`: `GolemBoss.cs`, `CrabBoss.cs`, `SkullBoss.cs`.
    *   **Networking**:
        *   `SocketClient.cs`: Main WebSocket client.
        *   `SocketReceiver.cs`: Handles incoming network messages.
        *   `ScoreReporter.cs`: Handles HTTP POST requests.
    *   **World Generation**:
        *   `WorldGenerator.cs`: Main procedural generation logic.
        *   `ParallaxController.cs`: Background visual effects.
    *   **UI**:
        *   `MainMenu.cs`, `PauseMenu.cs`, `GameOverPanel.cs`: UI interaction scripts.
*   `Assets/Scenes`: Game scenes including MainMenu, Gameplay, and Boss levels.
*   `Assets/Prefabs`: Reusable game objects like the Player, Enemies, and items.
*   `Assets/Sprites`: 2D art assets for characters, environments, and UI.

## Contributing

1.  Fork the project.
2.  Create your feature branch (`git checkout -b feature/AmazingFeature`).
3.  Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4.  Push to the branch (`git push origin feature/AmazingFeature`).
5.  Open a Pull Request.
