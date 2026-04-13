# Guide — Python Plugins (IronPython)

Python plugins use the **IronPython** runtime (Python 2.7 on .NET). They are well suited for game logic plugins with first-class support for CLR types.

---

## Directory Structure

```
/Plugins/
└── MyPlugin/
    └── MyPlugin.py    ← filename = class name = plugin name
```

---

## Minimal Structure

```python
__author__  = "YourName"
__version__ = "1.0"
__about__   = "Plugin description"

class MyPlugin:

    def On_PluginInit(self):
        """Called when the plugin is loaded."""
        Plugin.Log("system", "MyPlugin started!")

    def On_PluginShutdown(self):
        """Called when the plugin is unloaded."""
        Plugin.Log("system", "MyPlugin stopped!")
```

> **Required:** The class name MUST match the filename.

---

## Hooks

Implement any hook as a method of your class:

```python
class MyPlugin:

    def On_PlayerConnected(self, player):
        player.Message("Welcome, " + player.Name + "!")
        Server.Broadcast(player.Name + " joined!")

    def On_PlayerDisconnected(self, player):
        Server.Broadcast(player.Name + " left.")

    def On_Chat(self, player, chat):
        Plugin.Log("chat", player.Name + ": " + chat.Message)

    def On_Command(self, player, cmd, args):
        if cmd == "tpa":
            if len(args) < 1:
                player.Message("Usage: /tpa <PlayerName>")
                return
            target = Server.FindPlayer(args[0])
            if target is None:
                player.Message("Player not found!")
            else:
                player.TeleportTo(target)
                player.Message("Teleported to " + target.Name)

    def On_PlayerHurt(self, evt):
        if evt.AttackerPlayer is not None:
            Plugin.Log("pvp", evt.AttackerPlayer.Name + " hit " +
                       evt.VictimPlayer.Name + " for " + str(evt.DamageAmount))
```

---

## Accessing CLR Types

With IronPython, you can use .NET types directly:

```python
import clr
clr.AddReference("UnityEngine")
from UnityEngine import Vector3, Color, Quaternion

# Create a Unity position
pos = Vector3(100, 50, 200)
player.TeleportTo(pos.x, pos.y, pos.z)

# Use standard .NET types
from System.Collections.Generic import Dictionary, List
from System import String, Int32
```

---

## Timers

```python
class MyPlugin:

    def On_PluginInit(self):
        # Timer that fires every minute
        Plugin.CreateTimer("Announcement", 60000, True)

    def AnnouncementCallback(self, evt):
        Server.Broadcast("Server online for " + str(int(evt.ElapsedCount)) + " minute(s)!")

    def On_PluginShutdown(self):
        Plugin.KillTimers()
```

---

## Configuration and Data

```python
class MyPlugin:

    def On_PluginInit(self):
        self.config = Plugin.CreateIni("config")
        if self.config.GetSetting("General", "Message") is None:
            self.config.AddSetting("General", "Message", "Welcome!")
            self.config.Save()

    def On_PlayerConnected(self, player):
        msg = self.config.GetSetting("General", "Message")
        player.Message(msg)

        uid = str(player.UID)
        visits = DataStore.Get("my_plugin_visits", uid)
        if visits is None:
            visits = 1
        else:
            visits = int(visits) + 1
        DataStore.Add("my_plugin_visits", uid, visits)
```

---

## HTTP Requests

```python
import json

class MyPlugin:

    def On_Command(self, player, cmd, args):
        if cmd == "weather":
            # Synchronous GET request
            resp = Web.GetURL("https://wttr.in/?format=3")
            player.Message("Weather: " + str(resp))

    def notify_webhook(self, message):
        payload = json.dumps({"content": message})
        Web.PostURL("https://discord.com/api/webhooks/...", payload)
```

---

## IronPython Limitations

| Limitation | Description |
|---|---|
| **Python 2.7** | Uses Python 2 syntax and standard library, not Python 3 |
| **`print` statement** | Use `print "msg"` or `print("msg")` (both work in 2.7) |
| **No `asyncio`** | Use Fougerite's timer system for async-style operations |
| **No native extensions** | `.pyd` and C extension modules do not work; use pure-Python or .NET assemblies |
| **Library path** | Additional libraries go in `/Save/Lib/` |

---

## Adding Python Libraries

Place pure-Python `.py` files in `/Save/Lib/`:

```
/Save/
└── Lib/
    ├── my_helpers.py
    └── compatibility.py
```

```python
# Import normally in your plugin
import my_helpers
```
