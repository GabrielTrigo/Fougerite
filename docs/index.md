# Fougerite — Technical Documentation

Welcome to the technical documentation for **Fougerite**, the most stable and feature-rich modding platform for Rust Legacy. This documentation covers the plugin API, the hooks system, and practical guides for every supported language.

> **Project status:** Complete (2026) · **Lead author:** DreTaX / Team Pluton  
> **Repository:** [github.com/Notulp/Fougerite](https://github.com/Notulp/Fougerite)

---

## Table of Contents

### 🏛️ Architecture

- [Architecture Overview](architecture.md) — How Fougerite integrates with Rust Legacy, the IL patching mechanism, and internal call flow

### 🎣 Hooks (Events)

- [Hooks Reference](hooks-reference.md) — Complete list of all 74+ hooks with signatures, descriptions, and code examples

### 📚 API Reference

| Document | Description |
|---|---|
| [Server](api-reference/server.md) | Player lists, bans, broadcast, console commands |
| [World](api-reference/world.md) | Airdrops, entity spawning, zones, world save |
| [Player](api-reference/player.md) | State, inventory, teleport, messaging, permissions |
| [Entity](api-reference/entity.md) | Deployed objects, structures, container inventories |
| [BasePlugin](api-reference/base-plugin.md) | Timers, file I/O, logging, cross-plugin calls |
| [DataStore](api-reference/datastore.md) | Thread-safe key-value persistent storage |
| [Events](api-reference/events.md) | All event payload classes |
| [PermissionSystem](api-reference/permissions.md) | Groups, player permissions, wildcards, runtime revocation |
| [Zone3D](api-reference/zone3d.md) | Polygon-based 3D zones for PVP/PVE areas, safe zones, arenas |
| [Web](api-reference/web.md) | Async HTTP/HTTPS requests with callbacks |

### 💾 Data & Persistence

- [Data & Persistence](data-persistence.md) — DataStore, INI, JSON, logging, MySQL, SQLite, and world save

### 🛠️ Plugin Developer Guides

| Guide | Language |
|---|---|
| [Getting Started](guides/getting-started.md) | All languages |
| [C# Modules](guides/plugin-csharp.md) | C# / .NET CLR |
| [Python Plugins](guides/plugin-python.md) | IronPython |
| [JavaScript Plugins](guides/plugin-javascript.md) | Jint (ECMAScript 5.1) |
| [Lua Plugins](guides/plugin-lua.md) | MoonSharp (Lua 5.2) |

---

## Quick RCON Commands

| Command | Description |
|---|---|
| `fougerite.reload` | Reload all plugins |
| `fougerite.reload <Name>` | Reload a specific plugin |
| `fougerite.unload <Name>` | Unload a specific plugin |
| `fougerite.save` | Save the world (respects CrucialSavePoint window) |
| `fougerite.urgentsave` | Save the world immediately |

---

*Fougerite Project — © EquiFox, xEnt, Riketta, Team Pluton, DreTaX — 2013–2026*
