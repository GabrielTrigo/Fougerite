namespace Fougerite.Events
{
    public class BeltUseEvent
    {
        private readonly InventoryHolder _holder;
        private readonly int _belt;
        private readonly Player _player;
        private bool _bypass = false;
        private bool _cancelled = false;
        
        public BeltUseEvent(InventoryHolder holder, int belt)
        {
            _holder = holder;
            _belt = belt;
            if (holder.netUser != null)
            {
                _player = Server.GetServer().FindPlayer(holder.netUser.userID);
            }
        }

        /// <summary>
        /// Returns the slot number of the belt from 0-6
        /// </summary>
        public int SelectedBelt
        {
            get { return _belt; }
        }

        /// <summary>
        /// InventoryHolder that is using the belt.
        /// </summary>
        public InventoryHolder InventoryHolder
        {
            get { return _holder; }
        }

        /// <summary>
        /// The Player who is using the belt.
        /// </summary>
        public Player Player
        {
            get { return _player; }
        }

        /// <summary>
        /// Returns true if the cooldown check is bypassed for the belt use.
        /// </summary>
        public bool Bypassed
        {
            get { return _bypass; }
        }

        /// <summary>
        /// Returns true if the event was cancelled.
        /// </summary>
        public bool Cancelled
        {
            get { return _cancelled; }
        }

        /// <summary>
        /// Cancels the belt use action.
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }

        /// <summary>
        /// Bypasses the cooldown check, so there is no time limit when selecting different items.
        /// </summary>
        public void BypassBeltCooldown()
        {
            _bypass = true;
        }
    }
}