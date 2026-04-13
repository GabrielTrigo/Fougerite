# Guide — JavaScript Plugins (Jint)

JavaScript plugins use the **Jint** runtime (ECMAScript 5.1), a pure C# JS interpreter with full CLR interop.

---

## Directory Structure

```
/Plugins/
└── MyPlugin/
    └── MyPlugin.js    ← flat file, global-scope functions
```

---

## Minimal Structure

```javascript
var Author  = "YourName";
var Version = "1.0";
var About   = "Plugin description";

function On_PluginInit() {
    Plugin.Log("system", "MyPlugin started!");
}

function On_PluginShutdown() {
    Plugin.KillTimers();
    Plugin.Log("system", "MyPlugin stopped!");
}
```

> **Note:** There is no class. All functions are declared in the global scope.

---

## Hooks

```javascript
function On_PlayerConnected(player) {
    player.Message("Welcome, " + player.Name + "!");
    Server.Broadcast(player.Name + " joined!");
}

function On_PlayerDisconnected(player) {
    Server.Broadcast(player.Name + " left.");
}

function On_Command(player, cmd, args) {
    if (cmd === "kit") {
        var inv = player.Inventory;
        inv.AddItem("Wood", 500);
        inv.AddItem("Stones", 300);
        player.Message("Kit received!");
    }

    if (cmd === "pos") {
        player.Message("You are at: " + player.X.toFixed(1) +
                       ", " + player.Y.toFixed(1) +
                       ", " + player.Z.toFixed(1));
    }
}

function On_Chat(player, chat) {
    Plugin.Log("chat", player.Name + ": " + chat.Message);
}

function On_PlayerHurt(evt) {
    // Halve all incoming damage
    evt.DamageAmount = evt.DamageAmount * 0.5;
}

function On_EntityDecay(evt) {
    // Disable decay on storage boxes
    if (evt.Entity.Name.indexOf("Storage") !== -1) {
        evt.DamageAmount = 0;
    }
}
```

---

## Importing CLR Types

Jint exposes all loaded assemblies via `importClass()`:

```javascript
// Unity types
var Vector3    = importClass("UnityEngine.Vector3");
var GameObject = importClass("UnityEngine.GameObject");

// .NET types
var List = importClass("System.Collections.Generic.List`1");

// Fougerite types
var DataStore = importClass("Fougerite.DataStore");

// Use the imported type
var pos = new Vector3(100, 50, 200);
```

---

## Timers

```javascript
function On_PluginInit() {
    // Repeating timer every 60 seconds
    Plugin.CreateTimer("Announcement", 60000, true);
}

function AnnouncementCallback(evt) {
    Server.Broadcast("Server online! Use /help to see commands.");
}

// Parallel timer with custom args
function scheduleTeleport(player, x, y, z) {
    var args = Plugin.CreateDict();
    args["player"] = player;
    args["x"] = x;
    args["y"] = y;
    args["z"] = z;
    Plugin.CreateParallelTimer("TP_" + player.UID, 3000, args);
}

function TP_Callback(evt) {
    var player = evt.Args["player"];
    player.TeleportTo(evt.Args["x"], evt.Args["y"], evt.Args["z"]);
    player.Message("Teleported!");
}
```

---

## Configuration and Data

```javascript
var config;

function On_PluginInit() {
    config = Plugin.CreateIni("config");

    if (config.GetSetting("General", "Message") === null) {
        config.AddSetting("General", "Message", "Welcome!");
        config.Save();
    }
}

function On_PlayerConnected(player) {
    var msg = config.GetSetting("General", "Message");
    player.Message(msg);

    var uid = player.UID.ToString();
    var visits = DataStore.Get("my_plugin", uid);

    if (visits === null) {
        DataStore.Add("my_plugin", uid, 1);
        player.Notice("First visit!");
    } else {
        DataStore.Add("my_plugin", uid, parseInt(visits) + 1);
    }
}
```

---

## .NET API Compatibility

Jint automatically injects polyfills for .NET compatibility:

```javascript
// .Length works on both arrays and strings (normally only .length in pure JS)
var len = player.Name.Length;   // OK — polyfill injected
var len = player.Name.length;   // also OK

// .ToString() works on CLR objects
var uid = player.UID.ToString();

// CLR string comparison
if (player.Name.Equals("Admin")) { /* ... */ }
```

---

## Jint Limitations

| Limitation | Description |
|---|---|
| **ECMAScript 5.1** | No `let`, `const`, arrow functions, `Promise`, `async/await` |
| **No `require()`** | No module system — all code in one file |
| **No `setTimeout`** | Use `Plugin.CreateTimer()` instead |
| **Limited stack traces** | JS errors have limited line info; use `try/catch` aggressively |
| **Recursion limit** | Hard cap at 1,000,000 (sufficient in practice) |

---

## Best Practices

```javascript
// Wrap critical logic in try/catch
function On_Command(player, cmd, args) {
    try {
        processCommand(player, cmd, args);
    } catch (e) {
        Plugin.Log("errors", "Error in On_Command: " + e.message);
        player.Message("An error occurred. Contact an admin.");
    }
}

// Always null-check player objects from events
function On_PlayerKilled(evt) {
    if (evt.Killer !== null && evt.Victim !== null) {
        Server.Broadcast(evt.Killer.Name + " killed " + evt.Victim.Name);
    }
}
```
