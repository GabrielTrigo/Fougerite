namespace Fougerite.Events
{
    /// <summary>
    /// Types of armor changes.
    /// </summary>
    public enum ArmorChangeType
    {
        Equipped,
        Unequipped
    }
    
    /// <summary>
    /// Event triggered when armor is equipped or unequipped.
    /// </summary>
    public class ArmorEquipEvent
    {
        private readonly Player _player;
        private readonly IEquipmentItem _item;
        private readonly ArmorDataBlock _block;
        private readonly ArmorChangeType _changeType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmorEquipEvent"/> class.
        /// </summary>
        public ArmorEquipEvent(ArmorDataBlock block, IEquipmentItem item, ArmorChangeType changeType)
        {
            _block = block;
            _item = item;
            _changeType = changeType;

            if (item.inventory != null && item.inventory.networkView != null)
            {
                _player = Player.FindByNetworkPlayer(item.inventory.networkView.owner);
            }
        }

        /// <summary>
        /// Gets the player involved in the armor change.
        /// </summary>
        public Player Player
        {
            get { return _player; }
        }
        
        /// <summary>
        /// The type of armor change (equipped or unequipped).
        /// </summary>
        public ArmorChangeType ChangeType
        {
            get { return _changeType; }
        }

        /// <summary>
        /// Gets the specific armor item instance.
        /// </summary>
        public IEquipmentItem Item
        {
            get { return _item; }
        }

        /// <summary>
        /// Gets the DataBlock associated with this armor.
        /// </summary>
        public ArmorDataBlock ArmorBlock
        {
            get { return _block; }
        }
    }
}