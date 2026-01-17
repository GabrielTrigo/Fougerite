using System.Collections.Generic;
using System.Linq;

namespace Fougerite
{
    public class PlayerItem
    {
        private readonly Inventory internalInv;
        private readonly int internalSlot;
        private readonly PlayerInv _playerInv;

        public PlayerItem()
        {
        }

        public PlayerItem(ref Inventory inv, int slot)
        {
            internalInv = inv;
            internalSlot = slot;
        }
        
        public PlayerItem(ref Inventory inv, int slot, PlayerInv playerInv)
        {
            internalInv = inv;
            internalSlot = slot;
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
                DropHelper.DropItem(internalInv, Slot);
            }
        }

        /// <summary>
        /// Returns the IInventoryItem by internal slot.
        /// </summary>
        /// <returns></returns>
        private IInventoryItem GetItemRef()
        {
            IInventoryItem item;
            internalInv.GetItem(internalSlot, out item);
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
                return internalInv;
            }
        }

        /// <summary>
        /// Returns the slot of the item.
        /// </summary>
        public int InternalSlot
        {
            get
            {
                return internalSlot;
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
            set
            {
                RInventoryItem = value;
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
                RInventoryItem.SetUses(value);
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
        /// Gets the total number of mod slots available on the weapon.
        /// </summary>
        public int TotalModSlots
        {
            get
            {
                if (IsEmpty())
                    return 0;
                
                if (RInventoryItem is HeldItem<BulletWeaponDataBlock> bulletWeapon)
                {
                    return bulletWeapon.totalModSlots;
                }
                return 0;
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
                
                if (RInventoryItem is HeldItem<BulletWeaponDataBlock> bulletWeapon)
                {
                    return bulletWeapon.usedModSlots;
                }
                return 0;
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
                
                if (RInventoryItem is HeldItem<BulletWeaponDataBlock> bulletWeapon)
                {
                    return bulletWeapon.freeModSlots;
                }
                return 0;
            }
        }

        /// <summary>
        /// Removes a weapon mod from a specific slot (0-4) and optionally gives it back.
        /// </summary>
        public bool RemoveMod(int slot, bool giveBack = true)
        {
            if (IsEmpty()) 
                return false;
            
            if (RInventoryItem is HeldItem<BulletWeaponDataBlock> bulletWeapon)
            {
                ItemModDataBlock[] mods = bulletWeapon._itemMods;

                if (slot < 0 || slot >= mods.Length || mods[slot] == null)
                {
                    return false;
                }

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
                {
                    _playerInv.AddItem(modName, 1);
                }

                return true;
            }

            return false;
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
            {
                return modNames;
            }

            IInventoryItem item = RInventoryItem;
            
            if (item is HeldItem<BulletWeaponDataBlock> bulletWeapon)
            {
                int totalSlots = bulletWeapon.totalModSlots;
                ItemModDataBlock[] mods = bulletWeapon.itemMods;
                if (mods != null)
                {
                    for (int i = 0; i < totalSlots; i++)
                    {
                        if (mods[i] != null)
                        {
                            modNames.Add(mods[i].name);
                        }
                        else
                        {
                            modNames.Add("Empty slot");
                        }
                    }
                }

            }

            return modNames;
        }
    }
}