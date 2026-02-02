using System.Collections;
using System.Collections.Generic;
using Fougerite;
using RustPP.Commands;
using RustPP.Permissions;

namespace RustPP
{
    public class Core
    {
        public static string Name = "Rust++";
        public static string Version = "1.1.8.2";
        public static IniParser config;
        public static PList blackList = new PList();
        public static PList whiteList = new PList();
        public static PList muteList = new PList();
        public static List<ulong> tempConnect = new List<ulong>();
        public static Dictionary<ulong, string> userCache;
        public static Hashtable banWaitList = new Hashtable();
        public static Hashtable unbanWaitList = new Hashtable();
        public static Hashtable kickWaitList = new Hashtable();
        public static Hashtable killWaitList = new Hashtable();
        public static Hashtable whiteWaitList = new Hashtable();
        public static Hashtable adminFlagsWaitList = new Hashtable();
        public static Hashtable adminFlagWaitList = new Hashtable();
        public static Hashtable adminFlagsList = new Hashtable();
        public static Hashtable adminUnflagWaitList = new Hashtable();
        public static Hashtable adminAddWaitList = new Hashtable();
        public static Hashtable adminRemoveWaitList = new Hashtable();
        public static Hashtable muteWaitList = new Hashtable();
        public static Hashtable unmuteWaitList = new Hashtable();
        public static Hashtable friendWaitList = new Hashtable();
        public static Hashtable unfriendWaitList = new Hashtable();
        public static Hashtable shareWaitList = new Hashtable();
        public static Hashtable unshareWaitList = new Hashtable();

        public static void Init()
        {
            InitializeCommands();
            ShareCommand shareCmd = (ShareCommand) ChatCommand.GetCommand("share");
            FriendsCommand friendsCmd = (FriendsCommand) ChatCommand.GetCommand("friends");

            // Doors: JSON -> XML -> doorsSave.rpp
            var doors = Helper
                .LoadWithMigration<Dictionary<ulong, List<ulong>>, SerializableDictionary<ulong, List<ulong>>>(
                    "doorsSave.json", "doorsSave.xml", "doorsSave.rpp");
            if (doors != null)
            {
                Hashtable ht = new Hashtable();
                foreach (var kvp in doors) ht.Add(kvp.Key, new ArrayList(kvp.Value));
                shareCmd.SetSharedDoors(ht);
            }

            // Friends: JSON -> XML -> friendsSave.rpp
            var friends =
                Helper.LoadWithMigration<Hashtable, Hashtable>("friendsSave.json", "friendsSave.xml",
                    "friendsSave.rpp");
            if (friends != null) 
                friendsCmd.SetFriendsLists(friends);

            // Admins: JSON -> XML
            var admins =
                Helper.LoadWithMigration<List<Administrator>, List<Administrator>>("admins.json", "admins.xml");
            if (admins != null) 
                Administrator.AdminList = admins;

            var cache =
                Helper.LoadWithMigration<Dictionary<ulong, string>, SerializableDictionary<ulong, string>>(
                    "userCache.json", "userCache.xml", "cache.rpp");
            userCache = cache ?? new Dictionary<ulong, string>();

            // PLists: JSON -> XML
            whiteList = new PList(
                Helper.LoadWithMigration<List<PList.Player>, List<PList.Player>>("whitelist.json", "whitelist.xml") ??
                new List<PList.Player>());
            muteList = new PList(
                Helper.LoadWithMigration<List<PList.Player>, List<PList.Player>>("mutelist.json", "mutelist.xml") ??
                new List<PList.Player>());
            blackList = new PList(
                Helper.LoadWithMigration<List<PList.Player>, List<PList.Player>>("bans.json", "bans.xml") ??
                new List<PList.Player>());
        }

        public static void handleCommand(ref ConsoleSystem.Arg arg)
        {
            string displayname = arg.argUser.user.Displayname;
            string[] strArray = arg.GetString(0).Trim().Split(new char[] { ' ' });
            string cmd = strArray[0].Trim();
            string[] chatArgs = new string[strArray.Length - 1];
            for (int i = 1; i < strArray.Length; i++)
            {
                chatArgs[i - 1] = strArray[i];
            }
            ChatCommand.CallCommand(cmd, ref arg, ref chatArgs);
        }

        private static void InitializeCommands()
        {
            ChatCommand.AddCommand("/about", new AboutCommand());
            ChatCommand.AddCommand("/addfriend", new AddFriendCommand());
            AddAdminCommand command = new AddAdminCommand();
            command.AdminFlags = "CanAddAdmin";
            ChatCommand.AddCommand("/addadmin", command);
            AddFlagCommand command2 = new AddFlagCommand();
            command2.AdminFlags = "CanAddFlags";
            ChatCommand.AddCommand("/addflag", command2);
            AnnounceCommand command3 = new AnnounceCommand();
            command3.AdminFlags = "CanAnnounce";
            ChatCommand.AddCommand("/announce", command3);
            BanCommand command4 = new BanCommand();
            command4.AdminFlags = "CanBan";
            ChatCommand.AddCommand("/ban", command4);
            ChatCommand.AddCommand("/friends", new FriendsCommand());
            GetFlagsCommand command5 = new GetFlagsCommand();
            command5.AdminFlags = "CanGetFlags";
            ChatCommand.AddCommand("/getflags", command5);
            GiveItemCommand command6 = new GiveItemCommand();
            command6.AdminFlags = "CanGiveItem";
            ChatCommand.AddCommand("/give", command6);
            GodModeCommand command7 = new GodModeCommand();
            command7.AdminFlags = "CanGodMode";
            ChatCommand.AddCommand("/god", command7);
            ChatCommand.AddCommand("/help", new HelpCommand());
            ChatCommand.AddCommand("/history", new HistoryCommand());
            SpawnItemCommand command8 = new SpawnItemCommand();
            command8.AdminFlags = "CanSpawnItem";
            ChatCommand.AddCommand("/i", command8);
            InstaKOCommand command9 = new InstaKOCommand();
            command9.AdminFlags = "CanInstaKO";
            ChatCommand.AddCommand("/instako", command9);
            KickCommand command10 = new KickCommand();
            command10.AdminFlags = "CanKick";
            ChatCommand.AddCommand("/kick", command10);
            KillCommand command11 = new KillCommand();
            command11.AdminFlags = "CanKill";
            ChatCommand.AddCommand("/kill", command11);
            LoadoutCommand command12 = new LoadoutCommand();
            command12.AdminFlags = "CanLoadout";
            ChatCommand.AddCommand("/loadout", command12);
            ChatCommand.AddCommand("/motd", new MOTDCommand());
            MuteCommand command13 = new MuteCommand();
            command13.AdminFlags = "CanMute";
            ChatCommand.AddCommand("/mute", command13);
            ChatCommand.AddCommand("/location", new LocationCommand());
            ChatCommand.AddCommand("/ping", new PingCommand());
            ChatCommand.AddCommand("/players", new PlayersCommand());
            ChatCommand.AddCommand("/pm", new PrivateMessagesCommand());
            ReloadCommand command14 = new ReloadCommand();
            command14.AdminFlags = "CanReload";
            ChatCommand.AddCommand("/reload", command14);
            RemoveAdminCommand command15 = new RemoveAdminCommand();
            command15.AdminFlags = "CanDeleteAdmin";
            ChatCommand.AddCommand("/unadmin", command15);
            ChatCommand.AddCommand("/r", new ReplyCommand());
            ChatCommand.AddCommand("/rules", new RulesCommand());
            SaveAllCommand command16 = new SaveAllCommand();
            command16.AdminFlags = "CanSaveAll";
            ChatCommand.AddCommand("/saveall", command16);
            MasterAdminCommand command17 = new MasterAdminCommand();
            command17.AdminFlags = "RCON";
            ChatCommand.AddCommand("/setmasteradmin", command17);
            ChatCommand.AddCommand("/share", new ShareCommand());
            ChatCommand.AddCommand("/starter", new StarterCommand());
            TeleportHereCommand command18 = new TeleportHereCommand();
            command18.AdminFlags = "CanTeleport";
            ChatCommand.AddCommand("/tphere", command18);
            TeleportToCommand command19 = new TeleportToCommand();
            command19.AdminFlags = "CanTeleport";
            ChatCommand.AddCommand("/tpto", command19);
            UnbanCommand command20 = new UnbanCommand();
            command20.AdminFlags = "CanUnban";
            ChatCommand.AddCommand("/unban", command20);
            ChatCommand.AddCommand("/unfriend", new UnfriendCommand());
            RemoveFlagsCommand command21 = new RemoveFlagsCommand();
            command21.AdminFlags = "CanUnflag";
            ChatCommand.AddCommand("/unflag", command21);
            UnmuteCommand command22 = new UnmuteCommand();
            command22.AdminFlags = "CanUnmute";
            ChatCommand.AddCommand("/unmute", command22);
            ChatCommand.AddCommand("/unshare", new UnshareCommand());
            WhiteListAddCommand command23 = new WhiteListAddCommand();
            command23.AdminFlags = "CanWhiteList";
            ChatCommand.AddCommand("/addwl", command23);
            ShutDownCommand command24 = new ShutDownCommand();
            command24.AdminFlags = "CanShutdown";
            ChatCommand.AddCommand("/shutdown", command24);
            InstaKOAllCommand command25 = new InstaKOAllCommand();
            command25.AdminFlags = "CanInstaKOAll";
            ChatCommand.AddCommand("/instakoall", command25);
            ChatCommand.AddCommand("/sharelist", new ShareListCommand());
        }

        public static bool IsEnabled()
        {
            return config.GetBoolSetting("Settings", "rust++_enabled");
        }

        public static void motd(Fougerite.Player player)
        {
            if (config.GetBoolSetting("Settings", "motd"))
            {
                int num = 1;
                do
                {
                    string setting = config.GetSetting("Settings", $"motd{num}");
                    if (setting != null)
                    {
                        player.MessageFrom(Core.Name, setting);
                        num++;
                    }
                    else
                    {
                        num = 0;
                    }
                }
                while (num != 0);
            }
        }
    }
}