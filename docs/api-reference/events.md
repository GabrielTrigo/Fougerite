# API Reference — Event Classes

All hooks pass an event object as a parameter. This page describes the most commonly used event classes.

---

## HurtEvent

Used in: `On_PlayerHurt`, `On_EntityHurt`, `On_NPCHurt`

| Property | Type | Description |
|---|---|---|
| `Attacker` | `object` | Attacker — cast using `AttackerIsPlayer`, `AttackerIsNPC`, `AttackerIsEntity` |
| `AttackerIsPlayer` | `bool` | `true` if the attacker is a `Player` |
| `AttackerIsNPC` | `bool` | `true` if the attacker is an NPC/animal |
| `Victim` | `object` | Victim — cast using `VictimIsPlayer`, `VictimIsNPC`, `VictimIsSleeper` |
| `VictimIsPlayer` | `bool` | `true` if the victim is a `Player` |
| `DamageAmount` | `float` | Amount of damage (mutable; setting to `0` cancels the event) |
| `DamageType` | `string` | Damage type string (`"Bullet"`, `"Melee"`, `"Explosion"`, etc.) |
| `Cancelled` | `bool` | Read-only; `true` when `DamageAmount` was set to `0` |

```python
def On_PlayerHurt(evt):
    # Cancel friendly fire between allies
    if evt.AttackerIsPlayer and evt.VictimIsPlayer:
        attacker = evt.Attacker  # cast to Player — safe because AttackerIsPlayer is True
        victim   = evt.Victim
        if are_allies(attacker, victim):
            evt.DamageAmount = 0  # setting to 0 cancels the damage (Cancelled becomes True)
            attacker.Message("Friendly fire is disabled!")
            return

    # Reduce all damage by 50%
    evt.DamageAmount = evt.DamageAmount * 0.5
```

---

## DeathEvent

Used in: `On_PlayerKilled`, `On_NPCKilled`

| Property | Type | Description |
|---|---|---|
| `Killer` | `Player` | Player who killed (null if environment or NPC) |
| `KillerEntity` | `Entity` | Entity that killed |
| `Victim` | `Player` | Player who died |
| `CauseOfDeath` | `string` | Cause of death (fall, fire, weapon, etc.) |

```python
def On_PlayerKilled(evt):
    if evt.Killer is not None:
        Server.Broadcast(evt.Killer.Name + " killed " + evt.Victim.Name +
                         " with " + evt.CauseOfDeath)
```

---

## GatherEvent

Used in: `On_PlayerGathering`

| Property | Type | Description |
|---|---|---|
| `Item` | `ItemDataBlock` | Item being gathered |
| `Quantity` | `int` | Amount (mutable — use for gather rate multipliers) |
| `GatherSource` | `object` | Source of the gather (resource node or corpse) |

```python
def On_PlayerGathering(player, evt):
    # 2x gather rate for VIP players
    if DataStore.ContainsKey("vips", str(player.UID)):
        evt.Quantity = evt.Quantity * 2
```

---

## DoorEvent

Used in: `On_DoorUse`

| Property | Type | Description |
|---|---|---|
| `Entity` | `Entity` | The door |
| `Player` | `Player` | Who attempted to use it |
| `Open` | `bool` | True if trying to open, False if closing |
| `State` | `BasicDoor.State` | Current door state |

```python
def On_DoorUse(player, evt):
    # Block access to a restricted zone
    if is_in_restricted_zone(evt.Entity.Location):
        if not player.Admin:
            evt.Cancel = True
            player.Message("Access restricted to administrators!")
```

---

## DecayEvent

Used in: `On_EntityDecay`

| Property | Type | Description |
|---|---|---|
| `Entity` | `Entity` | Entity undergoing decay |
| `DamageAmount` | `float` | Decay damage (mutable — set to 0 to disable) |

```python
def On_EntityDecay(evt):
    # Disable decay for all storage boxes
    if "Storage" in evt.Entity.Name:
        evt.DamageAmount = 0.0
```

---

## CraftingEvent

Used in: `On_Crafting`

| Property | Type | Description |
|---|---|---|
| `Player` | `Player` | Player crafting |
| `DataBlock` | `ItemDataBlock` | Item being crafted |
| `Amount` | `int` | Quantity |
| `Cancel` | `bool` | Cancels the crafting job |

---

## InventoryModEvent

Used in: `On_ItemAdded`, `On_ItemRemoved`

| Property | Type | Description |
|---|---|---|
| `Inventory` | `Inventory` | The modified inventory |
| `Item` | `IInventoryItem` | The item that was added or removed |
| `Container` | `ItemModContainerType` | Container type |

---

## BanEvent

Used in: `On_PlayerBan`

| Property | Type | Description |
|---|---|---|
| `Player` | `Player` | Player being banned |
| `Banner` | `string` | Name of the admin who banned |
| `Reason` | `string` | Ban reason |
| `IP` | `string` | Player's IP address |
| `ID` | `string` | Player's SteamID |

---

## PlayerApprovalEvent

Used in: `On_PlayerApproval`

```python
def On_PlayerApproval(evt):
    ip  = evt.NetPlayer.ipAddress
    uid = str(evt.NetUser.userID.Steam64)

    if DataStore.ContainsKey("blacklist", ip):
        evt.Deny("Your IP is blocked.")
    elif DataStore.ContainsKey("whitelist_only", "enabled"):
        if not DataStore.ContainsKey("whitelist", uid):
            evt.Deny("This server is whitelisted. Contact an admin.")
```

---

## ConsoleEvent

Used in: `On_ConsoleWithCancel`

| Property | Type | Description |
|---|---|---|
| `Arg` | `ConsoleSystem.Arg` | Command arguments |
| `Cancelled` | `bool` | Set to `True` to cancel execution |

```python
def On_ConsoleWithCancel(evt):
    cmd = evt.Arg.Class + "." + evt.Arg.Function
    if cmd == "server.quit":
        if not evt.Arg.isAdmin:
            evt.Cancelled = True
```

---

## TimedEvent

Passed to timer callbacks.

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Timer name |
| `PluginName` | `string` | Owner plugin name |
| `Args` | `Dictionary<string, object>` | Custom arguments (parallel timers) |
| `Interval` | `int` | Interval in milliseconds |
| `AutoReset` | `bool` | Whether the timer repeats |
| `ElapsedCount` | `int` | How many times it has fired |

```python
def MyTimerCallback(evt):
    print("Timer '" + evt.Name + "' fired " + str(evt.ElapsedCount) + " time(s)")

    if evt.ElapsedCount >= 5:
        evt.Kill()  # self-destroy after 5 fires
```
