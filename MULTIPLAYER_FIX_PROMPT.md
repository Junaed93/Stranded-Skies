# Multiplayer Fix Prompt — Stranded Skies

## THE PROBLEM
My Unity WebGL game "Stranded Skies" has multiplayer mode. When I open two browser instances of the WebGL build, the second player does NOT spawn in the first player's game (and vice versa). Multiplayer is completely broken.

## MY SETUP
- **Backend:** Spring Boot (Java), running on `http://localhost:8080`
- **Frontend/Web Launcher:** Running on `http://localhost:3000`
- **Game:** Unity WebGL build, served from the frontend

## WHAT THE UNITY SIDE EXPECTS (ALREADY BUILT)

### 1. WebSocket Endpoint: `ws://localhost:8080/game`
The Unity game connects via WebSocket when Multiplayer mode is selected.

**File:** `SocketClient.cs` — connects to `ws://localhost:8080/game`

### 2. Message Protocol (JSON over WebSocket)
Unity sends and expects these exact JSON message types:

#### CLIENT → SERVER: JOIN
Sent when a player connects:
```json
{"type":"JOIN"}
```

#### SERVER → CLIENT: MOVE (broadcast to all OTHER clients)
When one client sends a MOVE, the server must broadcast it to all other connected clients with the sender's ID:
```json
{
  "type": "MOVE",
  "id": "unique-player-id",
  "x": 10.5,
  "y": 2.3,
  "velX": 1.5,
  "grounded": true
}
```

#### CLIENT → SERVER: MOVE
Sent continuously by the local player during gameplay:
```json
{"type":"MOVE","x":10.5,"y":2.3,"velX":1.5,"grounded":true}
```
**Note:** The client does NOT include its own `id` — the server must assign one and attach it before broadcasting.

#### SERVER → CLIENT: LEAVE
Broadcast when a player disconnects:
```json
{
  "type": "LEAVE",
  "id": "disconnected-player-id"
}
```

### 3. How Unity Handles These Messages
- **On MOVE received:** If the `id` is unknown, it **spawns a new remote player** (`RemotePlayerController`). If known, it **updates position** via smooth interpolation.
- **On LEAVE received:** It **destroys** the remote player GameObject.
- **On JOIN sent:** Just announces presence; the server should start relaying MOVE messages from other players.

### 4. REST API Endpoints (also need to work)

#### POST `http://localhost:8080/api/scores`
```
Headers:
  Content-Type: application/json
  Authorization: Bearer {jwt-token}

Body:
{
  "finalScore": 1500,
  "gameMode": "SinglePlayer",
  "timestamp": "2026-01-27T00:00:00.000Z",
  "playerId": "LocalPlayer"
}
```

#### GET `http://localhost:8080/api/leaderboard`
Returns array of scores.

#### POST `http://localhost:8080/auth/guest`
Creates a guest user and returns JWT token.

---

## WHAT YOU NEED TO DO

### Step 1: Check if the backend has a WebSocket handler at `/game`
Look in the Spring Boot project for a WebSocket configuration. It needs:
- A WebSocket endpoint at `/game`
- Session management (track connected clients with unique IDs)
- Message routing (broadcast MOVE messages from one client to all OTHER clients)
- Disconnect handling (broadcast LEAVE when a client disconnects)

### Step 2: If WebSocket handler is MISSING, create one
The backend needs these classes:

1. **WebSocket Config** — Register the `/game` endpoint with allowed origins `http://localhost:3000`
2. **Game WebSocket Handler** — Handle:
   - `afterConnectionEstablished` → assign unique ID, store session
   - `handleTextMessage` → parse JSON, if type is `MOVE`, attach the sender's ID and broadcast to all other sessions
   - `afterConnectionClosed` → broadcast `LEAVE` with disconnected player's ID, remove session

### Step 3: Ensure CORS is configured
The backend must allow:
- Origin: `http://localhost:3000`
- WebSocket origin: `http://localhost:3000`
- Methods: GET, POST, OPTIONS
- Headers: Content-Type, Authorization
- Credentials: true

### Step 4: Test
1. Start backend on port 8080
2. Open WebGL game in two browser tabs
3. Select "Multiplayer" in both
4. Check browser console (F12) for:
   - `[SocketClient] Connected!` — means WebSocket connected
   - `[SocketClient] Spawned Remote Player: {id}` — means second player appeared
5. Move in one tab, check if the player moves in the other tab

---

## KEY FILES IN UNITY (DO NOT MODIFY THESE — BACKEND MUST MATCH)

| File | Purpose |
|------|---------|
| `SocketClient.cs` | WebSocket client, connects to `ws://localhost:8080/game`, sends JOIN/MOVE, handles incoming MOVE/LEAVE |
| `RemotePlayerController.cs` | Controls remote player GameObject, receives position updates via `UpdateState()` |
| `SocketReceiver.cs` | Stub file with event handlers (currently just logging, not connected) |
| `GameSession.cs` | Tracks game mode (SinglePlayer/Multiplayer) |
| `MainMenu.cs` | Sets game mode and loads scene |
| `PlayerSpawner.cs` | Spawns local player |
| `ScoreReporter.cs` | Posts score to `POST /api/scores` with JWT auth |

---

## SUMMARY
The Unity game client is fully built and expects a WebSocket at `ws://localhost:8080/game`. The Spring Boot backend needs a WebSocket handler that:
1. Accepts connections at `/game`
2. Assigns unique IDs to each connected client
3. When it receives `{"type":"MOVE","x":...,"y":...,"velX":...,"grounded":...}` from a client, it adds the sender's `"id"` and broadcasts to ALL OTHER clients
4. When a client disconnects, broadcasts `{"type":"LEAVE","id":"..."}` to remaining clients
5. Allows CORS/WebSocket origin from `http://localhost:3000`
