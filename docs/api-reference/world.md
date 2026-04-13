# API Reference — Fougerite.World

**Singleton:** `World.GetWorld()`  
**Global in scripts:** `World`

Manages the server environment: airdrops, entity spawning, zones, day/night cycle, and world saves.

---

## Airdrops

```python
# Airdrop at a random location
World.Airdrop()

# Airdrop at exact coordinates (repeated N times)
World.AirdropAtOriginal(200.0, 0.0, 300.0, 1)

# Airdrop at a player's location
World.AirdropAtOriginal(player, 1)
```

---

## Entity Spawning

```python
# Spawn a prefab (returns raw Unity GameObject)
obj = World.Spawn("Assets/Prefabs/Deployable/Campfire/campfire.prefab",
                   UnityEngine.Vector3(100, 50, 200))

# Spawn and get a wrapped Entity object
entity = World.SpawnEntity(
    "Assets/Prefabs/Deployable/LargeWoodStorage/large_Wood_storage.prefab",
    UnityEngine.Vector3(100, 50, 200))
```

---

## Terrain and Elevation

```python
# Get the ground Y at given X and Z
groundY = World.GetGround(100.0, 200.0)

# Exact terrain height at a point
height = World.GetTerrainHeight(player.Location)

# Terrain steepness (0.0 = flat, 1.0 = vertical)
steepness = World.GetTerrainSteepness(player.Location)
```

---

## Day/Night Cycle

```python
# Get current day length in seconds
length = World.DayLength

# Set day length
World.DayLength = 3600.0  # 1 real hour per in-game day
```

---

## Zones

```python
# Create a named zone
zone = World.CreateZone("SpawnZone")
# Configure via Zone3D API...

# Access existing zones
zones = World.Zones  # Dictionary<string, Zone3D>
if "SpawnZone" in zones:
    my_zone = zones["SpawnZone"]
```

---

## Active Entities

```python
# Access all cached entities via EntityCache
from Fougerite.Caches import EntityCache
cache = EntityCache.GetInstance()
for entity in cache.Values:
    print(entity.Name, entity.Location)
```

---

## World Save

```python
# Blocking save (waits until complete)
World.ServerSaveHandler.ManualSave()

# Background save (non-blocking)
World.ServerSaveHandler.ManualBackGroundSave()
```

> **⚠️ Note:** The `CrucialSavePoint` window (configurable in minutes in `Fougerite.cfg`) blocks manual saves when an automatic save is imminent, preventing data corruption. Use `ManualBackGroundSave()` to avoid blocking the server.

---

## Practical Examples

### Admin airdrop command

```python
def On_Command(player, cmd, args):
    if cmd == "airdrop" and player.Admin:
        World.AirdropAtOriginal(player, 1)
        Server.Broadcast("Airdrop called by " + player.Name + "!")
```

### Check if player is on the ground

```python
def On_PlayerMove(player):
    groundY = World.GetGround(player.X, player.Z)
    if player.Y - groundY > 5.0:
        player.Notice("You appear to be flying!")
```
