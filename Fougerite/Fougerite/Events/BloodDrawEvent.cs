namespace Fougerite.Events
{
    /// <summary>
    /// Triggered when a player uses a Blood Draw Kit to extract blood.
    /// This allows for modifying the health cost or cancelling the action entirely.
    /// </summary>
    public class BloodDrawEvent
    {
        private readonly Player _player;
        private readonly IBloodDrawItem _item;
        private float _bloodToTake;
        private int _itemAmount;
        private bool _cancelled;

        /// <summary>
        /// Initializes a new instance of the <see cref="BloodDrawEvent"/> class.
        /// </summary>
        /// <param name="item">The blood draw item instance being used.</param>
        /// <param name="bloodToTake">The initial health cost defined in the datablock.</param>
        public BloodDrawEvent(IBloodDrawItem item, float bloodToTake)
        {
            _item = item;
            _bloodToTake = bloodToTake;
            _itemAmount = 1;
            _cancelled = false;

            if (item.inventory != null && item.inventory.networkView != null)
            {
                _player = Player.FindByNetworkPlayer(item.inventory.networkView.owner);
            }
        }

        /// <summary>
        /// Gets the player who is using the blood draw kit.
        /// </summary>
        public Player Player
        {
            get
            {
                return _player;
            }
        }

        /// <summary>
        /// Gets the interface of the blood draw item being used.
        /// </summary>
        public IBloodDrawItem Item
        {
            get
            {
                return _item;
            }
        }
        
        /// <summary>
        /// Gets or sets the quantity of the item to give.
        /// </summary>
        public int ItemAmount
        {
            get
            {
                return _itemAmount;
            }
            set
            {
                _itemAmount = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of health that will be consumed from the player.
        /// </summary>
        public float BloodToTake
        {
            get
            {
                return _bloodToTake;
            }
            set
            {
                _bloodToTake = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the blood draw action is cancelled.
        /// </summary>
        public bool Cancelled
        {
            get
            {
                return _cancelled;
            }
        }

        /// <summary>
        /// Cancels the blood draw action.
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }
    }
}