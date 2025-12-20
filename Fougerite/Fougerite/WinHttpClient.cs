using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace Fougerite
{
    /// <summary>
    /// Since UnityEngine is running on Mono and updating the security dlls would take a lot of work
    /// we use WinHTTP via P/Invoke to make HTTP requests with modern TLS support so websites do not reject us
    /// with TLS 1.2 enforcement.
    /// It should work with Wine under Linux as well.
    /// </summary>
    public class WinHttpClient
    {
        [DllImport("winhttp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr WinHttpOpen(string pszAgentW, uint dwAccessType, string pszProxyW,
            string pszProxyBypassW, uint dwFlags);

        [DllImport("winhttp.dll", SetLastError = true)]
        public static extern bool WinHttpCloseHandle(IntPtr hInternet);

        [DllImport("winhttp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr WinHttpConnect(IntPtr hSession, string pszServerName, ushort nServerPort,
            uint dwReserved);

        [DllImport("winhttp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr WinHttpOpenRequest(IntPtr hConnect, string pszVerb, string pszObjectName,
            string pszVersion, string pszReferrer, IntPtr ppwszAcceptTypes, uint dwFlags);

        [DllImport("winhttp.dll", SetLastError = true)]
        public static extern bool WinHttpSetOption(IntPtr hRequest, uint dwOption, IntPtr lpBuffer,
            uint dwBufferLength);

        [DllImport("winhttp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool WinHttpSendRequest(IntPtr hRequest, string lpszHeaders, uint dwHeadersLength,
            IntPtr lpOptional, uint dwOptionalLength, uint dwTotalLength, IntPtr lpContext);

        [DllImport("winhttp.dll", SetLastError = true)]
        public static extern bool WinHttpReceiveResponse(IntPtr hRequest, IntPtr lpReserved);

        [DllImport("winhttp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool WinHttpQueryHeaders(IntPtr hRequest, uint dwInfoLevel, string pwszName,
            IntPtr lpBuffer, ref uint lpdwBufferLength, ref uint lpdwIndex);

        [DllImport("winhttp.dll", SetLastError = true)]
        public static extern bool WinHttpReadData(IntPtr hRequest, byte[] lpBuffer, uint dwNumberOfBytesToRead,
            out uint lpdwNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateEventW(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState,
            string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        
        public const uint WINHTTP_ACCESS_TYPE_DEFAULT_PROXY = 0;
        public const uint WINHTTP_FLAG_SECURE = 0x00800000;
        public const ushort INTERNET_DEFAULT_HTTPS_PORT = 443;
        public const ushort INTERNET_DEFAULT_HTTP_PORT = 80;
        public const uint WINHTTP_OPTION_SECURITY_FLAGS = 31;
        public const uint WINHTTP_OPTION_CONNECT_TIMEOUT = 2;
        public const uint WINHTTP_OPTION_SEND_TIMEOUT = 3;
        public const uint WINHTTP_OPTION_RECEIVE_TIMEOUT = 4;
        public const uint SECURITY_FLAG_IGNORE_UNKNOWN_CA = 0x00000100;
        public const uint SECURITY_FLAG_IGNORE_CERT_WRONG_USAGE = 0x00000200;
        public const uint SECURITY_FLAG_IGNORE_CERT_CN_INVALID = 0x00001000;
        public const uint SECURITY_FLAG_IGNORE_CERT_DATE_INVALID = 0x00002000;
        public const uint WINHTTP_QUERY_STATUS_CODE = 19;
        public const uint WINHTTP_QUERY_FLAG_NUMBER = 0x20000000;
        public const int BUFFER_SIZE = 4096;
        public const uint WAIT_OBJECT_0 = 0;
        public const uint WAIT_TIMEOUT = 0x102;

        /// <summary>
        /// Session handle for WinHTTP.
        /// No need to access this from a plugin.
        /// </summary>
        public static IntPtr SessionHandle = IntPtr.Zero;
        
        /// <summary>
        /// Session lock for thread safety.
        /// No need to access this from a plugin.
        /// </summary>
        public static readonly object SessionLock = new object();

        /// <summary>
        /// Initializes the WinHTTP session.
        /// No need to call this from a plugin.
        /// </summary>
        public static void InitSession()
        {
            lock (SessionLock)
            {
                if (SessionHandle == IntPtr.Zero)
                {
                    SessionHandle = WinHttpOpen("Fougerite Mod", WINHTTP_ACCESS_TYPE_DEFAULT_PROXY, null, null, 0);
                }
            }
        }

        /// <summary>
        /// Closes the WinHTTP session.
        /// No need to call this from a plugin.
        /// </summary>
        public static void CloseSession()
        {
            lock (SessionLock)
            {
                if (SessionHandle != IntPtr.Zero)
                {
                    WinHttpCloseHandle(SessionHandle);
                    SessionHandle = IntPtr.Zero;
                }
            }
        }

        public static void MakeRequest(string url, Action<int, string> callback, string method = "GET",
            string inputBody = null, Dictionary<string, string> additionalHeaders = null,
            string contentType = "application/x-www-form-urlencoded", float timeout = 0f)
        {
            ThreadPool.QueueUserWorkItem(_ =>
                DoWinHttpRequest(url, callback, method, inputBody, additionalHeaders, contentType, timeout));
        }

        public static void DoWinHttpRequest(string url, Action<int, string> callback, string method, string inputBody,
            Dictionary<string, string> additionalHeaders, string contentType, float timeout)
        {
            IntPtr connectHandle = IntPtr.Zero;
            IntPtr requestHandle = IntPtr.Zero;
            GCHandle bodyPin = new GCHandle();
            GCHandle flagsPin = new GCHandle();

            try
            {
                InitSession();

                if (SessionHandle == IntPtr.Zero)
                {
                    callback(0, "NoSession");
                    return;
                }

                Uri uri;
                try
                {
                    uri = new Uri(url);
                }
                catch (Exception ex)
                {
                    callback(0, "BadUrl" + ex.Message);
                    return;
                }

                ushort port = uri.Scheme == "https" ? INTERNET_DEFAULT_HTTPS_PORT : INTERNET_DEFAULT_HTTP_PORT;
                uint flags = uri.Scheme == "https" ? WINHTTP_FLAG_SECURE : 0;

                connectHandle = WinHttpConnect(SessionHandle, uri.Host, port, 0);
                if (connectHandle == IntPtr.Zero)
                {
                    callback(0, "ConnectFail");
                    return;
                }

                string path = uri.PathAndQuery;
                if (!string.IsNullOrEmpty(uri.Fragment)) path += uri.Fragment;

                requestHandle = WinHttpOpenRequest(connectHandle, method, path, null, null, IntPtr.Zero, flags);
                if (requestHandle == IntPtr.Zero)
                {
                    callback(0, "RequestFail");
                    return;
                }

                uint timeoutMs = timeout > 0 ? (uint)(timeout * 1000) : 30000;

                byte[] connectTimeoutBuf = BitConverter.GetBytes(timeoutMs);
                GCHandle connectTimeoutPin = GCHandle.Alloc(connectTimeoutBuf, GCHandleType.Pinned);
                WinHttpSetOption(requestHandle, WINHTTP_OPTION_CONNECT_TIMEOUT, connectTimeoutPin.AddrOfPinnedObject(),
                    sizeof(uint));
                connectTimeoutPin.Free();

                byte[] sendTimeoutBuf = BitConverter.GetBytes(timeoutMs);
                GCHandle sendTimeoutPin = GCHandle.Alloc(sendTimeoutBuf, GCHandleType.Pinned);
                WinHttpSetOption(requestHandle, WINHTTP_OPTION_SEND_TIMEOUT, sendTimeoutPin.AddrOfPinnedObject(),
                    sizeof(uint));
                sendTimeoutPin.Free();

                byte[] recvTimeoutBuf = BitConverter.GetBytes(timeoutMs);
                GCHandle recvTimeoutPin = GCHandle.Alloc(recvTimeoutBuf, GCHandleType.Pinned);
                WinHttpSetOption(requestHandle, WINHTTP_OPTION_RECEIVE_TIMEOUT, recvTimeoutPin.AddrOfPinnedObject(),
                    sizeof(uint));
                recvTimeoutPin.Free();

                uint ignoreFlags = SECURITY_FLAG_IGNORE_UNKNOWN_CA | SECURITY_FLAG_IGNORE_CERT_WRONG_USAGE |
                                   SECURITY_FLAG_IGNORE_CERT_CN_INVALID | SECURITY_FLAG_IGNORE_CERT_DATE_INVALID;

                byte[] flagsBuffer = BitConverter.GetBytes(ignoreFlags);
                flagsPin = GCHandle.Alloc(flagsBuffer, GCHandleType.Pinned);
                IntPtr flagsPtr = flagsPin.AddrOfPinnedObject();

                bool sslSet = WinHttpSetOption(requestHandle, WINHTTP_OPTION_SECURITY_FLAGS, flagsPtr, sizeof(uint));

                byte[] bodyBytes = string.IsNullOrEmpty(inputBody) ? new byte[0] : Encoding.UTF8.GetBytes(inputBody);
                uint bodyLength = (uint)bodyBytes.Length;

                IntPtr bodyPtr = IntPtr.Zero;
                if (bodyLength > 0)
                {
                    bodyPin = GCHandle.Alloc(bodyBytes, GCHandleType.Pinned);
                    bodyPtr = bodyPin.AddrOfPinnedObject();
                }

                uint totalLength = bodyLength;

                bool sent = WinHttpSendRequest(requestHandle, null, 0, bodyPtr, bodyLength, totalLength, IntPtr.Zero);

                if (!sent)
                {
                    callback(0, "SendFail");
                    return;
                }

                bool received = WinHttpReceiveResponse(requestHandle, IntPtr.Zero);

                if (!received)
                {
                    callback(0, "RecvFail");
                    return;
                }

                uint statusCode = 0;
                uint bufLen = sizeof(uint);
                uint index = 0;

                GCHandle statusCodePin = GCHandle.Alloc(statusCode, GCHandleType.Pinned);
                bool queryResult = WinHttpQueryHeaders(requestHandle,
                    WINHTTP_QUERY_STATUS_CODE | WINHTTP_QUERY_FLAG_NUMBER, null, statusCodePin.AddrOfPinnedObject(),
                    ref bufLen, ref index);
                statusCode = (uint)Marshal.ReadInt32(statusCodePin.AddrOfPinnedObject());
                statusCodePin.Free();

                if (!queryResult || statusCode == 0)
                {
                    callback(0, "NoStatus");
                    return;
                }

                StringBuilder responseBuilder = new StringBuilder();
                byte[] buffer = new byte[BUFFER_SIZE];
                uint bytesRead;
                int totalRead = 0;
                const int MAX_SIZE = 10 * 1024 * 1024;

                while (WinHttpReadData(requestHandle, buffer, (uint)BUFFER_SIZE, out bytesRead) && bytesRead > 0)
                {
                    totalRead += (int)bytesRead;
                    responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, (int)bytesRead));

                    if (totalRead > MAX_SIZE)
                    {
                        break;
                    }
                }

                callback((int)statusCode, responseBuilder.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogError("[WinHttp] EXCEPTION: " + ex);
                callback(0, "Exception: " + ex.Message);
            }
            finally
            {
                if (bodyPin.IsAllocated) bodyPin.Free();
                if (flagsPin.IsAllocated) flagsPin.Free();
                if (requestHandle != IntPtr.Zero) WinHttpCloseHandle(requestHandle);
                if (connectHandle != IntPtr.Zero) WinHttpCloseHandle(connectHandle);
            }
        }


        public static string GetBlocking(string url, float timeout = 10f)
        {
            try
            {
                var result = new ManualResetEvent(false);
                string responseBody = null;
                int responseCode = 0;

                MakeRequest(url, (code, body) =>
                {
                    responseCode = code;
                    responseBody = body;
                    result.Set();
                }, "GET", null, null, "text/plain", timeout);

                if (result.WaitOne((int)(timeout * 1000) + 5000, false))
                    return responseCode >= 200 && responseCode < 300 ? responseBody : "HTTP " + responseCode;
                return "Timeout";
            }
            catch (Exception ex)
            {
                Logger.LogError("[GetBlocking] " + ex);
                return "Error " + ex.Message;
            }
        }

        public static string PostBlocking(string url, string inputBody,
            string contentType = "application/x-www-form-urlencoded", float timeout = 10f)
        {
            try
            {
                var result = new ManualResetEvent(false);
                string responseBody = null;
                int responseCode = 0;

                MakeRequest(url, (code, body) =>
                {
                    responseCode = code;
                    responseBody = body;
                    result.Set();
                }, "POST", inputBody, null, contentType, timeout);

                if (result.WaitOne((int)(timeout * 1000) + 5000, false))
                    return responseCode >= 200 && responseCode < 300 ? responseBody : "HTTP " + responseCode;
                return "Timeout";
            }
            catch (Exception ex)
            {
                Logger.LogError("[PostBlocking] " + ex);
                return "Error " + ex.Message;
            }
        }
    }
}