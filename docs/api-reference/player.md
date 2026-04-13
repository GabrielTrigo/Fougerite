# API Reference — Fougerite.Player

**Type:** Per-player instance  
**Global in scripts:** `player` (parameter passed in hooks)

Represents a connected player. This is the most commonly used class in plugin development.

---

## Identification

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Steam display name |
| `SteamID` | `string` | Steam ID as string |
| `UID` | `ulong` | Steam ID as numeric (useful in arithmetic and DataStore keys) |
| `GameID` | `string` | Alias for SteamID |
| `IP` | `string` | Client IP address |
| `Ping` | `int` | Current ping in milliseconds |
| `TimeOnline` | `long` | Seconds online since last connect |

---

## State and Status

| Property | Type | Description |
|---|---|---|
| `Health` | `float` | Current health (0–100) |
| `MaxHealth` | `float` | Maximum health |
| `IsOnline` | `bool` | Whether the player is currently connected |
| `Admin` | `bool` | Whether the player is a server admin |
| `Moderator` | `bool` | Whether the player is a server moderator |
| `IsBleeding` | `bool` | Bleeding status |
| `IsCold` | `bool` | Cold status |
| `IsInjured` | `bool` | Injured status |
| `IsRadiated` | `bool` | Radiated status |
| `IsStarving` | `bool` | Starving status |
| `IsThirsty` | `bool` | Thirsty status |

---

## Position

| Property | Type | Description |
|---|---|---|
| `Location` | `Vector3` | Current world position |
| `X` | `float` | X coordinate |
| `Y` | `float` | Y coordinate (height) |
| `Z` | `float` | Z coordinate |

---

## Messaging and Notifications

```python
# Chat bubble from the server
player.Message("Welcome to the server!")

# Chat with a custom sender name
player.MessageFrom("SYSTEM", "Your account has been verified.")

# Center-screen notice
player.Notice("⚠ Restricted area!")

# Inventory notification (bottom-right corner)
player.InventoryNotice("You received a gift!")
```

---

## Teleportation

```python
# Teleport to coordinates
player.TeleportTo(100.0, 50.0, 200.0)

# Teleport to another player
player.TeleportTo(otherPlayer)

# Safe teleport (avoids spawning inside structures)
success = player.SafeTeleportTo(100.0, 50.0, 200.0)
```

---

## Combat and Health

```python
# Deal damage
player.Damage(25.0)    # removes 25 health

# Heal (set Health directly)
player.Health += 50.0  # restores 50 health (capped at MaxHealth)

# Kill instantly
player.Kill()
```

---

## Inventory

```python
inv = player.Inventory

# Give items
inv.AddItem("Wood", 500)
inv.AddItem("Metal Fragments", 200)

# Remove items
inv.RemoveItem("Wood", 100)

# Check blueprint
if player.HasBlueprint(dataBlock):
    player.Message("You already know this blueprint!")
```

---

## Command Access Control

```python
# Block specific chat commands for this player
# Note: CommandCancelList returns a shallow copy — mutating it has no effect.
# Use the dedicated methods instead:
player.RestrictCommand("tp")
player.RestrictCommand("home")

# Unblock a command
player.UnRestrictCommand("tp")

# Block console commands
player.RestrictConsoleCommand("kick")
player.UnRestrictConsoleCommand("kick")
```

---

## Kick and Administration

```python
# Send a message then disconnect (there is no Kick(reason) method)
player.Message("You were kicked for inappropriate behavior.")
player.Disconnect()

# Disconnect without a notification
player.Disconnect(False)

# Send a console command to the player's client
player.SendCommand("hurtme 100")
```

---

## Static Lookup Methods

```python
# Not directly available in scripts — use Server.FindPlayer() instead.
# In native C#:
Player p = Player.FindBySteamID("76561198000000000");
Player p = Player.FindByName("Steve");
Player p = Player.FindByNetworkPlayer(np);
Player p = Player.FindByPlayerClient(pc);
```

---

## Practical Examples

### Welcome system

```python
def On_PlayerConnected(player):
    player.Message("Welcome, " + player.Name + "!")

    visits = DataStore.Get("visits", str(player.UID))
    if visits is None:
        visits = 0
        player.Notice("First time here? Enjoy your stay!")

    DataStore.Add("visits", str(player.UID), int(visits) + 1)
    Server.Broadcast(player.Name + " joined the server!")

def On_PlayerDisconnected(player):
    Server.Broadcast(player.Name + " left the server.")
```

### Teleport command

```python
def On_Command(player, cmd, args):
    if cmd == "tp" and player.Admin:
        if len(args) >= 3:
            x, y, z = float(args[0]), float(args[1]), float(args[2])
            player.TeleportTo(x, y, z)
            player.Message("Teleported to " + args[0] + ", " + args[1] + ", " + args[2])
```
