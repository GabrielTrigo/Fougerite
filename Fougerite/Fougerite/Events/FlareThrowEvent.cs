using UnityEngine;

namespace Fougerite.Events
{
    /// <summary>
    /// Triggered when a player throws a Flare using a TorchItemDataBlock.
    /// </summary>
    public class FlareThrowEvent
    {
        private readonly Player _player;
        private readonly ITorchItem _item;
        private readonly TorchItemDataBlock _instance;
        private readonly Vector3 _origin;
        private readonly Vector3 _forward;
        private bool _cancel;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlareThrowEvent"/> class.
        /// </summary>
        /// <param name="instance">The TorchItemDataBlock datablock.</param>
        /// <param name="item">The torch item interface.</param>
        /// <param name="origin">The world position where the flare is thrown from.</param>
        /// <param name="forward">The direction vector of the throw.</param>
        public FlareThrowEvent(TorchItemDataBlock instance, ITorchItem item, Vector3 origin, Vector3 forward)
        {
            _instance = instance;
            _item = item;
            _origin = origin;
            _forward = forward;
            _cancel = false;

            if (item.inventory != null && item.inventory.networkView != null)
            {
                _player = Player.FindByNetworkPlayer(item.inventory.networkView.owner);
            }
        }

        /// <summary>
        /// Gets the player who threw the flare.
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
        /// Gets the DataBlock associated with this flare.
        /// </summary>
        public TorchItemDataBlock Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Gets the world position the flare originated from.
        /// </summary>
        public Vector3 Origin
        {
            get
            {
                return _origin;
            }
        }

        /// <summary>
        /// Gets the direction vector the flare was thrown in.
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                return _forward;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the throw action is cancelled.
        /// </summary>
        public bool Cancelled
        {
            get
            {
                return _cancel;
            }
        }

        /// <summary>
        /// Cancels the flare throw action.
        /// </summary>
        public void Cancel()
        {
            _cancel = true;
        }
    }
}