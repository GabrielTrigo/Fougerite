# Guide — C# Modules

C# modules are the most powerful way to build Fougerite plugins. Unlike scripted plugins (Python/JS/Lua), C# modules compile to native DLLs and have unrestricted access to the Unity engine and Rust's internal APIs.

---

## Directory Structure

```
/Modules/
└── MyModule/
    ├── MyModule.dll         ← compiled assembly (required)
    ├── MyModule.cs          ← source file (optional, for reference)
    ├── config.ini           ← settings (created by the plugin)
    └── MyModule.log         ← log file (created by the plugin)
```

---

## Base Class: `Module`

Every C# module inherits from `Fougerite.Module`:

```csharp
using System;
using Fougerite;
using Fougerite.Events;

public class MyModule : Module
{
    // Required metadata
    public override string  Name        => "MyModule";
    public override Version Version     => new Version(1, 0, 0);
    public override string  Author      => "YourName";
    public override string  Description => "Module description";

    // Load priority (lower = loads first)
    // Default: uint.MaxValue (loads last)
    public override uint Order => 10;

    // Plugin folder is available via: ModuleFolder
}
```

---

## Lifecycle

```csharp
public override void Initialize()
{
    // Called when the module is loaded
    // Register your hooks here
    Hooks.OnPlayerConnected += MyConnectionHandler;
    Logger.Log("[MyModule] Initialized!");
}

public override void DeInitialize()
{
    // Called when the module is unloaded or reloaded
    // Always unsubscribe hooks to prevent memory leaks
    Hooks.OnPlayerConnected -= MyConnectionHandler;
    KillTimers();
    Logger.Log("[MyModule] Finalized!");
}
```

---

## Subscribing to Hooks

```csharp
public override void Initialize()
{
    Hooks.OnPlayerConnected    += OnPlayerConnected;
    Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
    Hooks.OnChat               += OnChat;
    Hooks.OnCommand            += OnCommand;
    Hooks.OnPlayerHurt         += OnPlayerHurt;
    Hooks.OnEntityDeployedWithPlacer += OnEntityDeployed;
}

private void OnPlayerConnected(Player player)
{
    Logger.Log($"[MyModule] {player.Name} connected (UID: {player.UID})");
}

private void OnChat(Player player, ref ChatString chat)
{
    // Modify the chat message
    chat.Message = $"[{player.Name}] {chat.Message}";
}

private void OnCommand(Player player, string cmd, string[] args)
{
    if (cmd == "mycommand")
        player.Message("Command received with " + args.Length + " argument(s)!");
}

private void OnPlayerHurt(HurtEvent evt)
{
    // Cancel environmental damage (fall, starvation, etc.)
    if (evt.AttackerPlayer == null)
        evt.Cancel = true;
}
```

---

## Timers

```csharp
// Simple timer (one per name, auto-repeating)
var timer = CreateTimer("Announcement", 60000, OnAnnouncementTimer, autoReset: true);

private void OnAnnouncementTimer(TimedEvent evt)
{
    Server.GetServer().Broadcast("Remember to vote for the server!");
}

// Parallel timer (multiple with the same name, with custom args)
var args = new Dictionary<string, object> { { "player", player } };
CreateParallelTimer("PendingTeleport", 3000, args, OnTeleportCallback, autoReset: false);

private void OnTeleportCallback(TimedEvent evt)
{
    var player = (Player) evt.Args["player"];
    player.TeleportTo(0f, 50f, 0f);
    player.Message("Teleported!");
}

// Kill timers
KillTimer("Announcement");
KillParallelTimer("PendingTeleport");
KillTimers();  // kill all
```

---

## Configuration (INI)

```csharp
private IniParser config;

public override void Initialize()
{
    // ModuleFolder is automatically set to the module's directory
    config = new IniParser(System.IO.Path.Combine(ModuleFolder, "config.ini"));

    if (config.GetSetting("General", "WelcomeMessage") == null)
    {
        config.AddSetting("General", "WelcomeMessage", "Welcome!");
        config.Save();
    }
}
```

---

## Persistence with DataStore

```csharp
var ds = DataStore.GetInstance();

// Save player data
ds.Add("myplugin_data", player.UID.ToString(), player.Location);

// Read data
var pos = ds.Get("myplugin_data", player.UID.ToString()) as UnityEngine.Vector3?;

// Check existence
if (ds.ContainsKey("myplugin_data", player.UID.ToString()))
{
    // ...
}

// Flush to disk immediately
ds.Save();
```

---

## Thread Safety with Loom

If you use **Tasks, Threads, or async library callbacks**, never access Unity or Fougerite APIs directly from a background thread. Use `Loom`:

```csharp
// FROM A BACKGROUND THREAD:
System.Threading.Tasks.Task.Run(() =>
{
    string result = MakeHttpRequest();

    // Marshal back to Unity main thread before touching game APIs
    Loom.QueueOnMainThread(() =>
    {
        Server.GetServer().Broadcast("Result: " + result);
    });
});
```

---

## Cross-Plugin Access

```csharp
var other = PluginLoader.GetInstance().Plugins["OtherPlugin"] as BasePlugin;
if (other != null)
{
    other.Invoke("SomeMethod", player, "argument");
}
```

---

## Intensive Events

The following hooks require the plugin to be C# (or `EnableScriptPluginsIntensiveEvents=true`):

```csharp
Hooks.OnPlayerMove     += OnPlayerMove;     // fires on every movement tick
Hooks.OnShoot          += OnShoot;          // fires on every bullet fired
Hooks.OnShotgunShoot   += OnShotgunShoot;
Hooks.OnBowShoot       += OnBowShoot;
Hooks.OnAnimalMovement += OnAnimalMovement;
Hooks.OnHeatZoneEnter  += OnHeatZoneEnter;
Hooks.OnWorkZoneEnter  += OnWorkZoneEnter;
```

> **⚠️ Warning:** These events fire at very high frequency. Keep implementations fast. Avoid I/O, locks, or heavy logic in these handlers.

---

## Compiling the Module

1. Create a Class Library project (.NET 4.x) in Visual Studio or Rider
2. Add references:
   - `Fougerite.dll` (from the `/Fougerite/` folder)
   - `UnityEngine.dll` (from `/rust_server_Data/Managed/`)
   - Patched `Assembly-CSharp.dll`
3. Build in Release mode → copy the `.dll` to `/Modules/MyModule/MyModule.dll`
4. Reload: `fougerite.reload MyModule`
