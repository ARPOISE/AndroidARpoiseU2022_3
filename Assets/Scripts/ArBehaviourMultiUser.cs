/*
ArBehaviourMultiUser.cs - MonoBehaviour for ARpoise multi-user handling.

Copyright (C) 2020, Tamiko Thiel and Peter Graf - All Rights Reserved

ARpoise - Augmented Reality point of interest service environment 

This file is part of ARpoise.

    ARpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ARpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ARpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
ARpoise, see www.ARpoise.com/

*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public interface IRemoteCallback
    {
        public void Set(string tag, string value, DateTime startDateTime, DateTime now);
    }

    public class ArBehaviourMultiUser : MonoBehaviour
    {
#if UNITY_IOS
        public const string OperatingSystem = "iOS";
#else
        public const string OperatingSystem = "Android";
#endif
        public const string Bundle = "20240915";
        public const string ArvosApplicationName = "Arvos";
        public const string ArpoiseApplicationName = "Arpoise";
#if AndroidArvosU2022_3 || iOsArvosU2022_3
        protected readonly string ApplicationName = ArvosApplicationName;
#else
        protected readonly string ApplicationName = ArpoiseApplicationName;
#endif
        public GameObject ArCamera = null;
        public const string AnimationTag = "Animation";
        public const string LockTag = "Lock";
        public const string VeraPlasticTag = "VePl";

        protected float FilteredLongitude = 0;
        protected float FilteredLatitude = 0;
        protected float? FixedDeviceLatitude = null;
        protected float? FixedDeviceLongitude = null;
        protected float UsedLatitude => FixedDeviceLatitude.HasValue ? FixedDeviceLatitude.Value : FilteredLatitude;
        protected float UsedLongitude => FixedDeviceLongitude.HasValue ? FixedDeviceLongitude.Value : FilteredLongitude;
        protected long StartTicks = 0;

        public ArObjectState ArObjectState { get; protected set; }

        private long _nowTicks;
        protected long NowTicks
        {
            get
            {
                return _nowTicks;
            }
            set
            {
                _nowTicks = value;
                CurrentSecond = _nowTicks / 10000000L;
            }
        }

        protected DateTime ConnectionStart { get; private set; }

        protected long CurrentSecond { get; private set; }

        protected virtual void Start()
        {
        }

        private byte[] _readBuffer = new byte[32 * 1024];
        private int _nRead = 0;

        private void Receive(ref TcpClient tcpClient, ref NetworkStream netStream)
        {
            var socket = tcpClient?.Client;
            if (socket == null || netStream == null)
            {
                return;
            }

            for (; ; )
            {
                int length = 0;
                if (_nRead >= 2)
                {
                    length = _readBuffer[0] * 0x100 + _readBuffer[1];
                    if (length < 8)
                    {
                        netStream?.Close();
                        netStream = null;
                        tcpClient?.Close();
                        tcpClient = null;
                        return;
                    }
                }

                if (length > 0 && length + 2 <= _nRead)
                {
                    byte[] bytes = new byte[length - 8];
                    Array.Copy(_readBuffer, 10, bytes, 0, bytes.Length);
                    var message = Encoding.ASCII.GetString(bytes);
                    AddMessage(message);

                    if (_nRead > length + 2)
                    {
                        byte[] newBuffer = new byte[32 * 1024];
                        Array.Copy(_readBuffer, length + 2, newBuffer, 0, _nRead - length + 2);
                        _nRead -= length + 2;
                        _readBuffer = newBuffer;
                    }
                    else
                    {
                        _nRead = 0;
                    }
                    continue;
                }

                List<Socket> readList = new List<Socket>();
                List<Socket> errorList = new List<Socket>();
                readList.Add(socket);
                errorList.Add(socket);

                try
                {
                    Socket.Select(readList, null, errorList, 1);
                }
                catch (Exception)
                {
                    netStream?.Close();
                    netStream = null;
                    tcpClient?.Close();
                    tcpClient = null;
                    return;
                }
                if (errorList.Count > 0)
                {
                    netStream?.Close();
                    netStream = null;
                    tcpClient?.Close();
                    tcpClient = null;
                    return;
                }
                if (readList.Count > 0)
                {
                    try
                    {
                        var rc = netStream.Read(_readBuffer, _nRead, _readBuffer.Length - _nRead);
                        if (rc > 0)
                        {
                            _nRead += rc;
                        }
                    }
                    catch (Exception)
                    {
                        netStream?.Close();
                        netStream = null;
                        tcpClient?.Close();
                        tcpClient = null;
                        return;
                    }
                    continue;
                }
                break;
            }
        }

        private bool Send(ref TcpClient tcpClient, ref NetworkStream netStream, string data)
        {
            int i = 0;
            var dataBytes = Encoding.ASCII.GetBytes(data);
            byte[] bytes = new byte[dataBytes.Length + 10];
            var length = bytes.Length - 2;

            bytes[i++] = (byte)(length / 0x100);
            bytes[i++] = (byte)length;

            bytes[i++] = 1; // Protocol version 
            bytes[i++] = 10; // IVSPROXY_UDPDATA

            var ipEndPoint = tcpClient?.Client?.RemoteEndPoint as IPEndPoint;
            var addressBytes = ipEndPoint?.Address?.MapToIPv4().GetAddressBytes();
            if (addressBytes == null)
            {
                return false;
            }
            for (int l = 0; l < addressBytes.Length; l++)
            {
                bytes[i++] = addressBytes[l];
            }

            var port = ipEndPoint.Port;
            bytes[i++] = (byte)(port / 0x100);
            bytes[i++] = (byte)port;

            for (int l = 0; l < dataBytes.Length; l++)
            {
                bytes[i++] = dataBytes[l];
            }

            try
            {
                netStream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception)
            {
                netStream?.Close();
                netStream = null;
                tcpClient?.Close();
                tcpClient = null;
            }

            _lastSendTime = DateTime.Now;
            return true;
        }

        private bool _reconnected = false;
        public bool SendToRemote(string tag, string value)
        {
            if (_clientId == null || _sceneId == null || _connectionId == null)
            {
                return false;
            }
            if (_tcpClient == null && _netStream == null && !string.IsNullOrWhiteSpace(_hostName) && _port > 0)
            {
                if (!CreateTcpClient(_hostName, _port))
                {
                    return false;
                }
                _reconnected = true;
            }
            var sb = new StringBuilder();
            sb.Append("RQ");
            sb.Append('\0');
            sb.Append("" + _packetId++);
            sb.Append('\0');
            sb.Append(_connectionId);
            sb.Append('\0');
            sb.Append("SET");
            sb.Append('\0');
            sb.Append("SCID");
            sb.Append('\0');
            sb.Append(_sceneId);
            sb.Append('\0');
            sb.Append("CHID");
            sb.Append('\0');
            sb.Append("0");
            sb.Append('\0');
            sb.Append(tag);
            sb.Append('\0');
            sb.Append(value);
            sb.Append('\0');

            return Send(ref _tcpClient, ref _netStream, sb.ToString());
        }

        private HashSet<string> _packedIdsOfAnimationRequests = new HashSet<string>();

        public bool SendAnimationToRemote(string name)
        {
            var rc = SendToRemote(AnimationTag, name);
            _packedIdsOfAnimationRequests.Add("" + (_packetId - 1));
            return rc;
        }

        public void CallUpdate()
        {
            Update();
        }

        private bool _animationTriggeredLocally = false;

        protected virtual void Update()
        {
            if (string.IsNullOrWhiteSpace(_url))
            {
                return;
            }
            if (_clientId == null || _sceneId == null || _connectionId == null)
            {
                if ((DateTime.Now - _lastSendTime).TotalSeconds > 15)
                {
                    var url = _url;
                    _url = null;
                    SetRemoteServerUrl(url, _sceneUrl, _sceneName, null);
                    return;
                }
            }

            if (_tcpClient == null && _netStream == null && !string.IsNullOrWhiteSpace(_hostName) && _port > 0)
            {
                if (!CreateTcpClient(_hostName, _port))
                {
                    return;
                }
                _reconnected = true;
            }
            if (_tcpClient != null && _netStream != null && _connectionId != null && (DateTime.Now - _lastSendTime).TotalSeconds > 12)
            {
                var sb = new StringBuilder();
                sb.Append("RQ");
                sb.Append('\0');
                sb.Append("" + _packetId++);
                sb.Append('\0');
                sb.Append(_connectionId);
                sb.Append('\0');
                sb.Append("PING");
                sb.Append('\0');

                Send(ref _tcpClient, ref _netStream, sb.ToString());
            }

            if (_tcpClient != null && _netStream != null)
            {
                Receive(ref _tcpClient, ref _netStream);
            }

            string message;
            while ((message = GetMessage()) != null)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }
                var parts = message.Split('\0');
                if (parts == null || parts.Length < 1)
                {
                    continue;
                }

                if (_clientId == null || _sceneId == null || _connectionId == null)
                {
                    if (parts.Length < 1 || "AN" != parts[0])
                    {
                        continue;
                    }
                    if (parts.Length < 2 || "0" != parts[1])
                    {
                        continue;
                    }
                    if (parts.Length < 3 || string.IsNullOrWhiteSpace(parts[2]))
                    {
                        continue;
                    }
                    if (parts.Length < 4 || "HI" != parts[3])
                    {
                        continue;
                    }
                    for (int i = 4; i < parts.Length - 1; i++)
                    {
                        if ("CLID" == parts[i])
                        {
                            _clientId = parts[++i];
                        }
                        else if ("NNM" == parts[i] && parts[++i] != _name)
                        {
                            break;
                        }
                        else if ("SCID" == parts[i])
                        {
                            _sceneId = parts[++i];
                        }
                    }

                    if (_clientId == null || _sceneId == null)
                    {
                        continue;
                    }
                    _connectionId = parts[2];
                    ConnectionStart = DateTime.Now;
                    _callbacks.ForEach(x => x.Set(string.Empty, string.Empty, ConnectionStart, DateTime.Now));
                    continue;
                }
                else if (_reconnected)
                {
                    if (parts.Length >= 4 && "AN" == parts[0] && "0" == parts[1] && !string.IsNullOrWhiteSpace(parts[2]) && "HI" == parts[3])
                    {
                        for (int i = 4; i < parts.Length - 1; i++)
                        {
                            if ("CLID" == parts[i])
                            {
                                _clientId = parts[++i];
                            }
                            else if ("SCID" == parts[i])
                            {
                                _sceneId = parts[++i];
                            }
                        }
                        _connectionId = parts[2];
                        ConnectionStart = DateTime.Now;
                        _callbacks.ForEach(x => x.Set(string.Empty, string.Empty, ConnectionStart, DateTime.Now));
                        _reconnected = false;
                        continue;
                    }
                }
                if ("AN" == parts[0])
                {
                    if (parts.Length > 1)
                    {
                        var packetId = parts[1];
                        if (_packedIdsOfAnimationRequests.Remove(packetId))
                        {
                            _animationTriggeredLocally = true;
                        }
                    }
                    continue;
                }
                if ("RQ" == parts[0])
                {
                    if (parts.Length < 4)
                    {
                        continue;
                    }
                    if ("PING" == parts[3])
                    {
                        var sb = new StringBuilder();
                        sb.Append("AN");
                        sb.Append('\0');
                        sb.Append("" + parts[1]);
                        sb.Append('\0');
                        sb.Append("" + parts[2]);
                        sb.Append('\0');
                        sb.Append("PONG");
                        sb.Append('\0');

                        Send(ref _tcpClient, ref _netStream, sb.ToString());
                        continue;
                    }
                    if ("SET" == parts[3])
                    {
                        string tag = string.Empty;
                        string value = string.Empty;

                        for (int i = 4; i < parts.Length - 1; i++)
                        {
                            if ("SCID" == parts[i] && parts[++i] != _sceneId)
                            {
                                break;
                            }
                            else if (AnimationTag == parts[i])
                            {
                                value = parts[++i];
                                break;
                            }
                            else if (LockTag == parts[i] || VeraPlasticTag == parts[i])
                            {
                                tag = parts[i];
                                value = parts[++i];
                                break;
                            }
                        }

                        var animationTriggeredLocally = _animationTriggeredLocally;
                        _animationTriggeredLocally = false;

                        var animationActivated = false;
                        var arObjectState = ArObjectState;
                        if (arObjectState != null && !string.IsNullOrWhiteSpace(value))
                        {
                            animationActivated = arObjectState.RemoteActivate(value, StartTicks, NowTicks);
                        }
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            _callbacks.ForEach(x => x.Set(tag, value, ConnectionStart, DateTime.Now));
                        }
                        if (animationActivated)
                        {
                            if (animationTriggeredLocally)
                            {
                                value = value.Replace("Remoted", "TriggeredLocally");
                            }
                            else
                            {
                                value = value.Replace("Remoted", "TriggeredRemotely");
                            }
                            arObjectState.RemoteActivate(value, StartTicks, NowTicks);
                            _callbacks.ForEach(x => x.Set(tag, value, ConnectionStart, DateTime.Now));
                        }
                    }
                    if (parts.Length > 3)
                    {
                        var sb = new StringBuilder();
                        sb.Append("AN");
                        sb.Append('\0');
                        sb.Append("" + parts[1]);
                        sb.Append('\0');
                        sb.Append("" + parts[2]);
                        sb.Append('\0');
                        sb.Append("OK");
                        sb.Append('\0');

                        Send(ref _tcpClient, ref _netStream, sb.ToString());
                        continue;
                    }
                }
            }
        }

        private DateTime _lastSendTime = DateTime.MinValue;
        private string _name;
        private string _hostName;
        private int _port;
        private TcpClient _tcpClient;
        private NetworkStream _netStream; // = tcpClient.GetStream();
        private string _url;
        private string _sceneUrl;
        private string _sceneName;
        private string _clientId;
        private string _sceneId;
        private string _connectionId;
        private int _packetId;

        public bool IsRemotingActivated { get { return !string.IsNullOrWhiteSpace(_url); } }

        private List<IRemoteCallback> _callbacks = new List<IRemoteCallback>();
        public void SetRemoteCallback(IRemoteCallback callback)
        {
            if (callback != null && !_callbacks.Contains(callback))
            {
                _callbacks.Add(callback);
                if (ConnectionStart != DateTime.MinValue)
                {
                    callback.Set(string.Empty, string.Empty, ConnectionStart, DateTime.Now);
                }
            }
        }

        public void SetRemoteServerUrl(string url, string sceneUrl, string sceneName, string scriptName)
        {
            if (_url == url && _sceneUrl == sceneUrl && _sceneName == sceneName && _tcpClient != null && _netStream != null)
            {
                return;
            }

            if (_tcpClient != null && _netStream != null && _clientId != null && _connectionId != null)
            {
                var sb = new StringBuilder();
                sb.Append("RQ");
                sb.Append('\0');
                sb.Append("" + _packetId++);
                sb.Append('\0');
                sb.Append(_connectionId);
                sb.Append('\0');
                sb.Append("BYE");
                sb.Append('\0');
                sb.Append("CLID");
                sb.Append('\0');
                sb.Append(_clientId);
                sb.Append('\0');

                Send(ref _tcpClient, ref _netStream, sb.ToString());
            }

            _sceneUrl = sceneUrl;
            _sceneName = string.IsNullOrWhiteSpace(sceneName) ? sceneUrl : sceneName;

            if (_netStream != null)
            {
                _netStream.Close();
                _netStream = null;
            }
            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
            }

            _clientId = null;
            _sceneId = null;
            _connectionId = null;
            ConnectionStart = DateTime.MinValue;
            _lastSendTime = DateTime.MinValue;
            _url = null;
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            string ptr = url;
            int index = ptr.IndexOf('@');
            if (index > 0)
            {
                _name = ptr.Substring(0, index);
                if (ptr.Length > index + 1)
                {
                    ptr = ptr.Substring(index + 1);
                }
                else
                {
                    ptr = null;
                }
            }
            else
            {
                _name = "N";
            }
            if (string.IsNullOrWhiteSpace(ptr))
            {
                return;
            }
            _name += $"/{(string.IsNullOrWhiteSpace(scriptName) ? ApplicationName : scriptName)}/{OperatingSystem}/{Bundle}";
            _name += $"/{UsedLatitude.ToString("F6", CultureInfo.InvariantCulture)}";
            _name += $"/{UsedLongitude.ToString("F6", CultureInfo.InvariantCulture)}";
            _name += DateTime.Now.ToString("/HH:mm:ss.fff");

            _hostName = null;
            _port = 0;
            string port = "2000";
            index = ptr.IndexOf(':');
            if (index > 0)
            {
                _hostName = ptr.Substring(0, index);
                if (ptr.Length > index + 1)
                {
                    port = ptr.Substring(index + 1);
                }
            }
            if (string.IsNullOrWhiteSpace(_hostName) || string.IsNullOrWhiteSpace(port))
            {
                return;
            }
            if (!int.TryParse(port, out _port))
            {
                return;
            }

            if (_port < 1 || _port > 0xffff)
            {
                return;
            }

            if (CreateTcpClient(_hostName, _port))
            {
                _url = url;
            }
        }

        private bool CreateTcpClient(string hostName, int port)
        {
            try
            {
                _tcpClient = new TcpClient(hostName, port);
                _netStream = _tcpClient.GetStream();
            }
            catch (Exception)
            {
                if (_netStream != null)
                {
                    _netStream.Close();
                    _netStream = null;
                }
                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                    _tcpClient = null;
                }
                return false;
            }

            _packetId = 0;
            if (_packetId < 1)
            {
                var sb = new StringBuilder();
                sb.Append("RQ");
                sb.Append('\0');
                sb.Append("" + _packetId++);
                sb.Append('\0');
                sb.Append("0");
                sb.Append('\0');
                sb.Append("ENTER");
                sb.Append('\0');
                sb.Append("NNM");
                sb.Append('\0');
                sb.Append(_name);
                sb.Append('\0');
                sb.Append("SCU");
                sb.Append('\0');
                sb.Append(_sceneUrl);
                sb.Append('\0');
                sb.Append("SCN");
                sb.Append('\0');
                sb.Append(_sceneName);
                sb.Append('\0');

                Send(ref _tcpClient, ref _netStream, sb.ToString());
            }
            return true;
        }
        private readonly List<string> _messages = new List<string>();

        private void AddMessage(string message)
        {
            _messages.Add(message);
        }

        private string GetMessage()
        {
            if (_messages.Any())
            {
                var result = _messages[0];
                _messages.RemoveAt(0);
                return result;
            }
            return null;
        }

        private const string _b64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        public static byte ToBase64(int i)
        {
            return (byte)_b64Chars[Math.Abs(i) % 64];
        }

        public static int FromBase64(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A';
            }
            if (c >= 'a' && c <= 'z')
            {
                return 26 + c - 'a';
            }
            if (c >= '0' && c <= '9')
            {
                return 52 + c - '0';
            }
            if (c == '+')
            {
                return 62;
            }
            return 63;
        }

        private static System.Random _random = new System.Random((int)DateTime.Now.Ticks);
        public static string RandomString(int length)
        {
            return new string(Enumerable.Range(1, length).Select(_ => _b64Chars[_random.Next(_b64Chars.Length)]).ToArray());
        }
    }
}
