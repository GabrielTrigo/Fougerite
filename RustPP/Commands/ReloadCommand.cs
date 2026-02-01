using System.Collections.Generic;
using Fougerite;
using RustPP.Permissions;

namespace RustPP.Commands
{
    public class ReloadCommand : ChatCommand
    {
        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            var sender = Server.GetServer().GetCachePlayer(Arguments.argUser.userID);
            sender.MessageFrom(Core.Name, "Reloading...");

            // Re-initialize timed events/config settings
            TimedEvents.startEvents();

            // Reload Admins json -> xml
            var admins = Helper.LoadWithMigration<List<Administrator>, List<Administrator>>("admins.json", "admins.xml");
            if (admins != null) 
                Administrator.AdminList = admins;

            // Reload Whitelist json -> whitelist.xml
            var wl = Helper.LoadWithMigration<List<PList.Player>, List<PList.Player>>("whitelist.json", "whitelist.xml");
            Core.whiteList = new PList(wl ?? new List<PList.Player>());

            // Reload Bans (Blacklist) json -> bans.xml
            var bl = Helper.LoadWithMigration<List<PList.Player>, List<PList.Player>>("bans.json", "bans.xml");
            Core.blackList = new PList(bl ?? new List<PList.Player>());

            sender.MessageFrom(Core.Name, "Reloaded!");
        }
    }
}