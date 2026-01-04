namespace Fougerite.Events
{
    /// <summary>
    /// This event is fired when an animal NPC moves.
    /// </summary>
    public class AnimalMovementEvent
    {
        private readonly NPC _npc;
        private readonly NavMeshMovement _movement;
        private readonly ulong _simMillis;
        
        public AnimalMovementEvent(NPC npc, NavMeshMovement movement, ulong simMillis)
        {
            _npc = npc;
            _movement = movement;
            _simMillis = simMillis;
        }

        /// <summary>
        /// The NPC that is moving.
        /// </summary>
        public NPC NPC
        {
            get { return _npc; }
        }
        
        /// <summary>
        /// The NavMeshMovement component of the animal.
        /// </summary>
        public NavMeshMovement NavMeshMovement
        {
            get { return _movement; }
        }
        
        /// <summary>
        /// The simulation time in milliseconds when the movement update occurred.
        /// Calculated by WildlifeManager. It's obfuscated in the original code.
        /// Figure it out if you must.
        /// </summary>
        public ulong SimMillis
        {
            get { return _simMillis; }
        }
    }
}