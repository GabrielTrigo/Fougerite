# Getting Started — Plugin Development

This guide walks you through creating your first plugin in all four supported languages. Every example implements the same plugin: **WelcomePlugin** — sends a welcome message on connect and responds to the `/info` command.

---

## Prerequisites

1. A Rust Legacy server with Fougerite installed and running
2. Access to the server plugin folders:
   - **Scripts** (Python / JS / Lua): `/Plugins/<PluginName>/`
   - **C# Modules**: `/Modules/<PluginName>/`
3. RCON or console access to reload plugins

---

## Directory Structure

```
/Plugins/
└── WelcomePlugin/
    ├── WelcomePlugin.py    ← (or .js / .lua)
    └── config.ini          ← (optional, auto-created by plugin)

/Modules/
└── WelcomePlugin/
    └── WelcomePlugin.dll   ← for C# modules only
```

---

## Globally Injected Variables

Every plugin in every language automatically receives these variables:

| Variable | What it is |
|---|---|
| `Plugin` | The plugin itself (`BasePlugin`) — timers, I/O, logging |
| `Server` | `Fougerite.Server` — players, broadcast, bans |
| `World` | `Fougerite.World` — airdrop, spawn, save |
| `DataStore` | Thread-safe persistent key-value storage |
| `Util` | Utilities (hashing, strings, reflection) |
| `Web` | HTTP GET/POST requests |
| `MySQL` / `SQLite` | Database connectors |
| `PermissionSystem` | Permission system |
| `PlayerCache` | Historical player cache |
| `Loom` | Unity main-thread dispatcher |

---

## Python Implementation

**File:** `/Plugins/WelcomePlugin/WelcomePlugin.py`

```python
__author__  = "YourName"
__version__ = "1.0"
__about__   = "Welcome plugin"

class WelcomePlugin:

    def On_PluginInit(self):
        self.config = Plugin.CreateIni("config")

        if self.config.GetSetting("General", "Message") is None:
            self.config.AddSetting("General", "Message", "Welcome to the server!")
            self.config.Save()

        Plugin.Log("system", "WelcomePlugin started!")

    def On_PlayerConnected(self, player):
        msg = self.config.GetSetting("General", "Message")
        player.Message(msg)
        Server.Broadcast(player.Name + " joined the server!")

        uid = str(player.UID)
        if not DataStore.ContainsKey("welcome_visits", uid):
            DataStore.Add("welcome_visits", uid, 1)
            player.Notice("First time here? Welcome!")
        else:
            visits = int(DataStore.Get("welcome_visits", uid)) + 1
            DataStore.Add("welcome_visits", uid, visits)

    def On_PlayerDisconnected(self, player):
        Server.Broadcast(player.Name + " left the server.")

    def On_Command(self, player, cmd, args):
        if cmd == "info":
            visits = DataStore.Get("welcome_visits", str(player.UID))
            player.Message("=== Player Info ===")
            player.Message("Name: "    + player.Name)
            player.Message("ID: "      + player.SteamID)
            player.Message("Visits: "  + str(visits))
            player.Message("Position: " + str(player.X) + ", " +
                                          str(player.Y) + ", " +
                                          str(player.Z))
```

---

## JavaScript Implementation

**File:** `/Plugins/WelcomePlugin/WelcomePlugin.js`

```javascript
var Author  = "YourName";
var Version = "1.0";
var About   = "Welcome plugin";

var config;

function On_PluginInit() {
    config = Plugin.CreateIni("config");

    if (config.GetSetting("General", "Message") === null) {
        config.AddSetting("General", "Message", "Welcome to the server!");
        config.Save();
    }

    Plugin.Log("system", "WelcomePlugin started!");
}

function On_PlayerConnected(player) {
    var msg = config.GetSetting("General", "Message");
    player.Message(msg);
    Server.Broadcast(player.Name + " joined the server!");

    var uid = player.UID.ToString();
    if (!DataStore.ContainsKey("welcome_visits", uid)) {
        DataStore.Add("welcome_visits", uid, 1);
        player.Notice("First time here? Welcome!");
    } else {
        var visits = parseInt(DataStore.Get("welcome_visits", uid)) + 1;
        DataStore.Add("welcome_visits", uid, visits);
    }
}

function On_PlayerDisconnected(player) {
    Server.Broadcast(player.Name + " left the server.");
}

function On_Command(player, cmd, args) {
    if (cmd === "info") {
        var visits = DataStore.Get("welcome_visits", player.UID.ToString());
        player.Message("=== Player Info ===");
        player.Message("Name: "     + player.Name);
        player.Message("ID: "       + player.SteamID);
        player.Message("Visits: "   + visits);
        player.Message("Position: " + player.X + ", " + player.Y + ", " + player.Z);
    }
}
```

---

## Lua Implementation

**File:** `/Plugins/WelcomePlugin/WelcomePlugin.lua`

```lua
Author  = "YourName"
Version = "1.0"
About   = "Welcome plugin"

local config

function On_PluginInit()
    config = Plugin:CreateIni("config")

    if config:GetSetting("General", "Message") == nil then
        config:AddSetting("General", "Message", "Welcome to the server!")
        config:Save()
    end

    Plugin:Log("system", "WelcomePlugin started!")
end

function On_PlayerConnected(player)
    local msg = config:GetSetting("General", "Message")
    player:Message(msg)
    Server:Broadcast(player.Name .. " joined the server!")

    local uid = tostring(player.UID)
    if not DataStore:ContainsKey("welcome_visits", uid) then
        DataStore:Add("welcome_visits", uid, 1)
        player:Notice("First time here? Welcome!")
    else
        local visits = tonumber(DataStore:Get("welcome_visits", uid)) + 1
        DataStore:Add("welcome_visits", uid, visits)
    end
end

function On_PlayerDisconnected(player)
    Server:Broadcast(player.Name .. " left the server.")
end

function On_Command(player, cmd, args)
    if cmd == "info" then
        local visits = DataStore:Get("welcome_visits", tostring(player.UID))
        player:Message("=== Player Info ===")
        player:Message("Name: "     .. player.Name)
        player:Message("ID: "       .. player.SteamID)
        player:Message("Visits: "   .. tostring(visits))
        player:Message("Position: " .. tostring(player.X) .. ", " ..
                                        tostring(player.Y) .. ", " ..
                                        tostring(player.Z))
    end
end
```

---

## C# Module Implementation

**File:** `/Modules/WelcomePlugin/WelcomePlugin.cs` → Compile to `WelcomePlugin.dll`

```csharp
using System;
using Fougerite;
using Fougerite.Events;
using Fougerite.PluginLoaders;

public class WelcomePlugin : Module
{
    public override string  Name        => "WelcomePlugin";
    public override Version Version     => new Version(1, 0);
    public override string  Author      => "YourName";
    public override string  Description => "Welcome plugin";

    private IniParser config;

    public override void Initialize()
    {
        config = GetIni("config") ?? CreateIni("config");

        if (config.GetSetting("General", "Message") == null)
        {
            config.AddSetting("General", "Message", "Welcome to the server!");
            config.Save();
        }

        Hooks.OnPlayerConnected    += OnPlayerConnected;
        Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
        Hooks.OnCommand            += OnCommand;

        Logger.Log("[WelcomePlugin] Started!");
    }

    public override void DeInitialize()
    {
        Hooks.OnPlayerConnected    -= OnPlayerConnected;
        Hooks.OnPlayerDisconnected -= OnPlayerDisconnected;
        Hooks.OnCommand            -= OnCommand;
        KillTimers();
    }

    private void OnPlayerConnected(Player player)
    {
        string msg = config.GetSetting("General", "Message");
        player.Message(msg);
        Server.GetServer().Broadcast(player.Name + " joined the server!");

        string uid = player.UID.ToString();
        var ds = DataStore.GetInstance();

        if (!ds.ContainsKey("welcome_visits", uid))
        {
            ds.Add("welcome_visits", uid, 1);
            player.Notice("First time here? Welcome!");
        }
        else
        {
            int visits = Convert.ToInt32(ds.Get("welcome_visits", uid)) + 1;
            ds.Add("welcome_visits", uid, visits);
        }
    }

    private void OnPlayerDisconnected(Player player)
        => Server.GetServer().Broadcast(player.Name + " left the server.");

    private void OnCommand(Player player, string cmd, string[] args)
    {
        if (cmd != "info") return;

        var ds = DataStore.GetInstance();
        var visits = ds.Get("welcome_visits", player.UID.ToString());

        player.Message("=== Player Info ===");
        player.Message($"Name: {player.Name}");
        player.Message($"ID: {player.SteamID}");
        player.Message($"Visits: {visits}");
        player.Message($"Position: {player.X:F1}, {player.Y:F1}, {player.Z:F1}");
    }
}
```

---

## Loading and Reloading

```bash
# Via RCON/console — reload a specific plugin
fougerite.reload WelcomePlugin

# Reload all plugins
fougerite.reload

# Unload a plugin
fougerite.unload WelcomePlugin
```

---

## Next Steps

- [Hooks Reference](../hooks-reference.md) — all available events
- [API Reference — Player](../api-reference/player.md) — what you can do with a player object
- [DataStore](../api-reference/datastore.md) — persistent data storage
- [BasePlugin](../api-reference/base-plugin.md) — timers, logging, and more
