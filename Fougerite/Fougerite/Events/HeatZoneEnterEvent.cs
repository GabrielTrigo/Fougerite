using UnityEngine;

namespace Fougerite.Events
{
    /// <summary>
    /// Triggered when a player is within the trigger of a HeatZone.
    /// It is called continually while the player remains in the zone.
    /// </summary>
    public class HeatZoneEnterEvent
    {
        private readonly HeatZone _instance;
        private readonly Collider _collider;
        private readonly Metabolism _metabolism;
        private readonly Player _player;
        private bool _cancel;

        /// <summary>
        /// Initializes a new instance of the HeatZoneEvent class.
        /// </summary>
        /// <param name="instance">The HeatZone component instance.</param>
        /// <param name="collider">The collider that entered the zone.</param>
        /// <param name="metabolism">The metabolism component found on the collider.</param>
        public HeatZoneEnterEvent(HeatZone instance, Collider collider, Metabolism metabolism)
        {
            _instance = instance;
            _collider = collider;
            _metabolism = metabolism;
            _cancel = false;
            
            if (metabolism != null)
            {
                var playerClient = metabolism.playerClient;
                if (playerClient != null)
                {
                    _player = Server.GetServer().FindPlayer(playerClient.userID);
                }
            }
        }

        /// <summary>
        /// Gets the HeatZone component that triggered this event.
        /// </summary>
        public HeatZone Instance 
        { 
            get { return _instance; } 
        }

        /// <summary>
        /// Gets the Collider that is currently staying in the heat zone.
        /// </summary>
        public Collider Collider 
        { 
            get { return _collider; } 
        }

        /// <summary>
        /// Gets the Metabolism component of the entity being warmed.
        /// </summary>
        public Metabolism Metabolism 
        { 
            get { return _metabolism; } 
        }

        /// <summary>
        /// Gets the Fougerite Player associated with the metabolism component.
        /// </summary>
        public Player Player 
        { 
            get { return _player; } 
        }

        /// <summary>
        /// Gets a value indicating whether the warming action has been cancelled.
        /// </summary>
        public bool Cancelled 
        { 
            get { return _cancel; } 
        }

        /// <summary>
        /// Cancels the warming action for this tick.
        /// </summary>
        public void Cancel() 
        { 
            _cancel = true; 
        }
    }
}