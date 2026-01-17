namespace Fougerite.Events
{
    /// <summary>
    /// Triggered when an ItemMod (attachment) is being installed onto an Item.
    /// This allows for restricting attachments or modifying installation logic.
    /// </summary>
    public class ItemModInstallEvent<T> where T : HeldItemDataBlock
    {
        private readonly ItemRepresentation _itemRep;
        private readonly ItemModDataBlock _modData;
        private readonly Player _player;
        private readonly CharacterStateFlags _flags;
        private readonly int _slot;
        private readonly HeldItem<T> _held;
        private bool _cancelled;

        public ItemModInstallEvent(HeldItem<T> held, ItemModDataBlock modData)
        {
            _held = held;
            _modData = modData;
            _slot = held.usedModSlots;
            _itemRep = held.itemRepresentation;
            
            if (held is IInventoryItem item)
            {
                if (item.character != null)
                {
                    _flags = item.character.stateFlags;
                }
                
                // Inventory may be null when the player is joining and so on.
                if (item.inventory != null && item.inventory.networkView != null)
                {
                    _player = Player.FindByNetworkPlayer(item.inventory.networkView.owner);
                }
            }
        }
        
        /// <summary>
        /// Gets the HeldItem receiving the modification.
        /// </summary>
        public HeldItem<T> HeldItem
        {
            get { return _held; }
        }

        /// <summary>
        /// Gets the ItemRepresentation receiving the modification.
        /// </summary>
        public ItemRepresentation ItemRep
        {
            get { return _itemRep; }
        }

        /// <summary>
        /// Gets the DataBlock of the modification being installed.
        /// </summary>
        public ItemModDataBlock ModData
        {
            get { return _modData; }
        }
        
        /// <summary>
        /// Gets the character state flags at the time of installation.
        /// </summary>
        public CharacterStateFlags Flags
        {
            get { return _flags; }
        }

        /// <summary>
        /// Gets the slot index (0-4) where the mod is being installed.
        /// </summary>
        public int Slot
        {
            get { return _slot; }
        }

        /// <summary>
        /// Gets the Player who owns the item receiving the mod. May be null.
        /// </summary>
        public Player Player
        {
            get { return _player; }
        }

        /// <summary>
        /// Returns true if the mod installation has been cancelled.
        /// </summary>
        public bool Cancelled
        {
            get { return _cancelled; }
        }

        /// <summary>
        /// Prevents the mod from being installed on the item.
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }
    }
}