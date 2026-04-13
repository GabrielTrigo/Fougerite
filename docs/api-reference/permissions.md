# API Reference — PermissionSystem

**Singleton:** `PermissionSystem.GetPermissionSystem()`  
**Global in scripts:** `PermissionSystem`  
**Files:**
- `/Save/GroupPermissions.json` — group definitions
- `/Save/PlayerPermissions.json` — per-player records

Fougerite's hierarchical permission system. Players inherit permissions from their assigned **groups** plus any **direct** permissions. Every player implicitly belongs to the built-in `Default` group.

---

## Concepts

```
Default (group — every player belongs here automatically)
├── permission.a
└── permission.b

VIP (group)
├── vip.commands.*     ← wildcard: matches any permission starting with "vip.commands."
└── vip.kits.daily

Player 76561198000001 (direct permissions + groups)
├── Groups:      ["VIP"]
└── Permissions: ["admin.tp"]
```

- **Wildcards** — `permission.*` matches any permission that starts with `permission.`
- **Super-wildcard** — `*` grants everything
- The `Default` group **always** applies and cannot be removed
- Matching is **case-insensitive**

---

## Checking Permissions

```python
ps = PermissionSystem

# Check if a player has a specific permission
if ps.PlayerHasPermission(player, "myplugin.use"):
    player.Message("Access granted!")

# Check by SteamID (useful for offline players)
if ps.PlayerHasPermission(player.UID, "myplugin.admin"):
    player.Message("Admin command executed.")

# Check group membership
if ps.PlayerHasGroup(player, "VIP"):
    player.Message("Welcome, VIP member!")
```

---

## Managing Player Permissions

```python
ps = PermissionSystem

# Register a new player record (required before adding direct permissions)
pPlayer = ps.CreatePermissionPlayer(player)

# Add a direct permission
ps.AddPermission(player, "myplugin.use")

# Remove a direct permission
ps.RemovePermission(player, "myplugin.use")

# Assign a player to a group
ps.AddGroupToPlayer(player.UID, "VIP")

# Remove a player from a group
ps.RemoveGroupFromPlayer(player.UID, "VIP")

# Remove a player's entire record
ps.RemovePermissionPlayer(player)

# Save changes to disk
ps.SaveToDisk()
```

---

## Managing Groups

```python
ps = PermissionSystem

# Create a new group
ps.CreateGroup("VIP", ["vip.commands.*", "vip.kits.daily"], "VIP Members")

# Add a permission to a group
ps.AddPermissionToGroup("VIP", "vip.home.extra")

# Remove a permission from a group
ps.RemovePermissionFromGroup("VIP", "vip.home.extra")

# Delete a group
ps.RemoveGroup("VIP")

# Save
ps.SaveToDisk()
```

---

## Querying

```python
ps = PermissionSystem

# Get all groups
groups = ps.GetPermissionGroups()
for g in groups:
    print(g.GroupName, g.GroupPermissions)

# Get all player records
players = ps.GetPermissionPlayers()

# Look up a specific player by SteamID
pPlayer = ps.GetPlayerBySteamID(player.UID)
if pPlayer is not None:
    print(pPlayer.Groups)
    print(pPlayer.Permissions)

# Look up a group by name
group = ps.GetGroupByName("VIP")
if group is not None:
    print(group.NickName)
    print(group.GroupPermissions)
```

---

## Reload from Disk

```python
# Reload both GroupPermissions.json and PlayerPermissions.json
PermissionSystem.ReloadPermissions()
```

---

## Temporary Permission Revocation

Disables **all** permissions for a player at runtime without modifying the JSON files. Reverts when the server restarts or you call `RemoveForceOffPermissions`.

```python
ps = PermissionSystem

# Revoke all permissions (including Default group)
ps.ForceOffPermissions(player.UID, True)

# Revoke only non-default permissions (Default group still applies)
ps.ForceOffPermissions(player.UID, False)

# Restore permissions
ps.RemoveForceOffPermissions(player.UID)

# Check if revoked
if ps.HasPermissionsForcedOff(player.UID):
    player.Message("Your permissions have been suspended.")
```

---

## Practical Example — Command Guard

```python
def On_Command(player, cmd, args):
    if cmd == "vipkit":
        if not PermissionSystem.PlayerHasPermission(player, "myplugin.vipkit"):
            player.Message("You don't have permission to use this command.")
            return

        player.Inventory.AddItem("Cooked Chicken", 5)
        player.Message("VIP kit granted!")

    elif cmd == "addvip" and player.Admin:
        if len(args) == 0:
            player.Message("Usage: /addvip <name>")
            return

        target = Server.FindPlayer(args[0])
        if target is None:
            player.Message("Player not found.")
            return

        PermissionSystem.CreatePermissionPlayer(target)
        PermissionSystem.AddGroupToPlayer(target.UID, "VIP")
        PermissionSystem.SaveToDisk()
        player.Message(target.Name + " is now a VIP.")
```
