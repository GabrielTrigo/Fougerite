namespace Fougerite.Events
{
    public enum PreviousDayCycle
    {
        Day = 0,
        Night = 1,
        Unknown = 2
    }
    
    /// <summary>
    /// Runs when the day cycle changes from day to night or night to day.
    /// </summary>
    public class DayCycleChangeEvent
    {
        private readonly EnvironmentControlCenter _ecc;
        private readonly PreviousDayCycle _previousDayCycle;
        private readonly bool _isNight;
        
        public DayCycleChangeEvent(EnvironmentControlCenter ecc, bool? wasNight)
        {
            _ecc = ecc;
            if (!wasNight.HasValue)
            {
                _previousDayCycle = PreviousDayCycle.Unknown;
            }
            else if (wasNight.Value)
            {
                _previousDayCycle = PreviousDayCycle.Night;
            }
            else
            {
                _previousDayCycle = PreviousDayCycle.Day;
            }
            
            _isNight = ecc.IsNight();
        }
        
        /// <summary>
        /// Returns the EnvironmentControlCenter associated with this event.
        /// </summary>
        public EnvironmentControlCenter EnvironmentControlCenter
        {
            get { return _ecc; }
        }
        
        /// <summary>
        /// Returns true if it is currently night, false if it is day at the time of the event.
        /// </summary>
        public bool IsNight
        {
            get { return _isNight; }
        }
        
        /// <summary>
        /// Returns the previous day cycle (Day, Night, or Unknown).
        /// Unknown indicates that the previous state could not be determined, because the server just started.
        /// </summary>
        public PreviousDayCycle PreviousDayCycle
        {
            get { return _previousDayCycle; }
        }
    }
}