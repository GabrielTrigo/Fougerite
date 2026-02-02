using System.Diagnostics;
using Fougerite;
using Fougerite.Events;

namespace RustPP
{
    public class TimedEvents
    {
        public static bool init = false;
        public static int time = 60;
        public static TimedEvent timer;
        public static TimedEvent shutdownTimer;

        /// <summary>
        /// Fires the advertisement messages defined in the config.
        /// </summary>
        private static void advertise_begin(TimedEvent te)
        {
            int amount;
            if (int.TryParse(Core.config.GetSetting("Settings", "notice_messages_amount"), out amount))
            {
                for (int i = 0; i < amount; i++)
                {
                    Server.GetServer().BroadcastFrom(Core.Name, Core.config.GetSetting("Settings", $"notice{(i + 1)}"));
                }
            }
        }

        private static void airdrop_begin()
        {
            int num;
            if (int.TryParse(Core.config.GetSetting("Settings", "airdrop_count"), out num))
            {
                World.GetWorld().Airdrop(num);
            }
        }

        public static void savealldata()
        {
            World.GetWorld().ServerSaveHandler.ManualSave();
        }

        /// <summary>
        /// Initiates the server shutdown sequence using the Util timer system.
        /// </summary>
        public static void shutdown()
        {
            savealldata();
            if (int.TryParse(Core.config.GetSetting("Settings", "shutdown_countdown"), out time))
            {
                shutdownTimer = Util.GetUtil().CreateParallelTimer("RPP_ShutdownTimer", 10000, null, delegate (TimedEvent te)
                {
                    shutdown_tick(te);
                }, true, "RustPP");

                // Execute the first tick immediately
                shutdown_tick(null);
            }
        }

        public static void shutdown_tick(TimedEvent te)
        {
            if (time <= 0)
            {
                te?.Kill(); 
                Helper.CreateSaves();
                Server.GetServer().BroadcastFrom(Core.Name, "Server Shutdown NOW!");
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                Logger.Log($"Server Shutting down in {time} seconds");
                Server.GetServer().BroadcastFrom(Core.Name, $"Server Shutting down in {time} seconds");
            }
            time -= 10;
        }

        /// <summary>
        /// Starts the initial server events and advertisement timers.
        /// </summary>
        public static void startEvents()
        {
            if (!init)
            {
                init = true;
                
                server.pvp = Core.config.GetBoolSetting("Settings", "pvp");
                crafting.instant = Core.config.GetBoolSetting("Settings", "instant_craft");
                sleepers.on = Core.config.GetSetting("Settings", "sleepers") == "true";
                truth.punish = Core.config.GetBoolSetting("Settings", "enforce_truth");
                
                if (!Core.config.GetBoolSetting("Settings", "voice_proximity"))
                {
                    voice.distance = 2.147484E+09f;
                }

                if (Core.config.GetBoolSetting("Settings", "notice_enabled"))
                {
                    int interval;
                    if (int.TryParse(Core.config.GetSetting("Settings", "notice_interval"), out interval))
                    {
                        timer = Util.GetUtil().CreateTimer("RPP_Advertiser", interval, advertise_begin, true, "RustPP");
                    }
                }
            }
        }
    }
}