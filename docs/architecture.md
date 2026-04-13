# Architecture Overview

## What is Fougerite?

Fougerite is a **server-side modding framework** for *Rust Legacy (v25)*, a Unity 3-era survival game. It is the direct successor to **Magma** (2013), which itself descended from **Rust++** (EquiFox / xEnt). Historical lineage:

```
Rust++ (EquiFox/xEnt)
  └── Magma (2013)
        └── Zumwalt (Riketta, 2014)
              └── Fougerite (Team Pluton / DreTaX, 2014–2026)
```

---

## Integration Mechanism

Fougerite does **not** perform runtime injection. Instead, it uses **static IL assembly patching** before the server starts. The `Fougerite.Patcher` project modifies three Rust Legacy server DLLs:

| Target Assembly | Role in Game |
|---|---|
| `Assembly-CSharp.dll` | Core Rust server logic |
| `uLink.dll` | Networking layer (uLink/Facepunch) |
| `Facepunch.MeshBatch.dll` | Mesh batch rendering system |

After patching, critical points in the game's execution flow are redirected to Fougerite's `Hooks` class, which acts as the **central event bus**. Every relevant game action (player connect, entity hurt, item pickup, etc.) passes through Fougerite before being processed by the native engine.

---

## Key Files

| File | Size | Responsibility |
|---|---|---|
| `Bootstrap.cs` | ~14 KB | Entry point; orchestrates engine loading, config, plugin discovery, and server lifecycle |
| `Hooks.cs` | ~170 KB (4,306 lines) | The largest file in the project; contains all event handler implementations |
| `Hooks_Base.cs` | ~29 KB | All `delegate`/`event` declarations; `ResetHooks()` method |
| `Hooks_RustFixes.cs` | ~71 KB | Bug fixes applied on top of the original Rust Legacy source code |
| `ServerSaveHandler.cs` | ~28 KB | World persistence engine; periodic, manual, and background saves |
| `Stopper.cs` | ~862 bytes | `IDisposable` profiling guard wrapping every hook call |
| `Loom.cs` | ~6 KB | Unity main-thread dispatcher for background thread callbacks |

---

## Hook Call Flow

```
Rust Engine (patched Assembly-CSharp)
  │
  ▼
Hooks.cs — static method (e.g. Hooks.PlayerConnected(netUser))
  │
  ├── Creates/updates Player in cache
  ├── Calls ExecuteSubscribers(Hooks.OnPlayerConnected, player)
  │     │
  │     ├── Plugin A: On_PlayerConnected(player)  ← isolated by try/catch
  │     ├── Plugin B: On_PlayerConnected(player)  ← isolated by try/catch
  │     └── Plugin N: On_PlayerConnected(player)  ← isolated by try/catch
  │
  └── Continues normal Rust execution
```

`ExecuteSubscribers` iterates each subscriber **individually**, isolating exceptions per plugin. A broken plugin does not interrupt the remaining chain.

---

## Plugin System

### Discovery and Loading

1. `Bootstrap` initializes each `IPluginLoader` (C#, Python, JS, Lua)
2. Each loader scans its dedicated directory for plugins
3. Plugins are instantiated and registered in the central `PluginLoader`
4. `PluginLoader.InstallHooks(plugin)` wires the plugin's `On_*` methods to the corresponding `Hooks` delegates
5. `Hooks.AllPluginsLoaded()` is fired once all loaders finish

### Hot-Reload

```
fougerite.reload MyPlugin
  │
  ├── Hooks.ResetHooks()   ← nulls all delegates to prevent ghost callbacks
  ├── plugin.KillTimers()  ← destroys plugin timers
  ├── plugin.Unload()
  └── loader.LoadPlugin()  ← reloads from disk
```

### Intensive Events (Throttle)

The following hooks are **only allowed for C# plugins by default**, unless `EnableScriptPluginsIntensiveEvents=true` is set in `Fougerite.cfg`:

- `On_PlayerMove`
- `On_Shoot` / `On_ShotgunShoot` / `On_BowShoot`
- `On_AnimalMovement`
- `On_HeatZoneEnter` / `On_WorkZoneEnter`

This prevents interpreted scripts (Python/JS/Lua) from overloading the main thread with high-frequency events.

---

## Comparison with Magma

| Aspect | Magma (2013) | Fougerite (2014–2026) |
|---|---|---|
| Rust bug fixes | None | 71 KB of corrections (`Hooks_RustFixes.cs`) |
| Number of hooks | ~15 | 74+ |
| Scripting languages | Rudimentary JS | C#, Python, JS, Lua |
| Native persistence | None | Thread-safe JSON DataStore |
| Permission system | None | Full `PermissionSystem` |
| Backward compatibility | — | Full Magma plugin compatibility |

---

## Solution Structure

```
Fougerite.sln
├── Fougerite/               ← Main modding runtime (this documentation)
├── Fougerite.Patcher/       ← IL patcher for the 3 Rust DLLs
├── GlitchFix/               ← Targeted game glitch correction module
├── PermissionManager/       ← Standalone C# permission management module
└── RustPP/                  ← Ported Rust++ legacy plugin (optional)
```
