namespace Fougerite.Events
{
    /// <summary>
    /// This class is created when a player is gathering from an animal or from a resource.
    /// </summary>
    public class GatherEvent
    {
        private string _item;
        private bool _over;
        private int _qty;
        private readonly string _type;
        private readonly ResourceTarget _res;
        private readonly ItemDataBlock _dataBlock;
        private readonly ResourceGivePair _resourceGivePair;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatherEvent"/> class for Tree gathering.
        /// </summary>
        /// <param name="r">The resource target being hit.</param>
        /// <param name="db">The datablock of the item being gathered.</param>
        /// <param name="qty">The initial quantity to be gathered.</param>
        public GatherEvent(ResourceTarget r, ItemDataBlock db, int qty)
        {
            _res = r;
            _qty = qty;
            _item = db.name;
            _type = "Tree";
            _dataBlock = db;
            Override = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GatherEvent"/> class for general resource gathering.
        /// </summary>
        /// <param name="r">The resource target being hit.</param>
        /// <param name="gp">The resource give pair containing the item data.</param>
        /// <param name="qty">The initial quantity to be gathered.</param>
        public GatherEvent(ResourceTarget r, ResourceGivePair gp, int qty)
        {
            _res = r;
            _qty = qty;
            _item = gp.ResourceItemDataBlock.name;
            _dataBlock = gp.ResourceItemDataBlock;
            _type = _res.type.ToString();
            _resourceGivePair = gp;
            Override = false;
        }

        /// <summary>
        /// Gets the total amount of resources remaining in the target object.
        /// </summary>
        public int AmountLeft
        {
            get
            {
                return _res.GetTotalResLeft();
            }
        }

        /// <summary>
        /// Gets or sets the name of the item that the player will receive.
        /// Changing this allows a plugin to swap the resource gathered (Like M4, and so on).
        /// </summary>
        public string Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the gathering logic should bypass resource limits.
        /// If false, the quantity is capped at the amount actually remaining in the resource.
        /// If true, the player can receive the full <see cref="Quantity"/> regardless of the object's remaining health.
        /// </summary>
        public bool Override
        {
            get
            {
                return _over;
            }
            set
            {
                _over = value;
            }
        }

        /// <summary>
        /// Gets the current percentage of resources remaining in the target.
        /// </summary>
        public float PercentFull
        {
            get
            {
                return _res.GetPercentFull();
            }
        }

        /// <summary>
        /// Gets or sets the quantity of items to be gathered. 
        /// IMPORTANT: Setting this value to 0 will effectively cancel the gathering event, 
        /// resulting in the player receiving no items and the object receiving no damage.
        /// </summary>
        public int Quantity
        {
            get
            {
                return _qty;
            }
            set
            {
                _qty = value;
            }
        }

        /// <summary>
        /// Gets the type of resource we are hitting.
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the original <see cref="ResourceTarget"/> object being interacted with.
        /// </summary>
        public ResourceTarget ResourceTarget
        {
            get
            {
                return _res;
            }
        }

        /// <summary>
        /// Gets the <see cref="ItemDataBlock"/> representing the gathered resource.
        /// </summary>
        public ItemDataBlock ItemDataBlock
        {
            get
            {
                return _dataBlock;
            }
        }

        /// <summary>
        /// Gets the original <see cref="ResourceGivePair"/> associated with this gather.
        /// Null if gathering from a Tree.
        /// </summary>
        public ResourceGivePair ResourceGivePair
        {
            get
            {
                return _resourceGivePair;
            }
        }
    }
}