---
description: Unity WebGL to Spring Boot Backend API Integration Reference
---

# Stranded Skies - Unity ↔ Backend API Reference

This document explains how the Unity WebGL game communicates with the Spring Boot backend.

## Server Configuration

| Service | URL | Port |
|---------|-----|------|
| **Backend (Spring Boot)** | `http://localhost:8080` | 8080 |
| **Frontend (Web Launcher)** | `http://localhost:3000` | 3000 |

---

## REST API Endpoints

### 1. Submit Score
**Used by:** `ScoreReporter.cs` (line 121)

```
POST http://localhost:8080/api/scores
```

**Headers:**
```
Content-Type: application/json
Authorization: Bearer {authToken}
```

**Request Body:**
```json
{
  "finalScore": 1500,
  "gameMode": "SinglePlayer",
  "timestamp": "2026-01-27T00:00:00.000Z",
  "playerId": "LocalPlayer"
}
```

**Response:** Success/Error message

---

### 2. Get Leaderboard
**Used by:** Frontend leaderboard page

```
GET http://localhost:8080/api/leaderboard
```

**Response:**
```json
[
  { "username": "Player1", "score": 5000, "timestamp": "..." },
  { "username": "Player2", "score": 4500, "timestamp": "..." }
]
```

---

### 3. Guest Login
**Used by:** Frontend login page

```
POST http://localhost:8080/auth/guest
```

**Response:**
```json
{
  "token": "jwt-token-here",
  "username": "Guest_12345"
}
```

---

## WebSocket Endpoints

### Multiplayer Game Sync
**Used by:** `SocketClient.cs` (line 15)

```
ws://localhost:8080/game
```

**Message Types:**

#### JOIN (Client → Server)
Sent when player connects:
```json
{ "type": "JOIN" }
```

#### MOVE (Bidirectional)
Sent continuously during gameplay:
```json
{
  "type": "MOVE",
  "id": "player-uuid",
  "x": 10.5,
  "y": 2.3,
  "velX": 1.5,
  "grounded": true
}
```

#### LEAVE (Server → Client)
Broadcast when a player disconnects:
```json
{
  "type": "LEAVE",
  "id": "player-uuid"
}
```

---

## Unity Scripts Reference

### ScoreReporter.cs
- **Purpose:** Sends score to backend when game ends
- **Auth:** Reads token from URL param `?token=XXX` or command line `--auth-token XXX`
- **Endpoint:** `POST /api/scores` on port 8080
- **Login Redirect:** `http://localhost:3000/login` (if no token)

### SocketClient.cs
- **Purpose:** Real-time multiplayer sync via WebSocket
- **Endpoint:** `ws://localhost:8080/game`
- **Messages:** JOIN, MOVE, LEAVE
- **Spawns:** Remote players as `RemotePlayerController`

---

## Authentication Flow

```
1. User visits http://localhost:3000 (Frontend)
2. User logs in or clicks "Play as Guest"
3. Frontend gets JWT token from backend
4. Frontend launches WebGL game with ?token=JWT_HERE
5. Unity reads token from URL (ScoreReporter.cs line 43-51)
6. Unity uses token for authenticated API calls
```

---

## Common Issues

| Issue | Solution |
|-------|----------|
| Leaderboard shows "Offline" | Make sure backend is running on port 8080 |
| Scores not saving | Check Unity console for auth errors, ensure token is passed |
| Multiplayer not working | Verify WebSocket endpoint `/game` exists on backend |
| CORS errors | Backend needs to allow origin `http://localhost:3000` |

---

## Backend Requirements

The Spring Boot backend must implement:

1. **REST Controller:**
   - `POST /api/scores` - Save score (protected, needs JWT)
   - `GET /api/leaderboard` - Get all scores
   - `POST /auth/guest` - Create guest user and return JWT

2. **WebSocket Handler:**
   ```java
   @ServerEndpoint("/game")
   public class GameWebSocketHandler {
       // Handle JOIN, MOVE, LEAVE messages
       // Broadcast MOVE to all connected clients
   }
   ```

3. **CORS Configuration:**
   - Allow origin: `http://localhost:3000`
   - Allow credentials: true
   - Allow headers: Content-Type, Authorization

---

## Quick Test Commands

```bash
# Test if backend is running
curl http://localhost:8080/api/leaderboard

# Test guest login
curl -X POST http://localhost:8080/auth/guest

# Test score submission (replace TOKEN)
curl -X POST http://localhost:8080/api/scores \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{"finalScore":100,"gameMode":"SinglePlayer"}'
```
