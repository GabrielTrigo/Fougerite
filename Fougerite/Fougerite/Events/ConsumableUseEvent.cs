namespace Fougerite.Events
{
    /// <summary>
    /// This event is triggered when a player uses a consumable item.
    /// It provides access to all nutritional and medicinal values.
    /// You can modify these values or cancel the event entirely.
    /// </summary>
    public class ConsumableUseEvent
    {
        private readonly ConsumableDataBlock _dataBlock;
        private readonly IConsumableItem _item;
        private readonly Player _player;
        private bool _cancelled;
        private float _calories;
        private float _water;
        private float _antiRads;
        private float _healthToHeal;
        private float _poisonAmount;
        private int _amountToConsume;

        public ConsumableUseEvent(ConsumableDataBlock dataBlock, IConsumableItem item)
        {
            _dataBlock = dataBlock;
            _item = item;
            _cancelled = false;

            // Initialize fields with default values from the DataBlock
            _calories = dataBlock.calories;
            _water = dataBlock.litresOfWater;
            _antiRads = dataBlock.antiRads;
            _healthToHeal = dataBlock.healthToHeal;
            _poisonAmount = dataBlock.poisonAmount;
            _amountToConsume = 1;
            
            if (item.inventory != null)
            {
                _player = Player.FindByNetworkPlayer(item.inventory.networkView.owner);
            }
        }

        /// <summary>
        /// Returns the static DataBlock associated with this item type.
        /// Use this to check item properties like name or description.
        /// </summary>
        public ConsumableDataBlock DataBlock
        {
            get
            {
                return _dataBlock;
            }
        }

        /// <summary>
        /// Returns the specific item instance in the inventory.
        /// Use this to access slot information or stack size.
        /// </summary>
        public IConsumableItem Item
        {
            get
            {
                return _item;
            }
        }

        /// <summary>
        /// Returns the Fougerite Player object who is consuming the item.
        /// This may be null if the player cannot be resolved.
        /// </summary>
        public Player Player
        {
            get
            {
                return _player;
            }
        }

        /// <summary>
        /// Gets or sets the amount of calories to add to the player.
        /// This is limited by the player's remaining caloric space.
        /// </summary>
        public float Calories
        {
            get
            {
                return _calories;
            }
            set
            {
                _calories = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of water (liters) to add to the metabolism.
        /// </summary>
        public float Water
        {
            get
            {
                return _water;
            }
            set
            {
                _water = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of radiation to remove from the player.
        /// </summary>
        public float AntiRads
        {
            get
            {
                return _antiRads;
            }
            set
            {
                _antiRads = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of health to heal over time.
        /// Use negative values to inflict immediate damage instead.
        /// </summary>
        public float HealthToHeal
        {
            get
            {
                return _healthToHeal;
            }
            set
            {
                _healthToHeal = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of poison to add to the metabolism.
        /// </summary>
        public float PoisonAmount
        {
            get
            {
                return _poisonAmount;
            }
            set
            {
                _poisonAmount = value;
            }
        }

        /// <summary>
        /// Gets or sets how many items will be removed from the stack.
        /// Default is 1. Setting this to 0 prevents item loss.
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
        /// Returns whether the event has been cancelled by a plugin.
        /// </summary>
        public bool Cancelled
        {
            get
            {
                return _cancelled;
            }
        }

        /// <summary>
        /// Cancels the event. The item will not be consumed and
        /// no metabolic changes will occur.
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }
    }
}