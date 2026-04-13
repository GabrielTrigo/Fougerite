# API Reference — Fougerite.Server

**Singleton:** `Server.GetServer()`  
**Global in scripts:** `Server`

Manages the global server state: player list, ban system, broadcasting, and console commands.

---

## Properties

| Property | Type | Description |
|---|---|---|
| `Players` | `List<Player>` | Online players (thread-safe shallow copy) |
| `PlayersCache` | `Dictionary<ulong, Player>` | History of all players seen since boot |
| `Sleepers` | `List<Sleeper>` | Active sleepers on the server |
| `Version` | `string` | Current Fougerite version string |
| `ServerLoaded` | `bool` | Whether the server is fully initialized |
| `HasRustPP` | `bool` | Whether the RustPP plugin is loaded |
| `MaxPlayers` | `int` | Configured player slot limit |
| `Hostname` | `string` | Server display name |
| `Map` | `string` | Current map name |

---

## Finding Players

```python
# By name (fuzzy match using Levenshtein Distance)
player = Server.FindPlayer("Steve")

# By SteamID as ulong
player = Server.FindPlayer(76561198000000000)

# By SteamID as string
player = Server.FindPlayer("76561198000000000")
```

```csharp
// C#
Player p = Server.GetServer().FindPlayer("Steve");
Player p = Server.GetServer().FindPlayer(76561198000000000UL);
```

---

## Messaging and Broadcast

```python
# Broadcast to all players
Server.Broadcast("Server restart in 5 minutes!")

# Broadcast with a custom sender name
Server.BroadcastFrom("SERVER", "Welcome everyone!")

# Center-screen notice for all players
Server.BroadcastNotice("⚠ Restart in 5 minutes")

# Inventory notification (bottom-right corner) for all
Server.BroadcastInv("Special item now available!")
```

---

## Ban System

```python
# Ban an online player (notify admins only, no broadcast)
Server.BanPlayer(player, "Admin", "Cheating")

# Ban and broadcast the ban to all players
Server.BanPlayer(player, "Admin", "Cheating", None, True)

# Ban by IP and SteamID (works offline too)
Server.BanPlayerIPandID("192.168.1.100", "76561198000000000", "Reason")

# Check ban status
if Server.IsBannedID("76561198000000000"):
    print("Player is banned!")

if Server.IsBannedIP("192.168.1.100"):
    print("IP is banned!")

# Remove bans
Server.UnbanByID("76561198000000000")
Server.UnbanByIP("192.168.1.100")
Server.UnbanByName("PlayerName")
```

---

## Server Commands

```python
# Execute a console command on the server
Server.RunServerCommand("env.time 12")
Server.RunServerCommand("quit")

# Restrict a console command globally
Server.RestrictConsoleCommand("quit")
```

---

## World Save

```python
Server.Save()  # saves the world (respects CrucialSavePoint window)
```

---

## Practical Examples

### List online players

```python
def On_Command(player, cmd, args):
    if cmd == "list":
        players = Server.Players
        player.Message("Online: " + str(len(players)) + " players")
        for p in players:
            player.Message("  - " + p.Name + " (" + str(p.UID) + ")")
```

### Broadcast on world save

```python
def On_ServerSaved(count, secs):
    Server.Broadcast("World saved! (" + str(count) + " entities in " + str(secs) + "s)")
```
