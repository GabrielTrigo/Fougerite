using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fougerite
{
    /// <summary>
    /// Represents a player's item within their inventory, providing methods to manipulate
    /// </summary>
    /// <remarks>
    /// IMPORTANT: Rust Legacy UI is heavily dependent on the Protobuf state.
    /// When modifying structural data (like Mod Slots), we use a "Protobuf Flush" pattern:
    /// 1. Use Save() to capture the current state.
    /// 2. Manually Re-Inject Identity: Protobuf's Build() fails if 'Id' or 'Name' are missing.
    /// 3. SetSlot() is mandatory: Without it, the client may move the item to slot 0 or 
    ///    fail to map the update to the correct UI square.
    /// 4. Load() & SetActiveItemManually(): This forces the client to discard the old UI 
    ///    and rebuild the inventory view from the new Protobuf data.
    /// </remarks>
    public class PlayerItem
    {
        private readonly Inventory _internalInv;
        private readonly int _internalSlot;
        private readonly PlayerInv _playerInv;

        /// <summary>
        /// Initializes a new, empty instance of the <see cref="PlayerItem"/> class.
        /// </summary>
        /// <remarks>
        /// This default constructor is currently unused and typically reserved for 
        /// serialization or initialization scenarios where values are assigned later.
        /// </remarks>
        public PlayerItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerItem"/> class using a direct reference 
        /// to a Rust <see cref="Inventory"/> and a specific slot index.
        /// </summary>
        /// <param name="inv">A reference to the underlying Rust <see cref="Inventory"/> component.</param>
        /// <param name="slot">The specific slot index this item instance will track.</param>
        /// <remarks>
        /// This constructor is currently unused but provides a lightweight way to wrap a Rust 
        /// item without attaching it to a specific Fougerite <see cref="PlayerInv"/>.
        /// </remarks>
        public PlayerItem(ref Inventory inv, int slot)
        {
            _internalInv = inv;
            _internalSlot = slot;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerItem"/> class, fully associated 
        /// with a Fougerite <see cref="PlayerInv"/> wrapper.
        /// </summary>
        /// <param name="inv">A reference to the underlying Rust <see cref="Inventory"/> component.</param>
        /// <param name="slot">The specific slot index this item instance will track.</param>
        /// <param name="playerInv">The Fougerite <see cref="PlayerInv"/> manager that owns this item.</param>
        /// <remarks>
        /// This is the primary constructor used throughout the framework. Passing <paramref name="playerInv"/> 
        /// is critical for functions like <see cref="AddMod"/>, as it allows the item to interact with 
        /// the player's overall inventory (e.g., removing a mod from the bag when attaching it to a weapon).
        /// </remarks>
        public PlayerItem(ref Inventory inv, int slot, PlayerInv playerInv)
        {
            _internalInv = inv;
            _internalSlot = slot;
            _playerInv = playerInv;
        }

        /// <summary>
        /// Consumes the item if its not empty.
        /// </summary>
        /// <param name="qty"></param>
        public void Consume(int qty)
        {
            if (!IsEmpty())
            {
                RInventoryItem.Consume(ref qty);
            }
        }

        /// <summary>
        /// Drops the item.
        /// </summary>
        public void Drop()
        {
            if (!IsEmpty())
            {
                DropHelper.DropItem(_internalInv, Slot);
            }
        }

        /// <summary>
        /// Retrieves the live reference of the <see cref="IInventoryItem"/> from the 
        /// internal Rust <see cref="Inventory"/> based on the pre-defined <see cref="_internalSlot"/>.
        /// </summary>
        /// <remarks>
        /// This is used internally to ensure the class is always working with the current 
        /// state of the item, which is critical if the item has been modified, moved, 
        /// or replaced since the <see cref="EntityItem"/> wrapper was instantiated.
        /// </remarks>
        /// <returns>
        /// The <see cref="IInventoryItem"/> found at the slot, returns null if the 
        /// slot is vacant or the inventory is invalid.
        /// </returns>
        private IInventoryItem GetItemRef()
        {
            IInventoryItem item;
            _internalInv.GetItem(_internalSlot, out item);
            return item;
        }

        /// <summary>
        /// Checks if the current item on the slot exists or not.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return (RInventoryItem == null);
        }

        /// <summary>
        /// Tries to combine this item with the specified one.
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public bool TryCombine(PlayerItem pi)
        {
            if (IsEmpty() || pi.IsEmpty())
            {
                return false;
            }
            return (RInventoryItem.TryCombine(pi.RInventoryItem) != InventoryItem.MergeResult.Failed);
        }

        /// <summary>
        /// Tries to stack this item with the specified one.
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public bool TryStack(PlayerItem pi)
        {
            if (IsEmpty() || pi.IsEmpty())
            {
                return false;
            }
            return (RInventoryItem.TryStack(pi.RInventoryItem) != InventoryItem.MergeResult.Failed);
        }

        /// <summary>
        /// Returns the inventory class of the item.
        /// </summary>
        public Inventory InternalInventory
        {
            get
            {
                return _internalInv;
            }
        }

        /// <summary>
        /// Returns the slot of the item.
        /// </summary>
        public int InternalSlot
        {
            get
            {
                return _internalSlot;
            }
        }

        /// <summary>
        /// Returns the original IInventoryItem class from Rust.
        /// </summary>
        public IInventoryItem RInventoryItem
        {
            get
            {
                return GetItemRef();
            }
        }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public string Name
        {
            get
            {
                if (!IsEmpty())
                {
                    return RInventoryItem.datablock.name;
                }
                return null;
            }
            set
            {
                RInventoryItem.datablock.name = value;
            }
        }

        /// <summary>
        /// Returns the amount of the item
        /// </summary>
        public int Quantity
        {
            get
            {
                return Util.UStackable.Contains(Name) ? 1 : UsesLeft;
            }
            set
            {
                UsesLeft = value;
            }
        }

        /// <summary>
        /// Gets the current slot of the item. Returns -1 if the item is empty.
        /// </summary>
        public int Slot
        {
            get
            {
                if (!IsEmpty())
                {
                    return RInventoryItem.slot;
                }
                return -1;
            }
        }

        /// <summary>
        /// Gets the uses left of this item.
        /// </summary>
        public int UsesLeft
        {
            get
            {
                if (!IsEmpty())
                {
                    return RInventoryItem.uses;
                }
                return -1;
            }
            set
            {
                if (!IsEmpty())
                {
                    RInventoryItem.SetUses(value);
                }
            }
        }
        
        /// <summary>
        /// A reference to the PlayerInv who owns this item.
        /// </summary>
        public PlayerInv PlayerInv
        {
            get
            {
                return _playerInv;
            }
        }

        /// <summary>
        /// Gets or sets the total number of mod slots available on the firearm. 
        /// When set, it triggers a full Protobuf rebuild and network sync to update the client UI.
        /// </summary>
        public int TotalModSlots
        {
            get
            {
                if (IsEmpty())
                    return 0;

                switch (RInventoryItem)
                {
                    case HeldItem<BulletWeaponDataBlock> bulletWeapon:
                        return bulletWeapon.totalModSlots;
                    case HeldItem<ShotgunDataBlock> shotgun:
                        return shotgun.totalModSlots;
                    default:
                        return 0;
                }
            }
            set
            {
                if (IsEmpty())
                    return;

                switch (RInventoryItem)
                {
                    case HeldItem<BulletWeaponDataBlock> bulletWeapon:
                        bulletWeapon.SetTotalModSlotCount(value);
                        RInventoryItem.MarkDirty();
                        _internalInv.MarkSlotDirty(InternalSlot);
                        _internalInv.SendMessage("UpdateToNetListeners", SendMessageOptions.DontRequireReceiver);
                        break;
                    case HeldItem<ShotgunDataBlock> shotgun:
                        shotgun.SetTotalModSlotCount(value);
                        RInventoryItem.MarkDirty();
                        _internalInv.MarkSlotDirty(InternalSlot);
                        _internalInv.SendMessage("UpdateToNetListeners", SendMessageOptions.DontRequireReceiver);
                        break;
                    default:
                        return;
                }
                
                RustProto.Item.Builder builder = RustProto.Item.CreateBuilder();
        
                // Save the current item state into the Protobuf builder
                RInventoryItem.Save(ref builder);
                
                // Set the basic item properties
                builder.SetId(RInventoryItem.datablock.uniqueID);
                builder.SetName(RInventoryItem.datablock.name);
                builder.SetSlot(_internalSlot);
                builder.SetCount(RInventoryItem.uses);
                builder.SetCondition(RInventoryItem.condition);
                builder.SetMaxcondition(RInventoryItem.maxcondition);
                
                // Set the subslots (mod slots) to the new value
                builder.SetSubslots(value);
        
                // Build the completed item
                RustProto.Item completedItem = builder.Build();
        
                // Re-load the item from the modified Protobuf data
                RInventoryItem.Load(ref completedItem);
                
                // Update
                RInventoryItem.MarkDirty();
        
                if (_internalInv != null)
                {
                    // Notify the inventory that the slot has changed
                    _internalInv.MarkSlotDirty(_internalSlot);
            
                    if (_internalInv.activeItem == RInventoryItem)
                    {
                        ItemRepresentation rep = null;
                        switch (RInventoryItem)
                        {
                            case HeldItem<BulletWeaponDataBlock> b:
                                rep = b.itemRepresentation;
                                break;
                            case HeldItem<ShotgunDataBlock> s:
                                rep = s.itemRepresentation;
                                break;
                        }
                
                        if (rep != null)
                            _internalInv.SetActiveItemManually(_internalSlot, rep);
                    }
            
                    _internalInv.SendMessage("UpdateToNetListeners", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        /// <summary>
        /// Gets the number of slots currently occupied by mods.
        /// </summary>
        public int UsedModSlots
        {
            get
            {
                if (IsEmpty())
                    return 0;

                switch (RInventoryItem)
                {
                    case HeldItem<BulletWeaponDataBlock> bulletWeapon:
                        return bulletWeapon.usedModSlots;
                    case HeldItem<ShotgunDataBlock> shotgun:
                        return shotgun.usedModSlots;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Gets the number of empty mod slots available.
        /// </summary>
        public int FreeModSlots
        {
            get
            {
                if (IsEmpty())
                    return 0;

                switch (RInventoryItem)
                {
                    case HeldItem<BulletWeaponDataBlock> bulletWeapon:
                        return bulletWeapon.freeModSlots;
                    case HeldItem<ShotgunDataBlock> shotgun:
                        return shotgun.freeModSlots;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Adds a weapon mod to the first available slot.
        /// </summary>
        public bool AddMod(string modName, bool removeFromInv = true)
        {
            ItemModDataBlock modBlock = DatablockDictionary.GetByName(modName) as ItemModDataBlock;
            if (modBlock == null || IsEmpty())
                return false;

            switch (RInventoryItem)
            {
                case HeldItem<BulletWeaponDataBlock> bulletWeapon:
                {
                    if (bulletWeapon.usedModSlots >= bulletWeapon.totalModSlots)
                        return false;

                    int slot = -1;
                    for (int i = 0; i < bulletWeapon._itemMods.Length; i++)
                    {
                        if (bulletWeapon._itemMods[i] == null)
                        {
                            slot = i;
                            break;
                        }
                    }

                    if (slot == -1) 
                        return false;

                    if (removeFromInv && _playerInv != null)
                    {
                        if (_playerInv.HasItem(modName, 1))
                            _playerInv.RemoveItem(modName, 1);
                        else
                            return false;
                    }
                
                    bulletWeapon._itemMods[slot] = modBlock;
                    bulletWeapon.RecalculateMods();

                    if (bulletWeapon.itemRepresentation != null)
                    {
                        ItemRepresentation rep = bulletWeapon.itemRepresentation;
                        MethodInfo getFlags = typeof(ItemRepresentation).GetMethod("GetCharacterStateFlags",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        CharacterStateFlags flags = (getFlags != null)
                            ? (CharacterStateFlags)getFlags.Invoke(rep, null)
                            : new CharacterStateFlags();
                        rep._itemMods.InstallMod(slot, rep, modBlock, flags);

                        uLink.BitStream stream = new uLink.BitStream(false);
                        stream.WriteByte((byte)bulletWeapon.usedModSlots);
                        for (int i = 0; i < bulletWeapon._itemMods.Length; i++)
                        {
                            if (bulletWeapon._itemMods[i] != null)
                                stream.WriteInt32(bulletWeapon._itemMods[i].uniqueID);
                        }

                        rep.networkView.RPC("Mods", uLink.RPCMode.OthersBuffered, stream.GetDataByteArray());
                    }
                
                    RInventoryItem.MarkDirty();
                    return true;
                }
                case HeldItem<ShotgunDataBlock> shotgun:
                {
                    if (shotgun.usedModSlots >= shotgun.totalModSlots)
                        return false;

                    int slot = -1;
                    for (int i = 0; i < shotgun._itemMods.Length; i++)
                    {
                        if (shotgun._itemMods[i] == null)
                        {
                            slot = i;
                            break;
                        }
                    }

                    if (slot == -1) 
                        return false;

                    if (removeFromInv && _playerInv != null)
                    {
                        if (_playerInv.HasItem(modName, 1))
                            _playerInv.RemoveItem(modName, 1);
                        else
                            return false;
                    }
                    
                    shotgun._itemMods[slot] = modBlock;
                    shotgun.RecalculateMods();

                    if (shotgun.itemRepresentation != null)
                    {
                        ItemRepresentation rep = shotgun.itemRepresentation;
                        MethodInfo getFlags = typeof(ItemRepresentation).GetMethod("GetCharacterStateFlags",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        CharacterStateFlags flags = (getFlags != null)
                            ? (CharacterStateFlags)getFlags.Invoke(rep, null)
                            : new CharacterStateFlags();
                        rep._itemMods.InstallMod(slot, rep, modBlock, flags);

                        uLink.BitStream stream = new uLink.BitStream(false);
                        stream.WriteByte((byte)shotgun.usedModSlots);
                        for (int i = 0; i < shotgun._itemMods.Length; i++)
                        {
                            if (shotgun._itemMods[i] != null)
                                stream.WriteInt32(shotgun._itemMods[i].uniqueID);
                        }

                        rep.networkView.RPC("Mods", uLink.RPCMode.OthersBuffered, stream.GetDataByteArray());
                    }
                    
                    RInventoryItem.MarkDirty();
                    return true;
                }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Removes a weapon mod from a specific slot (0-4) and optionally gives it back.
        /// </summary>
        public bool RemoveMod(int slot, bool giveBack = true)
        {
            if (IsEmpty())
                return false;

            switch (RInventoryItem)
            {
                case HeldItem<BulletWeaponDataBlock> bulletWeapon:
                {
                    ItemModDataBlock[] mods = bulletWeapon._itemMods;
                    if (slot < 0 || slot >= mods.Length || mods[slot] == null) 
                        return false;

                    string modName = mods[slot].name;
                    mods[slot] = null;
                    bulletWeapon.RecalculateMods();

                    if (bulletWeapon.itemRepresentation != null)
                    {
                        ItemRepresentation rep = bulletWeapon.itemRepresentation;
                        if (slot < 5)
                        {
                            ItemRepresentation.ItemModPair pair = rep._itemMods[slot];
                            rep.KillModRep(ref pair.representation, false);
                            pair.dataBlock = null;
                            pair.bindState = ItemRepresentation.BindState.Vacant;
                            rep._itemMods[slot] = pair;
                        }
                    }

                    RInventoryItem.MarkDirty();
                    if (giveBack && _playerInv != null)
                        _playerInv.AddItem(modName, 1);
                    
                    return true;
                }
                case HeldItem<ShotgunDataBlock> shotgun:
                {
                    ItemModDataBlock[] mods = shotgun._itemMods;
                    if (slot < 0 || slot >= mods.Length || mods[slot] == null)
                        return false;

                    string modName = mods[slot].name;
                    mods[slot] = null;
                    shotgun.RecalculateMods();

                    if (shotgun.itemRepresentation != null)
                    {
                        ItemRepresentation rep = shotgun.itemRepresentation;
                        if (slot < 5)
                        {
                            ItemRepresentation.ItemModPair pair = rep._itemMods[slot];
                            rep.KillModRep(ref pair.representation, false);
                            pair.dataBlock = null;
                            pair.bindState = ItemRepresentation.BindState.Vacant;
                            rep._itemMods[slot] = pair;
                        }
                    }

                    RInventoryItem.MarkDirty();
                    if (giveBack && _playerInv != null)
                        _playerInv.AddItem(modName, 1);
                    
                    return true;
                }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Removes every mod from the firearm.
        /// </summary>
        public bool ClearMods(bool giveBack = true)
        {
            if (IsEmpty())
                return false;

            bool anyRemoved = false;
            for (int i = 0; i < 5; i++)
            {
                if (RemoveMod(i, giveBack))
                {
                    anyRemoved = true;
                }
            }

            return anyRemoved;
        }

        /// <summary>
        /// Returns a list of mod names currently attached to this item.
        /// </summary>
        public List<string> GetMods()
        {
            List<string> modNames = new List<string>(5);
            if (IsEmpty()) 
                return modNames;

            switch (RInventoryItem)
            {
                case HeldItem<BulletWeaponDataBlock> bulletWeapon:
                {
                    int totalSlots = bulletWeapon.totalModSlots;
                    ItemModDataBlock[] mods = bulletWeapon.itemMods;
                    if (mods != null)
                    {
                        for (int i = 0; i < totalSlots; i++)
                        {
                            modNames.Add(mods[i] != null ? mods[i].name : "Empty slot");
                        }
                    }

                    break;
                }
                case HeldItem<ShotgunDataBlock> shotgun:
                {
                    int totalSlots = shotgun.totalModSlots;
                    ItemModDataBlock[] mods = shotgun.itemMods;
                    if (mods != null)
                    {
                        for (int i = 0; i < totalSlots; i++)
                        {
                            modNames.Add(mods[i] != null ? mods[i].name : "Empty slot");
                        }
                    }

                    break;
                }
            }

            return modNames;
        }
    }
}