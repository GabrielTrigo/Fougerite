using Fougerite.Events;

namespace Fougerite
{
    /// <summary>
    /// This class is used by Fougerite to filter any flood connections to the server.
    /// </summary>
    public class Flood
    {
        private TimedEvent _te;
        private int _count = 1;
        private readonly string _ip;
            
        public Flood(string ip)
        {
            _ip = ip;
            _te = Util.GetUtil().CreateTimer($"Flood.{ip}", 3000, Check, false, $"{nameof(Fougerite)}.{nameof(Flood)}");
            _te.Start();
        }

        public void Increase()
        {
            _count = _count + 1;
        }

        public int Amount
        {
            get { return _count; }
        }

        public void Reset()
        {
            _te.Kill();
            _te = Util.GetUtil().CreateTimer($"Flood.{_ip}", 3000, Check, false, $"{nameof(Fougerite)}.{nameof(Flood)}");
            _te.Start();
        }

        public void Stop()
        {
            _te.Kill();
        }

        private void Check(TimedEvent evt)
        {
            evt.Kill();
            if (Hooks.FloodChecks.ContainsKey(_ip))
            {
                Hooks.FloodChecks.Remove(_ip);
            }
        }
    }
}