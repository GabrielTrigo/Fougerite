using UnityEngine;

namespace Fougerite.Events
{
    /// <summary>
    /// Triggered when a player is within the trigger of a WorkZone.
    /// It is called continually while the player remains in the zone.
    /// </summary>
    public class WorkZoneEnterEvent
    {
        private readonly WorkZone _instance;
        private readonly Collider _collider;
        private readonly CraftingInventory _craftingInventory;
        private readonly Player _player;
        private bool _cancel;

        /// <summary>
        /// Initializes a new instance of the WorkZoneEvent class.
        /// </summary>
        /// <param name="instance">The WorkZone component instance.</param>
        /// <param name="collider">The collider that entered the zone.</param>
        /// <param name="craftingInv">The crafting inventory component found on the collider.</param>
        public WorkZoneEnterEvent(WorkZone instance, Collider collider, CraftingInventory craftingInv)
        {
            _instance = instance;
            _collider = collider;
            _craftingInventory = craftingInv;
            _cancel = false;

            var character = craftingInv.GetComponent<Character>();
            if (character != null)
            {
                var netUser = character.netUser;
                _player = Server.GetServer().FindPlayer(netUser.userID);
            }
        }

        /// <summary>
        /// Gets the WorkZone component that triggered this event.
        /// </summary>
        public WorkZone Instance 
        { 
            get { return _instance; } 
        }

        /// <summary>
        /// Gets the Collider that is currently staying in the work zone.
        /// </summary>
        public Collider Collider 
        { 
            get { return _collider; } 
        }

        /// <summary>
        /// Gets the CraftingInventory component of the entity near the bench.
        /// </summary>
        public CraftingInventory CraftingInventory 
        { 
            get { return _craftingInventory; } 
        }

        /// <summary>
        /// Gets the Fougerite Player associated with the crafting inventory.
        /// </summary>
        public Player Player 
        { 
            get { return _player; } 
        }

        /// <summary>
        /// Gets a value indicating whether the workbench status has been cancelled.
        /// </summary>
        public bool Cancelled 
        { 
            get { return _cancel; } 
        }

        /// <summary>
        /// Cancels the workbench buff for this tick, preventing specialized crafting.
        /// </summary>
        public void Cancel() 
        { 
            _cancel = true; 
        }
    }
}