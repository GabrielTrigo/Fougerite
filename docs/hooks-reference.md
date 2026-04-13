# Hooks Reference

This page lists every hook (event) available in Fougerite. Implement a function with the exact hook name in your plugin to subscribe to that event.

> **Total hooks:** 74 distinct events  
> **⚠️ Hooks marked [INTENSIVE]** only work in C# plugins by default. Enable `EnableScriptPluginsIntensiveEvents=true` in `Fougerite.cfg` to allow them in scripted plugins.

---

## Server Lifecycle

| Hook | Parameters | Description |
|---|---|---|
| `On_ServerInit` | *(none)* | Server starting to load, before world and plugins |
| `On_ServerLoaded` | *(none)* | Server fully initialized |
| `On_AllPluginsLoaded` | *(none)* | Fired once after all plugin loaders complete |
| `On_ServerShutdown` | *(none)* | Server is shutting down |
| `On_ServerSaved` | `int itemCount, double elapsedSecs` | World save completed |
| `On_PluginInit` | *(none)* | Called immediately after the plugin is loaded |
| `On_PluginShutdown` | *(none)* | Called before the plugin is unloaded |
| `On_ItemsLoaded` | *(none)* | `ItemDataBlock` dictionary initialized |
| `On_TablesLoaded` | *(none)* | `LootSpawnList` tables initialized |
| `On_GenericSpawnLoad` | *(none)* | `ResourceSpawner` loaded |

---

## Player — Connection

| Hook | Parameters | Description |
|---|---|---|
| `On_PlayerConnected` | `Player player` | Player connected to the server |
| `On_PlayerDisconnected` | `Player player` | Player disconnected |
| `On_PlayerApproval` | `PlayerApprovalEvent evt` | Pre-auth approval check; call `evt.Deny()` to block the join |
| `On_SteamDeny` | `SteamDenyEvent evt` | Player rejected by Steam authentication |
| `On_PlayerBan` | `BanEvent evt` | Player is being banned |
| `On_VoiceChat` | `uLink.NetworkPlayer np, Player player` | Player activated the microphone |

---

## Player — Spawn and Movement

| Hook | Parameters | Description |
|---|---|---|
| `On_PlayerSpawning` | `Player player, SpawnEvent evt` | Player is about to spawn (pre-spawn) |
| `On_PlayerSpawned` | `Player player, SpawnEvent evt` | Player just spawned in the world |
| `On_PlayerTeleport` | `Player player, Vector3 from, Vector3 dest` | Player was teleported via the API |
| `On_PlayerMove` | `HumanController hc, Vector3 pos, int p, ushort p2, NetworkMessageInfo info, PlayerActions action` | Position update — high frequency **[INTENSIVE]** |

---

## Player — Combat and Damage

| Hook | Parameters | Description |
|---|---|---|
| `On_PlayerHurt` | `HurtEvent evt` | Player takes damage |
| `On_PlayerKilled` | `DeathEvent evt` | Player dies |
| `On_FallDamage` | `FallDamageEvent evt` | Player receives fall damage |

---

## Chat and Console

| Hook | Parameters | Description |
|---|---|---|
| `On_Chat` | `Player player, ref ChatString chat` | Chat message (text is mutable) |
| `On_Command` | `Player player, string cmd, string[] args` | Slash command (e.g. `/help`) |
| `On_ConsoleWithCancel` | `ConsoleEvent evt` | Console command **(preferred)** — supports `evt.Cancelled = true` |
| `On_Console` | `ConsoleSystem.Arg arg` | *(Deprecated)* Console command without cancel support |
| `On_CommandRestriction` | `CommandRestrictionEvent evt` | Command restriction table changed |

---

## World Entities

| Hook | Parameters | Description |
|---|---|---|
| `On_EntityDeployed` | `Player player, Entity entity, Player actualPlacer` | Entity placed in the world |
| `On_EntityHurt` | `HurtEvent evt` | Entity takes damage |
| `On_EntityDestroyed` | `DestroyEvent evt` | Entity destroyed |
| `On_EntityDecay` | `DecayEvent evt` | Entity natural decay tick; `evt.DamageAmount` is mutable |
| `On_DoorUse` | `Player player, DoorEvent evt` | Door opened or closed |
| `On_LootUse` | `LootStartEvent evt` | Player starts looting a container |
| `On_RepairBench` | `RepairEvent evt` | Repair bench used |

---

## Combat and Weapons

| Hook | Parameters | Description |
|---|---|---|
| `On_Shoot` | `ShootEvent evt` | Firearm discharged **[INTENSIVE]** |
| `On_ShotgunShoot` | `ShotgunShootEvent evt` | Shotgun discharged **[INTENSIVE]** |
| `On_BowShoot` | `BowShootEvent evt` | Bow fired **[INTENSIVE]** |
| `On_GrenadeThrow` | `GrenadeThrowEvent evt` | Grenade thrown |
| `On_TimedExplosiveSpawned` | `TimedExplosiveEvent evt` | C4 placed |
| `On_ItemModInstall` | `ItemModInstalledEvent evt` | Weapon attachment installed |
| `On_BloodDraw` | `BloodDrawEvent evt` | Blood Draw Kit used |

---

## Inventory and Items

| Hook | Parameters | Description |
|---|---|---|
| `On_ItemAdded` | `InventoryModEvent evt` | Item added to any inventory |
| `On_ItemRemoved` | `InventoryModEvent evt` | Item removed from any inventory |
| `On_ItemPickup` | `ItemPickupEvent evt` | Item picked up from ground |
| `On_ItemMove` | `ItemMoveEvent evt` | Item moved between slots or inventories |
| `On_BeltUse` | `BeltUseEvent evt` | Belt slot selection changed |
| `On_BlueprintUse` | `Player player, BPUseEvent evt` | Blueprint consumed to learn a recipe |
| `On_Research` | `ResearchEvent evt` | Research bench used |
| `On_Crafting` | `CraftingEvent evt` | Crafting job started |
| `On_ConsumableUse` | `ConsumableUseEvent evt` | Food or drink consumed |
| `On_MedikitUse` | `MedikitUseEvent evt` | Medkit or bandage used |
| `On_ArmorEquip` | `ArmorEquipEvent evt` | Armor piece equipped |
| `On_ArmorUnEquip` | `ArmorEquipEvent evt` | Armor piece unequipped |
| `On_PlayerGathering` | `Player player, GatherEvent evt` | Player gathers from resource node or animal corpse |

---

## NPCs and World

| Hook | Parameters | Description |
|---|---|---|
| `On_NPCHurt` | `HurtEvent evt` | NPC or animal takes damage |
| `On_NPCKilled` | `DeathEvent evt` | NPC or animal killed |
| `On_NPCSpawned` | `NPC npc` | NPC instance spawned |
| `On_AnimalMovement` | `AnimalMovementEvent evt` | Animal position update **[INTENSIVE]** |
| `On_ResourceSpawn` | `ResourceTarget target` | Resource node spawned |
| `On_SleeperSpawned` | `Sleeper sleeper` | Sleeper object created |
| `On_DayCycleChanged` | `DayCycleChangeEvent evt` | Day/night transition |
| `On_HeatZoneEnter` | `HeatZoneEnterEvent evt` | Player enters a heat zone **[INTENSIVE]** |
| `On_WorkZoneEnter` | `WorkZoneEnterEvent evt` | Player enters a work zone **[INTENSIVE]** |
| `On_FireBarrelToggle` | `FireBarrelToggleEvent evt` | Fire barrel lit or extinguished |

---

## Supply Drop (Airdrop)

| Hook | Parameters | Description |
|---|---|---|
| `On_Airdrop` | `Vector3 targetPos` | Airdrop called |
| `On_SupplyDropPlaneCreated` | `SupplyDropPlane plane` | C-130 supply plane spawned |
| `On_AirdropCrateDropped` | `SupplyDropPlane plane, Entity crate` | Supply crate released from plane |
| `On_SupplySignalExploded` | `SupplySignalExplosionEvent evt` | Supply signal grenade detonated |

---

## Flare and Torch

| Hook | Parameters | Description |
|---|---|---|
| `On_FlareThrow` | `FlareThrowEvent evt` | Flare thrown |
| `On_FlareIgnite` | `FlareIgniteEvent evt` | Flare ignited in hand |
| `On_TorchIgnite` | `BasicTorchIgniteEvent evt` | Basic torch ignited |

---

## Miscellaneous

| Hook | Parameters | Description |
|---|---|---|
| `On_Logger` | `LoggerEvent evt` | Internal logger functions triggered |

---

## Usage Examples

### Python

```python
class MyPlugin:
    def On_PlayerConnected(self, player):
        Server.Broadcast(player.Name + " joined the server!")

    def On_Chat(self, player, chat):
        Plugin.Log("chat", player.Name + ": " + chat.Message)

    def On_PlayerKilled(self, evt):
        if evt.Killer != None:
            Server.Broadcast(evt.Killer.Name + " killed " + evt.Victim.Name)
```

### JavaScript

```javascript
function On_PlayerConnected(player) {
    Server.Broadcast(player.Name + " joined the server!");
}

function On_Chat(player, chat) {
    Plugin.Log("chat", player.Name + ": " + chat.Message);
}

function On_PlayerKilled(evt) {
    if (evt.Killer !== null) {
        Server.Broadcast(evt.Killer.Name + " killed " + evt.Victim.Name);
    }
}
```

### Lua

```lua
function On_PlayerConnected(player)
    Server:Broadcast(player.Name .. " joined the server!")
end

function On_Chat(player, chat)
    Plugin:Log("chat", player.Name .. ": " .. chat.Message)
end
```

### C# (Module)

```csharp
public class MyPlugin : Module
{
    public override string Name => "MyPlugin";
    public override Version Version => new Version(1, 0);
    public override string Author => "YourName";

    public override void Initialize()
    {
        Hooks.OnPlayerConnected += OnPlayerConnected;
        Hooks.OnChat += OnChat;
    }

    public override void DeInitialize()
    {
        Hooks.OnPlayerConnected -= OnPlayerConnected;
        Hooks.OnChat -= OnChat;
    }

    private void OnPlayerConnected(Player player)
        => Server.GetServer().Broadcast(player.Name + " joined the server!");

    private void OnChat(Player player, ref ChatString chat)
        => Logger.Log("[chat] " + player.Name + ": " + chat.Message);
}
```
