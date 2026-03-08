namespace Fougerite.Events
{
    /// <summary>
    /// This class is created when the player suffers fall damage.
    /// </summary>
    public class FallDamageEvent
    {
        private readonly float _fallspeed;
        private readonly FallDamage _fd;
        private bool _flag;
        private bool _flag2;
        private readonly Player _player;
        private bool _cancelled;
        private float _num;

        public FallDamageEvent(FallDamage fd, float speed, float num, bool flag, bool flag2)
        {
            _fd = fd;
            _player = Server.GetServer().FindPlayer(fd.idMain.netUser.userID);
            _fallspeed = speed;
            _num = num;
            _flag = flag;
            _flag2 = flag2;
        }

        /// <summary>
        /// Gets the player of the event.
        /// </summary>
        public Player Player
        {
            get { return _player; }
        }

        /// <summary>
        /// Gets the speed of the fall.
        /// </summary>
        public float FloatSpeed
        {
            get { return _fallspeed; }
        }

        /// <summary>
        /// Gets or sets the damage of the fall damage.
        /// 10f + num * fd.maxHealth
        /// </summary>
        public float Num
        {
            get { return _num; }
            set { _num = value; }
        }

        /// <summary>
        /// Returns the original FallDamage class
        /// </summary>
        public FallDamage FallDamage
        {
            get { return _fd; }
        }
        
        /// <summary>
        /// Checks if the player is going to bleed from this event.
        /// Setting this to false will prevent the player from bleeding, but they will still take fall damage.
        /// </summary>
        public bool Bleeding
        {
            get { return _flag; }
            set { _flag = value; }
        }

        /// <summary>
        /// Checks if the player is going to get broken legs from this event.
        /// Setting this to false will prevent the player from getting broken legs, but they will still take fall damage.
        /// </summary>
        public bool BrokenLegs
        {
            get { return _flag2; }
            set { _flag2 = value; }
        }
        
        /// <summary>
        /// Returns true if the whole event is cancelled, false otherwise.
        /// </summary>
        public bool Cancelled
        {
            get { return _cancelled; }
        }

        /// <summary>
        /// Cancels the fall damage event.
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }
    }
}
