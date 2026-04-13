# Data & Persistence

Fougerite provides five persistence mechanisms for plugins, each suited for a different purpose.

---

## Overview

| Mechanism | Best For | Thread-safe | Storage |
|---|---|---|---|
| **DataStore** | Shared cross-plugin data; player data | ✅ Yes | JSON (`/Save/FougeriteDatastore.ds`) |
| **INI** | Plugin configuration files | ❌ No | `.ini` (plugin folder) |
| **JSON** | Structured plugin data | ❌ No | `.json` (plugin folder) |
| **MySQL** | Relational data, high volume | Depends on driver | External DB |
| **SQLite** | Local relational data | Depends on driver | Local `.db` file |

---

## 1. DataStore — Global KV Store

See the [complete DataStore reference](api-reference/datastore.md) for all methods, supported types, and examples.

**Use when:**
- Storing player data that must persist across sessions
- Sharing data between multiple plugins
- Positions, counters, per-UID settings
- Home systems, ban lists, VIP status, kits, etc.

---

## 2. INI Files — Plugin Configuration

Ideal for settings that server operators can edit without restarting the server.

```python
def On_PluginInit():
    global config_ini
    config_ini = Plugin.CreateIni("config")

    if config_ini.GetSetting("General", "MaxPlayers") is None:
        config_ini.AddSetting("General", "MaxPlayers", "50")
        config_ini.AddSetting("General", "WelcomeMessage", "Welcome!")
        config_ini.AddSetting("Commands", "AllowTP", "true")
        config_ini.Save()
```

**Generated file** (`/Plugins/MyPlugin/config.ini`):
```ini
[General]
MaxPlayers=50
WelcomeMessage=Welcome!

[Commands]
AllowTP=true
```

**Reload config without restarting the plugin:**
```python
def On_Command(player, cmd, args):
    if cmd == "reloadconfig" and player.Admin:
        global config_ini
        config_ini = Plugin.GetIni("config")
        player.Message("Configuration reloaded!")
```

---

## 3. JSON Files — Structured Data

Ideal for complex data (nested lists and dictionaries) specific to the plugin.

```python
import json

def save_data(data):
    Plugin.ToJsonFile("data", json.dumps(data, indent=2))

def load_data():
    if Plugin.JsonFileExists("data"):
        return json.loads(Plugin.FromJsonFile("data"))
    return {}

def On_PluginInit():
    global data
    data = load_data()

def On_ServerShutdown():
    save_data(data)
```

---

## 4. MySQL — External Relational Database

**Global in scripts:** `MySQL`

```python
# Connect (typically in On_PluginInit)
MySQL.OpenConnection("localhost", "user", "password", "my_database")

# Read query
result = MySQL.ExecuteReader("SELECT * FROM players WHERE uid = ?", str(player.UID))
while result.Read():
    print(result.GetString("name"))

# Write query
MySQL.ExecuteNonQuery(
    "INSERT INTO kills (uid, victim, timestamp) VALUES (?, ?, ?)",
    str(killer.UID), victim.Name, str(time.time())
)

# Close connection
MySQL.CloseConnection()
```

**Configuration** in `Fougerite.cfg`:
```ini
[MySQL]
Host=localhost
Port=3306
Database=fougerite
Username=root
Password=password
```

---

## 5. SQLite — Local Relational Database

**Global in scripts:** `SQLite`

```python
# Open (creates file if missing)
SQLite.OpenConnection(Plugin.RootDir.FullName + "/my_plugin.db")

# Create table
SQLite.ExecuteNonQuery("""
    CREATE TABLE IF NOT EXISTS bans (
        uid TEXT PRIMARY KEY,
        reason TEXT,
        timestamp INTEGER
    )
""")

# Insert
SQLite.ExecuteNonQuery(
    "INSERT OR REPLACE INTO bans VALUES (?, ?, ?)",
    str(player.UID), "Cheating", int(time.time())
)

# Query
result = SQLite.ExecuteReader("SELECT * FROM bans WHERE uid = ?", str(player.UID))
if result.Read():
    print("Banned for:", result.GetString("reason"))

SQLite.CloseConnection()
```

---

## 6. World Save — ServerSaveHandler

Manages persistence of physical world entities: buildings, item drops, resource nodes.

```python
# Blocking save (waits until complete)
World.ServerSaveHandler.ManualSave()

# Background async save (non-blocking)
World.ServerSaveHandler.ManualBackGroundSave()

# Via Server shorthand
Server.Save()
```

**⚠️ CrucialSavePoint:** A protection window (configurable in minutes) blocks manual saves when an automatic save is imminent, preventing data corruption from concurrent saves.

---

## Best Practices

### Use DataStore for player data
```python
# ✅ Correct — thread-safe, auto-persisted
DataStore.Add("my_plugin", str(player.UID), player.Location)

# ❌ Avoid — plain Python dict is not thread-safe with timers
local_data[player.UID] = player.Location
```

### Save critical data in On_ServerShutdown
```python
def On_ServerShutdown():
    save_data(my_data)
    Plugin.Log("system", "Data saved successfully.")
```

### Use INI for operator-editable settings
`.ini` files let the server operator change plugin behavior without editing code or reloading.

### Never access MySQL/SQLite from background threads without synchronization
The basic connectors are not inherently thread-safe. Use `Loom.QueueOnMainThread` or implement your own locking if accessing from timers or async callbacks.
