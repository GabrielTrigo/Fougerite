# API Reference — Web (HTTP Requests)

**Singleton:** `Web.GetInstance()`  
**Global in scripts:** `Web`

Provides HTTP/HTTPS request capabilities for plugins. Always use `CreateAsyncHTTPRequest` — the synchronous methods (`GET`, `POST`) block the Unity main thread and will freeze the server until the request completes.

> **TLS note:** This version of Mono supports up to TLS 1.1. TLS 1.2 is **not** supported. Use HTTP endpoints or APIs that accept TLS 1.0/1.1 when possible.

---

## Async Requests (Recommended)

`CreateAsyncHTTPRequest` is non-blocking. The callback runs on a **background thread**.

> **Thread safety:** If you need to call Unity API or Fougerite methods (e.g., `player.Message`, `Server.Broadcast`) inside the callback, wrap them in `Loom.QueueOnMainThread` to avoid crashes.

### Signature

```python
Web.CreateAsyncHTTPRequest(
    url,              # Full URL (http:// or https://)
    callback,         # Action[int, str] — called with (statusCode, responseBody)
    method,           # "GET", "POST", "PUT", "DELETE", etc.  (default: "GET")
    inputBody,        # Request body string or None
    additionalHeaders, # Dict[str, str] of custom headers or None
    contentType,      # Content-Type header (default: "application/x-www-form-urlencoded")
    timeout,          # Timeout in seconds; 0 = default  (default: 0)
    allowDecompression # Enable GZip/Deflate decompression for HTTP  (default: False)
)
```

---

## GET Request

```python
import clr
import System
from System import Action

def On_PluginInit():
    Web.CreateAsyncHTTPRequest(
        'http://api.example.com/status',
        Action[int, str](self.OnWebResponse),
        'GET'
    )

def OnWebResponse(self, statusCode, body):
    # This runs on a background thread!
    Loom.QueueOnMainThread(lambda: self.HandleResponse(statusCode, body))

def HandleResponse(self, statusCode, body):
    # Safe to call Fougerite API here (main thread)
    if statusCode == 200:
        Server.Broadcast("Server status: " + body)
    else:
        Server.Broadcast("Request failed: " + str(statusCode))
```

---

## POST Request (JSON)

```python
import clr
import json
import System
from System import Action

def On_Command(player, cmd, args):
    if cmd == "register":
        payload = json.dumps({"uid": str(player.UID), "name": player.Name})

        Web.CreateAsyncHTTPRequest(
            'http://api.example.com/register',
            Action[int, str](handle_register),
            'POST',
            payload,
            None,
            'application/json'
        )

def handle_register(statusCode, body):
    Loom.QueueOnMainThread(lambda: after_register(statusCode, body))

def after_register(statusCode, body):
    if statusCode == 200:
        Server.Broadcast("A new player registered!")
```

---

## POST with Custom Headers

```python
import clr
import System
from System import Action

def send_webhook(message):
    headers = Plugin.CreateStringDict()
    headers["Authorization"] = "Bearer my-secret-token"
    headers["X-Plugin-Name"]  = Plugin.Name

    Web.CreateAsyncHTTPRequest(
        'http://hooks.example.com/notify',
        Action[int, str](on_webhook_response),
        'POST',
        'message=' + message,
        headers,
        'application/x-www-form-urlencoded'
    )

def on_webhook_response(statusCode, body):
    if statusCode != 200:
        Plugin.Log("webhook", "Webhook failed: " + str(statusCode) + " " + body)
```

---

## Common Patterns

### Store result in DataStore after web call

```python
def fetch_player_rank(player):
    uid = str(player.UID)
    Web.CreateAsyncHTTPRequest(
        'http://api.example.com/rank?uid=' + uid,
        Action[int, str](lambda code, body: on_rank(code, body, uid)),
        'GET'
    )

def on_rank(statusCode, body, uid):
    if statusCode == 200:
        Loom.QueueOnMainThread(lambda: DataStore.Add("ranks", uid, body.strip()))
```

### Retry on failure

```python
_retry_count = {}

def fetch_with_retry(uid, attempt=0):
    if attempt >= 3:
        Plugin.Log("errors", "Max retries reached for uid: " + str(uid))
        return

    Web.CreateAsyncHTTPRequest(
        'http://api.example.com/player?uid=' + str(uid),
        Action[int, str](lambda code, body: handle_fetch(code, body, uid, attempt)),
        'GET'
    )

def handle_fetch(statusCode, body, uid, attempt):
    if statusCode != 200:
        Loom.QueueOnMainThread(lambda: fetch_with_retry(uid, attempt + 1))
        return
    Loom.QueueOnMainThread(lambda: DataStore.Add("players", str(uid), body))
```

---

## Notes

| Topic | Detail |
|---|---|
| HTTPS routing | URLs starting with `https://` are routed through `WinHttpClient`; `http://` uses `HttpWebRequest` |
| TLS support | Max TLS 1.1. TLS 1.2 will fail silently or throw an exception |
| Thread safety | Callbacks run on a background thread — use `Loom.QueueOnMainThread` before touching Fougerite/Unity APIs |
| Decompression | `allowDecompression=True` only works for HTTP (not HTTPS/WinHTTP) |
| Deprecated methods | `Web.GET()` and `Web.POST()` are marked `[Obsolete]` — they block the main thread and should not be used |
