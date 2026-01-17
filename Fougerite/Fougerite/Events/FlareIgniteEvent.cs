namespace Fougerite.Events
{
    /// <summary>
    /// Triggered when a player ignites a torch.
    /// </summary>
    public class FlareIgniteEvent
    {
        private readonly Player _player;
        private readonly ITorchItem _item;
        private readonly TorchItemDataBlock _instance;
        private readonly uLink.BitStream _stream;
        private readonly ItemRepresentation _itemRep;
        private readonly uLink.NetworkMessageInfo _info;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlareIgniteEvent"/> class.
        /// </summary>
        /// <param name="instance">The TorchItemDataBlock datablock.</param>
        /// <param name="item">The torch item interface.</param>
        /// <param name="stream">The network bitstream.</param>
        /// <param name="itemRep">The item representation.</param>
        /// <param name="info">The network message info.</param>
        public FlareIgniteEvent(TorchItemDataBlock instance, ITorchItem item, uLink.BitStream stream, ItemRepresentation itemRep, uLink.NetworkMessageInfo info)
        {
            _instance = instance;
            _item = item;
            _stream = stream;
            _itemRep = itemRep;
            _info = info;

            if (item.inventory != null && item.inventory.networkView != null)
            {
                _player = Player.FindByNetworkPlayer(item.inventory.networkView.owner);
            }
        }

        /// <summary>
        /// Gets the player who ignited the torch.
        /// </summary>
        public Player Player
        {
            get
            {
                return _player;
            }
        }

        /// <summary>
        /// Gets the Torch item instance.
        /// </summary>
        public ITorchItem Item
        {
            get
            {
                return _item;
            }
        }

        /// <summary>
        /// Gets the DataBlock associated with this torch.
        /// </summary>
        public TorchItemDataBlock Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Gets the network bitstream for this action.
        /// </summary>
        public uLink.BitStream Stream
        {
            get
            {
                return _stream;
            }
        }

        /// <summary>
        /// Gets the item representation.
        /// </summary>
        public ItemRepresentation ItemRep
        {
            get
            {
                return _itemRep;
            }
        }

        /// <summary>
        /// Gets the network message information.
        /// </summary>
        public uLink.NetworkMessageInfo Info
        {
            get
            {
                return _info;
            }
        }
    }
}