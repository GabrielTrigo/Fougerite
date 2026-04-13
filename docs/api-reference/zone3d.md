# API Reference — Zone3D

**Class:** `Fougerite.Zone3D`  
**Global in scripts:** constructed manually; registered automatically in `World.Zones`

`Zone3D` defines a **3D volumetric area** by a 2D polygon footprint (X/Z plane) and an optional vertical range (MinY / MaxY). The polygon boundary uses a ray-casting algorithm with AABB pre-checking for performance.

Zones are used for PVP/PVE areas, safe zones, event arenas, restricted regions, and any other area-based game logic.

---

## Creating a Zone

```python
# Import the class (Python)
import clr
clr.AddReferenceByPartialName("Fougerite")
from Fougerite import Zone3D

# Create and register a named zone
zone = Zone3D("MyZone")

# Define the polygon corners (X, Z coordinates — Y is not used here)
zone.Mark(100.0, 100.0)
zone.Mark(200.0, 100.0)
zone.Mark(200.0, 200.0)
zone.Mark(100.0, 200.0)

# Optional: restrict the vertical range (default: -1000 to 5000)
zone.MinY = 0.0
zone.MaxY = 100.0

# Optional: set PVP and protection flags
zone.PVP = False       # disable PVP inside this zone
zone.Protected = True  # mark as a protected area (custom use)
```

> **Note:** Zones are registered instantly on construction. The name must be unique. Access any zone via `World.Zones["MyZone"]`.

---

## Containment Checks

```python
# Check if a Player is inside the zone
zone = World.GetWorld().zones.get("MyZone")
if zone is not None and zone.Contains(player):
    player.Message("You are inside MyZone!")

# Check if an Entity is inside
if zone.Contains(entity):
    print("Entity is in the zone")

# Check an arbitrary world position
from UnityEngine import Vector3
if zone.Contains(Vector3(150.0, 50.0, 150.0)):
    print("Position is inside")
```

---

## Access from World

```python
# Get all registered zones
zones = World.GetWorld().zones  # Dictionary<string, Zone3D>

# Check membership
if "MyZone" in zones:
    zone = zones["MyZone"]

# Iterate all zones
for name, z in zones.items():
    print(name, "PVP:", z.PVP, "Protected:", z.Protected)
```

---

## Visual Markers

Spawns metal pillars at each corner of the zone polygon so players can see the outline.

```python
# Show markers in the world
zone.ShowMarkers()

# Remove markers
zone.HideMarkers()
```

---

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `PVP` | `bool` | `True` | Whether PVP is allowed inside the zone |
| `Protected` | `bool` | `False` | Custom flag for protected area logic |
| `MinY` | `float` | `-1000` | Minimum altitude boundary |
| `MaxY` | `float` | `5000` | Maximum altitude boundary |
| `Points` | `List<Vector2>` | — | List of polygon corner points (X, Z) |

---

## Practical Example — Safe Zone

```python
safe_zone = None

def On_PluginInit():
    global safe_zone
    safe_zone = Zone3D("SafeZone")
    safe_zone.Mark(0.0,   0.0)
    safe_zone.Mark(500.0, 0.0)
    safe_zone.Mark(500.0, 500.0)
    safe_zone.Mark(0.0,   500.0)
    safe_zone.PVP = False

def On_PlayerHurt(evt):
    if not evt.AttackerIsPlayer or not evt.VictimIsPlayer:
        return

    victim   = evt.Victim
    attacker = evt.Attacker

    if safe_zone is not None and safe_zone.Contains(victim):
        evt.DamageAmount = 0  # cancel damage inside safe zone
        attacker.Message("You cannot attack players inside the Safe Zone!")
```

---

## Practical Example — Dynamic Zone on Command

```python
def On_Command(player, cmd, args):
    if cmd == "createzone" and player.Admin:
        if len(args) < 1:
            player.Message("Usage: /createzone <name>")
            return

        name = args[0]
        z = Zone3D(name)
        # Create a 200x200 zone centered on the player
        x = player.X
        zc = player.Z
        z.Mark(x - 100, zc - 100)
        z.Mark(x + 100, zc - 100)
        z.Mark(x + 100, zc + 100)
        z.Mark(x - 100, zc + 100)
        z.ShowMarkers()
        player.Message("Zone '" + name + "' created around you.")
```
