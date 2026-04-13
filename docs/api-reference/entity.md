# API Reference — Fougerite.Entity

**Type:** Per-entity instance  
**Global in scripts:** `entity` (parameter passed in hooks)

Wrapper around world objects: deployables (boxes, furnaces, traps), building structures, and lootable containers.

---

## Identification and Type

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Prefab name (e.g. `"Wood Storage Box"`) |
| `InstanceID` | `int` | Unique Unity instance ID |
| `IsDeployable` | `bool` | True if it is a deployed object (box, campfire, etc.) |
| `IsStructure` | `bool` | True if it is a building component (wall, floor, etc.) |
| `IsStructureMaster` | `bool` | True if it is the StructureMaster (full building base) |
| `IsSleeper` | `bool` | True if it is a sleeper object |
| `IsFireBarrel` | `bool` | True if it is a fire barrel |
| `hasInventory` | `bool` | True if the object has an inventory (furnace, box, campfire, etc.) |

---

## Position

| Property | Type | Description |
|---|---|---|
| `Location` | `Vector3` | World position |
| `X` | `float` | X coordinate |
| `Y` | `float` | Y coordinate (height) |
| `Z` | `float` | Z coordinate |

---

## Ownership and Creation

| Property | Type | Description |
|---|---|---|
| `Owner` | `Player` | Current owner of the entity |
| `OwnerID` | `string` | Owner SteamID as string |
| `Creator` | `Player` | Who placed or created the entity |
| `CreatorID` | `string` | Creator SteamID as string |

```python
# Change the entity owner
entity.ChangeOwner(newPlayer)

# Access the raw Unity object
raw = entity.Object
```

---

## Health and Damage

| Member | Description |
|---|---|
| `Health` (float) | Current entity health |
| `GetTakeDamage()` | Returns the TakeDamage component for direct manipulation |
| `UpdateHealth()` | Forces a visual health update |
| `SetDecayEnabled(bool)` | *(Deprecated — no effect)* |

---

## Container Inventory

```python
if entity.hasInventory:
    inv = entity.Inventory  # EntityInv

    # Add item to container
    inv.AddItem("Wood", 500)

    # Remove item from container
    inv.RemoveItem("Wood", 100)
```

---

## Linked Structures

```python
# Get all linked structural entities (returns List<Entity>)
linked = entity.GetLinkedStructs()
for struct in linked:
    print("Linked structure:", struct.Name)
```

---

## Destruction

```python
entity.Destroy()
```

---

## Native Type Access (C# only)

```csharp
var deployable = entity.GetObject<DeployableObject>();
var structure  = entity.GetObject<StructureComponent>();
var lootable   = entity.GetObject<LootableObject>();
```

---

## Practical Examples

### Log deployed entities

```python
def On_EntityDeployed(player, entity, placer):
    Plugin.Log("deploy", placer.Name + " placed " + entity.Name +
               " at " + str(entity.Location))
```

### Disable decay on storage boxes

```python
def On_EntityDecay(evt):
    if "Wood Storage" in evt.Entity.Name:
        evt.DamageAmount = 0.0
```

### Log destroyed entities

```python
def On_EntityDestroyed(evt):
    if evt.Entity.Name == "Large Wood Storage":
        Plugin.Log("destroy", "Box destroyed at " + str(evt.Entity.Location) +
                   " (owner: " + evt.Entity.OwnerID + ")")
```
