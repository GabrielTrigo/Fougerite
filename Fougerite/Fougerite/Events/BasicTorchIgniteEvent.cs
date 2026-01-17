namespace Fougerite.Events
{
    /// <summary>
    /// Triggered when a player ignites a basic torch.
    /// </summary>
    public class BasicTorchIgniteEvent
    {
        private readonly Player _player;
        private readonly IBasicTorchItem _item;
        private readonly BasicTorchItemDataBlock _instance;
        private readonly uLink.BitStream _stream;
        private readonly ItemRepresentation _itemRep;
        private readonly uLink.NetworkMessageInfo _info;
        private bool _cancel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTorchIgniteEvent"/> class.
        /// </summary>
        /// <param name="instance">The BasicTorchItemDataBlock instance.</param>
        /// <param name="item">The basic torch item interface.</param>
        /// <param name="stream">The network bitstream.</param>
        /// <param name="itemRep">The item representation.</param>
        /// <param name="info">The network message info.</param>
        public BasicTorchIgniteEvent(BasicTorchItemDataBlock instance, IBasicTorchItem item, uLink.BitStream stream, ItemRepresentation itemRep, uLink.NetworkMessageInfo info)
        {
            _instance = instance;
            _item = item;
            _stream = stream;
            _itemRep = itemRep;
            _info = info;
            _cancel = false;

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
        /// Gets the Basic Torch item instance.
        /// </summary>
        public IBasicTorchItem Item
        {
            get
            {
                return _item;
            }
        }

        /// <summary>
        /// Gets the DataBlock associated with this torch.
        /// </summary>
        public BasicTorchItemDataBlock Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Gets the network bitstream.
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