# Space Warfare

A C# experimental game to learn and test the .NET environment.

![Login Screenshot](/docs/login_screenshot.png)
![Game Screenshot](/docs/game_screenshot.png)

## Run

Start the database :
```
docker compose up -d
```

Build and run the project :
```
dotnet run --project BattleShip.API
dotnet run --project BattleShip.App
```

## Things I would change

- Custom JWT is fun but probably insecure and sessions are partially implemented.
- Game logic should be outside of the War class.
- Add CouchDB to store War history.
- Better ranking mechanism.
- Better "AI" algorithm.
- Add error handling on client request.

## Denomination
- Game is War
- Grid is Astec
- Ships are Spacecrafts
- Group of Spacecraft is a Fleet
- Attack is Beam
- Player is Commander
- AI is Cosmos

## Features
- [x] Spacecraft Orientation (North, East, South, West)
- [x] \*AI
  - [x] Minimal AI
  - [x] \*Improved AI (ugly but harder)
- [x] \*gRPC Support (Leaderboard)
- [ ] Shorter Game ID For User (Random Galaxy Name)
- [x] \*Custom User Spacecraft Placement
- [x] \*War Difficulty
  - [x] \*Grid Size
  - [x] \*Moderate and Hard AI
- [x] \*User Management
  - [x] \*Login Password - ~~Auth0~~ Custom JWT
  - [ ] \*Multiplayer - SignalR
  - [ ] \*Multiplayer - Lobby with replay
  - [ ] \*War and Beam History
  - [x] \*Security
  - [x] \*Leaderboard
- [ ] Keyboard Support
  - [ ] Vim Style
- [ ] \*Improved UI
  - [ ] Random Space Background
  - [ ] Great Grid Styling
  - [ ] Unique Spacecraft Images
  - [x] Oriented Spacecraft Images
  - [x] \*Easy Start New War
  - [ ] \*Kill Feed
- [ ] Lore Friendly Spacecraft Types names and images (Pod, Cruiser, Carrier, Explorer, Colonizer)

## Lore
- Every war happens in a random system

### Astec

Assisted Spacecraft Topology for Environmental Coordination

### Cosmos

Central Orchestrating System to Monitor and Obliterate Space

