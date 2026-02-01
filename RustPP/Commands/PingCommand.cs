using Fougerite;

namespace RustPP.Commands
{
    public class PingCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            var pl = Server.GetServer().GetCachePlayer(Arguments.argUser.userID);
            pl.MessageFrom(Core.Name, "Ping: " + pl.Ping);
        }
    }
}