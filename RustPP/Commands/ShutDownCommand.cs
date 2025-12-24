using System.Diagnostics;
using Fougerite;
using Fougerite.Events;

namespace RustPP.Commands
{
    public class ShutDownCommand : ChatCommand
    {
        internal static TimedEvent _timer;
        internal static TimedEvent _timer2;
        public static int ShutdownTime = 60;
        public static int TriggerTime = 10;
        internal static int Time = 0;

        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            var pl = Server.GetServer().GetCachePlayer(Arguments.argUser.userID);
            if ((_timer != null && _timer.IsRunning) || (_timer2 != null && _timer2.IsRunning))
            {
                pl.MessageFrom(Core.Name, "Shutdown timer(s) already running!");
                return;
            }
            
            if (ChatArguments.Length == 1)
            {
                if (ChatArguments[0] == "urgent")
                {
                    Fougerite.Hooks.IsShuttingDown = true;
                    Fougerite.Server.GetServer().BroadcastFrom(Core.Name, "Server Shutdown NOW!");
                    //UnityEngine.Application.Quit();
                    Process.GetCurrentProcess().Kill();
                }
                else if (ChatArguments[0] == "safeurgent")
                {
                    Fougerite.Hooks.IsShuttingDown = true;
                    Fougerite.Server.GetServer().BroadcastFrom(Core.Name, "Saving Server...");
                    World.GetWorld().ServerSaveHandler.ManualSave();
                    Fougerite.Server.GetServer().BroadcastFrom(Core.Name, "Saved Server Data!");
                    Fougerite.Server.GetServer().BroadcastFrom(Core.Name,
                        "Server is shutting down in " + ShutdownTime + " seconds.");
                    _timer = Util.GetUtil().CreateTimer("Shutdown.Trigger", TriggerTime * 1000, Trigger, true, "RustPP");
                    _timer.Start();
                }

                return;
            }

            StartShutdown();
        }

        public static void StartShutdown()
        {
            try
            {
                ShutdownTime = int.Parse(Core.config.GetSetting("Settings", "shutdown_countdown"));
                TriggerTime = int.Parse(Core.config.GetSetting("Settings", "shutdown_trigger"));
            }
            catch
            {
                Logger.LogError("[RustPP] Failed to execute shutdown! Invalid config options!");
                return;
            }

            Fougerite.Hooks.IsShuttingDown = true;
            Fougerite.Server.GetServer()
                .BroadcastFrom(Core.Name, "Server is shutting down in " + ShutdownTime + " seconds.");
            _timer = Util.GetUtil().CreateTimer("Shutdown.Trigger", TriggerTime * 1000, Trigger, true, "RustPP");
            _timer.Start();
        }

        internal static void Trigger(TimedEvent evt)
        {
            Time += TriggerTime;
            if (Time >= ShutdownTime)
            {
                evt.Kill();
                Fougerite.Server.GetServer().BroadcastFrom(Core.Name, "Saving Server...");
                World.GetWorld().ServerSaveHandler.ManualSave();
                Fougerite.Server.GetServer().BroadcastFrom(Core.Name, "Saved Server Data!");
                Fougerite.Server.GetServer().BroadcastFrom(Core.Name, "Server shutdown in 15 seconds!");
                _timer2 = Util.GetUtil().CreateTimer("Shutdown.Final", 15000, Trigger2, false, "RustPP");
                _timer2.Start();
            }
            else
            {
                Fougerite.Server.GetServer().BroadcastFrom(Core.Name,
                    "Server is shutting down in " + (ShutdownTime - Time) + " seconds.");
            }
        }

        internal static void Trigger2(TimedEvent evt)
        {
            evt.Kill();
            Fougerite.Server.GetServer().BroadcastFrom(Core.Name, "Server Shutdown NOW!");
            //Loom.QueueOnMainThread(UnityEngine.Application.Quit);
            //UnityEngine.Application.Quit();
            Process.GetCurrentProcess().Kill();
        }
    }
}