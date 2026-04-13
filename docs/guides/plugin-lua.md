# Guide — Lua Plugins (MoonSharp)

Lua plugins use the **MoonSharp** runtime, a complete Lua 5.2 implementation in pure C# with automatic CLR type interop.

---

## Directory Structure

```
/Plugins/
└── MyPlugin/
    └── MyPlugin.lua    ← flat file, global-scope functions
```

---

## Minimal Structure

```lua
Author  = "YourName"
Version = "1.0"
About   = "Plugin description"

function On_PluginInit()
    Plugin:Log("system", "MyPlugin started!")
end

function On_PluginShutdown()
    Plugin:KillTimers()
    Plugin:Log("system", "MyPlugin stopped!")
end
```

> **Note:** Like JavaScript, functions are declared in the global scope. Metadata (`Author`, `Version`, `About`) are Lua global variables.  
> **Lua method syntax:** Use `:` to call methods on CLR objects (it passes `self` implicitly).

---

## Hooks

```lua
function On_PlayerConnected(player)
    player:Message("Welcome, " .. player.Name .. "!")
    Server:Broadcast(player.Name .. " joined!")
end

function On_PlayerDisconnected(player)
    Server:Broadcast(player.Name .. " left.")
end

function On_Command(player, cmd, args)
    if cmd == "kit" then
        local inv = player.Inventory
        inv:AddItem("Wood", 500)
        inv:AddItem("Stones", 300)
        player:Message("Kit received!")

    elseif cmd == "pos" then
        player:Message(string.format("Position: %.1f, %.1f, %.1f",
                                      player.X, player.Y, player.Z))
    end
end

function On_Chat(player, chat)
    Plugin:Log("chat", player.Name .. ": " .. chat.Message)
end

function On_PlayerHurt(evt)
    -- Cancel fall damage
    if evt.WeaponName == "fall" then
        evt.Cancel = true
    end
end

function On_EntityDecay(evt)
    -- Disable decay on storage boxes
    if string.find(evt.Entity.Name, "Storage") then
        evt.DamageAmount = 0
    end
end
```

---

## CLR Interop

MoonSharp with `InteropRegistrationPolicy.Automatic` exposes all CLR types automatically:

```lua
-- Access Unity type properties directly
local pos = player.Location    -- UnityEngine.Vector3
local x   = pos.x
local y   = pos.y

-- Convert .NET values to Lua types
local uid = tostring(player.UID)    -- ulong → Lua string

-- Boolean check
if player.Admin then
    player:Message("You are an administrator.")
end
```

---

## Timers

```lua
function On_PluginInit()
    -- Repeating timer every 30 seconds
    Plugin:CreateTimer("Announcement", 30000, true)
end

function AnnouncementCallback(evt)
    Server:Broadcast("Visit our Discord! discord.gg/example")
end

-- Parallel timer with custom args
function scheduleTeleport(player, x, y, z)
    local args = Plugin:CreateDict()
    args["player"] = player
    args["x"] = x
    args["y"] = y
    args["z"] = z
    Plugin:CreateParallelTimer("TP", 3000, args)
end

function TPCallback(evt)
    local player = evt.Args["player"]
    player:TeleportTo(evt.Args["x"], evt.Args["y"], evt.Args["z"])
    player:Message("Teleported!")
end
```

---

## Configuration and Data

```lua
local config

function On_PluginInit()
    config = Plugin:CreateIni("config")

    if config:GetSetting("General", "Message") == nil then
        config:AddSetting("General", "Message", "Welcome!")
        config:Save()
    end
end

function On_PlayerConnected(player)
    local msg = config:GetSetting("General", "Message")
    player:Message(msg)

    local uid    = tostring(player.UID)
    local visits = DataStore:Get("my_plugin", uid)

    if visits == nil then
        DataStore:Add("my_plugin", uid, 1)
        player:Notice("First visit!")
    else
        DataStore:Add("my_plugin", uid, tonumber(visits) + 1)
    end
end
```

---

## Local Tables (Session Data)

```lua
local session = {}   -- local table (not persisted, cleared on reload)

function On_PlayerConnected(player)
    session[tostring(player.UID)] = {
        name        = player.Name,
        connectedAt = os.time()
    }
end

function On_PlayerDisconnected(player)
    local uid = tostring(player.UID)
    if session[uid] then
        local elapsed = os.time() - session[uid].connectedAt
        Server:Broadcast(player.Name .. " was online for " .. elapsed .. " second(s).")
        session[uid] = nil
    end
end
```

---

## MoonSharp Limitations

| Limitation | Description |
|---|---|
| **Lua 5.2** | Missing some Lua 5.3+ features (integer division `//`, native bitwise ops, etc.) |
| **No external `require()`** | All code must be in the main file |
| **Asymmetric CLR interop** | CLR methods use `:`, properties use `.` |
| **null vs nil** | CLR `null` values are returned as Lua `nil` |

---

## The `.` vs `:` Distinction

This is the most common source of errors in Lua plugins:

```lua
-- Properties (use dot):
local name   = player.Name
local health = player.Health

-- Methods (use colon — passes self implicitly):
player:Message("Hello!")
player:TeleportTo(0, 50, 0)
Server:Broadcast("Notice!")
Plugin:Log("file", "message")

-- WRONG — will raise an error:
player.Message("Hello!")   -- ← missing implicit self
```
