# API Reference — BasePlugin

**Class:** `Fougerite.PluginLoaders.BasePlugin`  
**Global in scripts:** `Plugin`

Abstract base class for all plugins across all languages. Provides timers, file I/O, logging, cross-plugin calls, and thread-safe collection factories.

---

## Plugin Identity

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Plugin name (filename without extension) |
| `Author` | `string` | Author defined in plugin metadata |
| `Version` | `string` | Version defined in plugin metadata |
| `About` | `string` | Description defined in plugin metadata |
| `Type` | `PluginType` | Enum: `CSharp`, `Python`, `JavaScript`, `Lua` |
| `RootDir` | `DirectoryInfo` | The plugin's directory |
| `State` | `PluginState` | `Loaded`, `FailedToLoad`, `Unloaded` |
| `HasErrors` | `bool` | Whether an exception occurred during execution |
| `LastError` | `string` | Last error message |
| `DontReload` | `bool` | If `true`, the plugin is skipped on `fougerite.reload` |

---

## Timers

Timers are backed by Unity `MonoBehaviour` and survive scene changes. They are automatically destroyed when the plugin is unloaded.

### Simple Timer (one per name)

```python
# Fire once after 5 seconds
timer = Plugin.CreateTimer("MyTimer", 5000)
# → calls MyTimerCallback(evt) when it fires

# Repeating timer (every 10 seconds)
timer = Plugin.CreateTimer("RepeatTimer", 10000, True)

# Timer with a max fire count (auto-kills itself)
timer = Plugin.CreateTimer("LimitedTimer", 1000, True, 5)

# Check if a timer exists
t = Plugin.GetTimer("MyTimer")

# Kill a specific timer
Plugin.KillTimer("MyTimer")

# Kill all timers
Plugin.KillTimers()
```

### Parallel Timer (multiple with the same name)

Useful for per-player or per-entity delayed actions:

```python
args = Plugin.CreateDict()
args["player"] = player
args["reason"] = "teleport"
timer = Plugin.CreateParallelTimer("PlayerTimer", 3000, args)
# → calls PlayerTimerCallback(evt) with evt.Args["player"]

# Kill all parallel timers with the same name
Plugin.KillParallelTimer("PlayerTimer")
```

### Timer Callback (Python)

```python
def MyTimerCallback(evt):
    Server.Broadcast("Timer fired!")
    evt.Kill()  # optional: destroy the timer after execution

def PlayerTimerCallback(evt):
    player = evt.Args["player"]
    player.Message("Your action was processed!")
```

---

## INI Files (Configuration)

```python
# Open or create a config file
ini = Plugin.CreateIni("config")       # creates config.ini if missing
ini = Plugin.GetIni("config")          # opens existing

# Read settings
val = ini.GetSetting("General", "MaxPlayers")

# Write settings
ini.AddSetting("General", "MaxPlayers", "50")
ini.Save()

# Get all .ini files in a subdirectory
inis = Plugin.GetInis("data")
```

---

## JSON Files

```python
import json

# Save data as JSON
data = {"key": "value", "number": 42}
Plugin.ToJsonFile("data", json.dumps(data))

# Load data from JSON
if Plugin.JsonFileExists("data"):
    raw  = Plugin.FromJsonFile("data")
    data = json.loads(raw)

# Delete JSON file
Plugin.DeleteJsonFile("data")
```

---

## Plugin Logging

```python
# Append a timestamped line to a log file
Plugin.Log("events", "Player connected: " + player.Name)

# Rotate logs (keep up to N backups)
Plugin.RotateLog("events", 6)  # keeps events.log + 6 backups

# Delete log file
Plugin.DeleteLog("events")
```

---

## Cross-Plugin Interaction

```python
# Get a reference to another loaded plugin
other = Plugin.GetPlugin("OtherPluginName")

# Call a method on another plugin
if other is not None:
    other.Invoke("On_PlayerConnected", player)
```

---

## Thread-Safe Collection Factories

Use these when working with data accessed by timers or multiple threads:

```python
d  = Plugin.CreateDict()               # Dictionary<string, object>
ds = Plugin.CreateStringDict()         # Dictionary<string, string>
cd = Plugin.CreateConcurrentDict()     # ConcurrentDictionary<string, object>

list_  = Plugin.CreateList()           # List<object>
clist  = Plugin.CreateConcurrentList() # ConcurrentList<object>
rwl    = Plugin.CreateReaderWriterLock()
```

---

## Global Shared Data

```python
# Shared across all plugins (not persisted)
Plugin.GlobalData["my_key"] = "value"
val = Plugin.GlobalData["my_key"]
```

---

## Path Safety

All file operations are validated by `ValidateRelativePath()`. It is **not possible** to access files outside the plugin's own directory (path traversal protection).

```python
# OK — inside the plugin directory
Plugin.Log("events", "test")           # → /Plugins/MyPlugin/events.log
Plugin.ToJsonFile("save", "...")       # → /Plugins/MyPlugin/save.json

# ERROR — path traversal blocked
Plugin.Log("../../etc/passwd", "hack") # raises an exception
```
