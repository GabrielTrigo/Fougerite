using Fougerite;
using RustPP.Permissions;
 
namespace RustPP.Commands
{
    public class HelpCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            var pl = Server.GetServer().GetCachePlayer(Arguments.argUser.userID);
            int i = 1;
            string setting = Core.config.GetSetting("Settings", $"help_string{i}");
            while (setting != null)
            {
                pl.MessageFrom(Core.Name, setting);
                i++;
                setting = Core.config.GetSetting("Settings", $"help_string{i}");
            }

            if (Administrator.IsAdmin(Arguments.argUser.userID))
            {
                i = 1;
                setting = Core.config.GetSetting("Settings", $"admin_help_string{i}");
                while (setting != null)
                {
                    pl.MessageFrom(Core.Name, setting);
                    i++;
                    setting = Core.config.GetSetting("Settings", $"admin_help_string{i}");
                }
            }
        }
    }
}