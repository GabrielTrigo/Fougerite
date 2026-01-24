
namespace Fougerite.Events
{
    /// <summary>
    /// This class is created when an Item is added/removed from an inventory.
    /// </summary>
    public class InventoryModEvent
    {
        private readonly Inventory _inventory;
        private readonly Inventory _fromInventory;
        private readonly int _slot;
        private readonly IInventoryItem _item;
        private readonly Player _player = null;
        private readonly NetUser _netuser = null;
        private readonly uLink.NetworkPlayer _netplayer;
        private readonly string _etype;
        private readonly FInventory _finventory;
        private readonly FInventory _fromFinventory;
        private readonly EntityItem _entityitem;
        private bool _cancelled;

        /// <summary>
        /// Initializes the event. 
        /// </summary>
        /// <param name="inventory">The inventory where the action is happening.</param>
        /// <param name="slot">The target slot.</param>
        /// <param name="item">The item involved.</param>
        /// <param name="type">"Add" or "Remove".</param>
        /// <param name="fromInv">The source inventory involved (args.item.inventory during Add).</param>
        public InventoryModEvent(Inventory inventory, int slot, IInventoryItem item, string type, Inventory fromInv = null)
        {
            _inventory = inventory;
            _fromInventory = fromInv;
            _slot = slot;
            _item = item;
            _etype = type;
            if (inventory._netListeners != null) // This is null when Rust is filling up the boxes with loot.
            {
                foreach (uLink.NetworkPlayer netplayer in inventory._netListeners)
                {
                    if (netplayer.GetLocalData() is NetUser user)
                    {
                        _netuser = user;
                        _player = Server.GetServer().FindPlayer(_netuser.userID);
                        _netplayer = netplayer;
                        break;
                    }
                }
            }

            _finventory = new FInventory(_inventory);
            
            if (_fromInventory != null)
            {
                _fromFinventory = new FInventory(_fromInventory);
            }
            
            _entityitem = new EntityItem(_inventory, _slot, _finventory);
        }

        /// <summary>
        /// Cancels the event.
        /// Only works if the causer is a player.
        /// </summary>
        public void Cancel()
        {
            if (_netuser == null) return;
            if (_netuser.playerClient == null) return;
            if (_cancelled) return;
            _cancelled = true;
        }

        /// <summary>
        /// Gets if the event was cancelled.
        /// </summary>
        public bool Cancelled
        {
            get { return _cancelled; }
        }
        
        /// <summary>
        /// Returns true if the source inventory and target inventory are the same instance.
        /// </summary>
        public bool IsInternalMove
        {
            get
            {
                if (_inventory == null || _fromInventory == null) 
                    return false;
                
                return ReferenceEquals(_inventory, _fromInventory) || _inventory == _fromInventory;
            }
        }

        /// <summary>
        /// Gets the itemname of the item.
        /// </summary>
        public string ItemName
        {
            get { return _item.datablock.name; }
        }

        /// <summary>
        /// Gets the player if possible. Returns null if the causer of this event is not a player.
        /// </summary>
        public Player Player
        {
            get { return _player; }
        }

        /// <summary>
        /// Returns the netuser of the player.
        /// </summary>
        public NetUser NetUser
        {
            get { return _netuser; }
        }

        /// <summary>
        /// Returns the NetworkPlayer of the player.
        /// </summary>
        public uLink.NetworkPlayer NetPlayer
        {
            get { return _netplayer; }
        }

        /// <summary>
        /// Returns the original IInventoryItem class
        /// </summary>
        public IInventoryItem InventoryItem
        {
            get { return _item; }
        }

        /// <summary>
        /// Returns the Item as EntityItem.
        /// </summary>
        public EntityItem Item
        {
            get { return _entityitem; }
        }

        /// <summary>
        /// Gets the slot that the item is being moved to.
        /// </summary>
        public int Slot
        {
            get { return _slot; }
        }

        /// <summary>
        /// Gets the original inventory class.
        /// </summary>
        public Inventory Inventory
        {
            get { return _inventory; }
        }
        
        /// <summary>
        /// Gets the source inventory (where the item came from).
        /// </summary>
        public Inventory FromInventory
        {
            get { return _fromInventory; }
        }

        /// <summary>
        /// This getter tries to convert the Inventory to Fougerite's FInventory class.
        /// </summary>
        public FInventory FInventory
        {
            get { return _finventory; }
        }
        
        /// <summary>
        /// Gets the source inventory as Fougerite's FInventory class.
        /// </summary>
        public FInventory FromFInventory
        {
            get { return _fromFinventory; }
        }

        /// <summary>
        /// Returns the type of the event.
        /// </summary>
        public string Type
        {
            get { return _etype; }
        }
    }
}
