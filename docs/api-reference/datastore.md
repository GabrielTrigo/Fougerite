# API Reference — DataStore

**Singleton:** `DataStore.GetInstance()`  
**Global in scripts:** `DataStore`  
**File:** `/Save/FougeriteDatastore.ds` (JSON format)

DataStore is Fougerite's key-value persistence system. It is **thread-safe** (protected by `ReaderWriterLock`) and supports automatic serialization of Unity types such as `Vector3` and `Color`.

---

## Internal Structure

```
DataStore
├── "table_a"
│   ├── "key1" → any value (string, int, float, list, etc.)
│   ├── "key2" → Vector3(10|20|30)   ← serialized transparently
│   └── "key3" → True
└── "table_b"
    └── "key1" → {"nested": "json"}
```

---

## CRUD

```python
# Add / update
DataStore.Add("my_plugin", "key", "value")
DataStore.Add("my_plugin", "visits", 42)
DataStore.Add("my_plugin", "position", player.Location)  # Vector3 serialized automatically

# Read
value    = DataStore.Get("my_plugin", "key")
visits   = DataStore.Get("my_plugin", "visits")
position = DataStore.Get("my_plugin", "position")  # returns Vector3

# Remove a single entry
DataStore.Remove("my_plugin", "key")

# Clear an entire table
DataStore.Flush("my_plugin")
```

---

## Checking

```python
if DataStore.ContainsKey("my_plugin", "key"):
    print("Key exists!")

if DataStore.ContainsValue("my_plugin", 42):
    print("Value found!")

count = DataStore.Count("my_plugin")
```

---

## Enumeration

```python
# All keys in a table
keys = DataStore.Keys("my_plugin")
for key in keys:
    print(key)

# All values in a table
values = DataStore.Values("my_plugin")

# Full table as Hashtable
table = DataStore.GetTable("my_plugin")

# All table names
names = DataStore.GetTableNames()
```

---

## Disk Persistence

```python
# Write to /Save/FougeriteDatastore.ds
DataStore.Save()

# Load from disk (called automatically at boot)
DataStore.Load()
```

> **Note:** Fougerite calls `DataStore.Save()` automatically on every world save. Call it manually when you have critical data to persist immediately.

---

## Supported Unity Types

Serialization is fully transparent — just store and retrieve normally:

| Unity Type | Internal Format (transparent) |
|---|---|
| `UnityEngine.Vector3` | `V3(x\|y\|z)` |
| `UnityEngine.Vector2` | `V2(x\|y)` |
| `UnityEngine.Quaternion` | `Q(x\|y\|z\|w)` |
| `UnityEngine.Color` | `C(r\|g\|b\|a)` |
| `UnityEngine.Rect` | `R(x\|y\|w\|h)` |

```python
# Save player position
DataStore.Add("homes", str(player.UID), player.Location)

# Retrieve — Vector3 is automatically reconstructed
pos = DataStore.Get("homes", str(player.UID))
player.TeleportTo(pos.x, pos.y, pos.z)
```

---

## INI ↔ DataStore Conversion

```python
ini = Plugin.CreateIni("config")

# Export a DataStore table to INI
DataStore.ToIni("my_table", ini)

# Import an INI file into DataStore
DataStore.FromIni(ini)
```

---

## Practical Examples

### Home system

```python
def On_Command(player, cmd, args):
    uid = str(player.UID)

    if cmd == "sethome":
        DataStore.Add("homes", uid, player.Location)
        DataStore.Save()
        player.Message("Home set at " + str(player.Location))

    elif cmd == "home":
        pos = DataStore.Get("homes", uid)
        if pos is None:
            player.Message("You have no home set. Use /sethome first.")
        else:
            player.TeleportTo(pos.x, pos.y, pos.z)
            player.Message("Teleported to your home!")
```

### Kill counter

```python
def On_PlayerKilled(evt):
    victim = evt.Victim
    uid    = str(victim.UID)
    deaths = DataStore.Get("deaths", uid)
    deaths = 0 if deaths is None else int(deaths)
    deaths += 1
    DataStore.Add("deaths", uid, deaths)
    victim.Message("You have died " + str(deaths) + " time(s).")

def On_Command(player, cmd, args):
    if cmd == "deaths":
        deaths = DataStore.Get("deaths", str(player.UID))
        player.Message("Your deaths: " + str(deaths if deaths is not None else 0))
```

### IP blacklist

```python
def On_PlayerApproval(evt):
    ip = evt.NetPlayer.ipAddress
    if DataStore.ContainsKey("blacklist_ips", ip):
        evt.Deny("Your IP address is banned.")
```
