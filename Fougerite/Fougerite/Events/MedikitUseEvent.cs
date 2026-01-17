namespace Fougerite.Events
{
    /// <summary>
    /// Event data for medical kit and bandage usage.
    /// Provides access to healing amounts and bleeding status before application.
    /// </summary>
    public class MedikitUseEvent
    {
        private readonly BasicHealthKitDataBlock _dataBlock;
        private readonly IBasicHealthKit _item;
        private readonly Player _player;
        private bool _cancelled;
        private float _healthAddMin;
        private float _healthAddMax;
        private bool _stopsBleeding;
        private int _amountToConsume;

        public MedikitUseEvent(BasicHealthKitDataBlock dataBlock, IBasicHealthKit item)
        {
            _dataBlock = dataBlock;
            _item = item;
            _cancelled = false;

            // Initialize values from the DataBlock defaults
            _healthAddMin = dataBlock.healthAddMin;
            _healthAddMax = dataBlock.healthAddMax;
            _stopsBleeding = dataBlock.stopsBleeding;
            _amountToConsume = 1;

            if (item.inventory != null && item.inventory.networkView != null)
            {
                _player = Player.FindByNetworkPlayer(item.inventory.networkView.owner);
            }
        }

        /// <summary>
        /// Returns the static DataBlock defining this medical item's base properties.
        /// </summary>
        public BasicHealthKitDataBlock DataBlock
        {
            get
            {
                return _dataBlock;
            }
        }

        /// <summary>
        /// Returns the specific IBasicHealthKit item instance.
        /// </summary>
        public IBasicHealthKit Item
        {
            get
            {
                return _item;
            }
        }

        /// <summary>
        /// Returns the Player object using the medical kit. May be null.
        /// </summary>
        public Player Player
        {
            get
            {
                return _player;
            }
        }

        /// <summary>
        /// Gets or sets the minimum health amount to be restored.
        /// </summary>
        public float HealthAddMin
        {
            get
            {
                return _healthAddMin;
            }
            set
            {
                _healthAddMin = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum health amount to be restored.
        /// </summary>
        public float HealthAddMax
        {
            get
            {
                return _healthAddMax;
            }
            set
            {
                _healthAddMax = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this use will stop the player's bleeding.
        /// </summary>
        public bool StopsBleeding
        {
            get
            {
                return _stopsBleeding;
            }
            set
            {
                _stopsBleeding = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of items to consume from the stack.
        /// </summary>
        public int AmountToConsume
        {
            get
            {
                return _amountToConsume;
            }
            set
            {
                _amountToConsume = value;
            }
        }

        /// <summary>
        /// Returns true if the event has been cancelled.
        /// </summary>
        public bool Cancelled
        {
            get
            {
                return _cancelled;
            }
        }

        /// <summary>
        /// Cancels the medical use. No healing or bleeding removal occurs.
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }
    }
}