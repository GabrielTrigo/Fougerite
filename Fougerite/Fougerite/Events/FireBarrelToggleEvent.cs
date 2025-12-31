namespace Fougerite.Events
{
    /// <summary>
    /// Runs when a FireBarrel is toggled on or off.
    /// Campfire or Furnace basically.
    /// </summary>
    public class FireBarrelToggleEvent
    {
        private readonly FireBarrel _fireBarrel;
        private readonly Entity _entity;
        private readonly Player _lastUser;
        private bool _on;
        private bool _cancelled;
        private float _cookDuration;
        
        public FireBarrelToggleEvent(FireBarrel fireBarrel, bool on, Entity entity, float cookDuration)
        {
            _fireBarrel = fireBarrel;
            _on = on;
            _entity = entity;
            _cancelled = false;
            _cookDuration = cookDuration;
            
            var lastControllableUser = fireBarrel.lastControllableUser;
            if (lastControllableUser != null)
            {
                var playerClient = lastControllableUser.playerClient;
                if (playerClient != null)
                    _lastUser = Server.GetServer().FindPlayer(playerClient.userID);
            }
        }
        
        /// <summary>
        /// Returns the FireBarrel that is being toggled.
        /// </summary>
        public FireBarrel FireBarrel
        {
            get { return _fireBarrel; }
        }
        
        /// <summary>
        /// True if turning on, False if turning off.
        /// In some cases you can set this value to true, for example when the furnace is off
        /// the code still creates this event with On = false, but you can set it to true to turn the furnace on.
        /// It would only work if you do not cancel the event.
        /// </summary>
        public bool On
        {
            get { return _on; }
            set { _on = value; }
        }
        
        /// <summary>
        /// Returns the Entity of the FireBarrel.
        /// </summary>
        public Entity Entity
        {
            get { return _entity; }
        }
        
        /// <summary>
        /// Gets or sets the cook duration.
        /// Calculated originally by:
        /// float cookDuration = fireBarrel.GetCookDuration();
        /// cookDuration = UnityEngine.Random.Range(cookDuration * 0.5f, cookDuration);
        /// </summary>
        public float CookDuration
        {
            get { return _cookDuration; }
            set { _cookDuration = value; }
        }
        
        /// <summary>
        /// Returns the last user who set the FireBarrel on or off.
        /// May be null.
        /// </summary>
        public Player LastUser
        {
            get { return _lastUser; }
        }
        
        /// <summary>
        /// Returns if the event was cancelled.
        /// </summary>
        public bool Cancelled
        {
            get { return _cancelled; }
        }
        
        /// <summary>
        /// Cancels the event.
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }
    }
}