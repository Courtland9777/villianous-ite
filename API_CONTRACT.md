# API_CONTRACT.md — HTTP & SignalR
_Date: 2025-08-25_

This document defines the API surface for the Villainous web app. The API follows **REST for stateful queries** and **SignalR for realtime gameplay events**. All errors return **ProblemDetails** (RFC 9457).

---

## REST Endpoints

### POST /api/matches
**Description**: Create a new match.

Request:
```json
{
  "villains": ["Ursula", "Hook"],
  "seed": 12345
}
```

Response `200 OK`:
```json
{
  "matchId": "f3b6f28a-07a2-4ec6-9b1a-32e9c9ac08e1",
  "seed": 12345,
  "state": { /* GameStateDto redacted for caller */ }
}
```

---

### GET /api/matches/{id}/state
**Description**: Retrieve the current game state snapshot (redacted for calling player).

Response:
```json
{
  "matchId": "f3b6f28a-07a2-4ec6-9b1a-32e9c9ac08e1",
  "villain": "Ursula",
  "power": 3,
  "locations": [
    {
      "id": "ursula-lair",
      "actions": ["PlayCard","Vanquish"],
      "heroes": [],
      "allies": [],
      "items": []
    }
  ],
  "handCount": 4,
  "discardCount": 2
}
```

---

### GET /api/matches/{id}/replay
**Description**: Retrieve the full replay log (for authorized participants).

Response: array of domain events.
```json
[
  { "playerId": "guid", "location": "forest", "hero": "robin" }
]
```

---

### POST /api/matches/{id}/commands
**Description**: Submit a command to the match.

Request:
```json
{
  "type": "Fate",
  "playerId": "11111111-1111-1111-1111-111111111111",
  "targetPlayerId": "22222222-2222-2222-2222-222222222222",
  "card": "ariel"
}
```

Response `200 OK`:
```json
{ "accepted": true }
```

Error Response `400 Bad Request` for unknown command types.

---

## SignalR Hub: `/hub`

### Methods
- `JoinMatch(matchId)`  
  - Adds connection to group, sends **MatchJoined** with snapshot.  
- `SendCommand(matchId, CommandDto)`  
  - Validates, applies to state, broadcasts **StateUpdated**.  
- `LeaveMatch(matchId)`  
  - Removes from group.

### Events
- **MatchJoined**  
  ```json
  { "matchId": "f3b6f28a", "state": { /* snapshot */ } }
  ```
- **StateUpdated**  
  ```json
  { "matchId": "f3b6f28a", "events": [ { "seq": 6, "type": "HeroDefeated", "payload": { "heroId": "ariel" } } ] }
  ```
- **CommandRejected**  
  ```json
  { "code": "rules.illegal_action", "message": "You cannot Fate yourself.", "traceId": "..." }
  ```

---

## Error Codes
- `rules.illegal_action` — action not legal in current state → 400  
- `rules.invalid_target` — target not valid for action → 400  
- `rules.conflict` — command conflicts with newer state → 409  
- `engine.invariant_violation` — internal bug detected → 500  

---

## Notes
- All responses include `traceId` for correlation.  
- REST versioning via `api-version` header (e.g., `1.0`).  
- SignalR payloads carry `version` property for forward compatibility.  
