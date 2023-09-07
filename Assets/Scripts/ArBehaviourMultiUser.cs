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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviourMultiUser : MonoBehaviour
    {
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

        protected long CurrentSecond { get; private set; }

        protected virtual void Start()
        {
        }

        private byte[] _readBuffer = new byte[2048];
        private int _nRead = 0;

        private void Receive(ref TcpClient tcpClient, ref NetworkStream netStream)
        {
            if (tcpClient == null || netStream == null)
            {
                return;
            }

            var socket = tcpClient.Client;
            if (socket != null)
            {
                for (; ; )
                {
                    int length = 0;
                    if (_nRead >= 2)
                    {
                        length = _readBuffer[0] * 0xff;
                        length += _readBuffer[1];

                        if (length < 8)
                        {
                            netStream.Close();
                            netStream = null;
                            tcpClient.Close();
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
                            byte[] newBuffer = new byte[2048];
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
                        netStream.Close();
                        netStream = null;
                        tcpClient.Close();
                        tcpClient = null;
                        return;
                    }
                    if (errorList.Count > 0)
                    {
                        netStream.Close();
                        netStream = null;
                        tcpClient.Close();
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
                            netStream.Close();
                            netStream = null;
                            tcpClient.Close();
                            tcpClient = null;
                        }
                        continue;
                    }
                    break;
                }
            }
        }

        private bool Send(ref TcpClient tcpClient, ref NetworkStream netStream, string data)
        {
            int i = 0;
            var dataBytes = Encoding.ASCII.GetBytes(data);
            byte[] bytes = new byte[dataBytes.Length + 10];
            var length = bytes.Length - 2;

            bytes[i++] = (byte)(length / 0xff);
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
            bytes[i++] = (byte)(port / 0xff);
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
                netStream.Close();
                netStream = null;
                tcpClient.Close();
                tcpClient = null;
            }

            _lastSendTime = DateTime.Now;
            return true;
        }

        public bool SendToRemote(string animationName)
        {
            if (_tcpClient == null || _netStream == null || _clientId == null || _sceneId == null || _connectionId == null)
            {
                return false;
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
            sb.Append("Animation");
            sb.Append('\0');
            sb.Append(animationName);
            sb.Append('\0');

            return Send(ref _tcpClient, ref _netStream, sb.ToString());
        }

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
                    SetRemoteServerUrl(url, _sceneUrl, _sceneName);
                    return;
                }
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
                    if (parts.Length < 1 || !"AN".Equals(parts[0]))
                    {
                        continue;
                    }
                    if (parts.Length < 2 || !"0".Equals(parts[1]))
                    {
                        continue;
                    }
                    if (parts.Length < 3 || string.IsNullOrWhiteSpace(parts[2]))
                    {
                        continue;
                    }
                    if (parts.Length < 4 || !"HI".Equals(parts[3]))
                    {
                        continue;
                    }
                    for (int i = 4; i < parts.Length - 1; i++)
                    {
                        if ("CLID".Equals(parts[i]))
                        {
                            _clientId = parts[++i];
                        }
                        else if ("NNM".Equals(parts[i]) && !parts[++i].Equals(_name))
                        {
                            break;
                        }
                        else if ("SCID".Equals(parts[i]))
                        {
                            _sceneId = parts[++i];
                        }
                    }

                    if (_clientId == null || _sceneId == null)
                    {
                        continue;
                    }
                    _connectionId = parts[2];
                    continue;
                }
                if ("AN".Equals(parts[0]))
                {
                    continue;
                }
                if ("RQ".Equals(parts[0]))
                {
                    if (parts.Length < 4)
                    {
                        continue;
                    }
                    if ("PING".Equals(parts[3]))
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
                    if ("SET".Equals(parts[3]))
                    {
                        string animationName = string.Empty;

                        for (int i = 4; i < parts.Length - 1; i++)
                        {
                            if ("SCID".Equals(parts[i]) && !parts[++i].Equals(_sceneId))
                            {
                                break;
                            }
                            else if ("Animation".Equals(parts[i]))
                            {
                                animationName = parts[++i];
                                break;
                            }
                        }

                        var arObjectState = ArObjectState;
                        if (arObjectState != null && !string.IsNullOrWhiteSpace(animationName))
                        {
                            arObjectState.RemoteActivate(animationName, StartTicks, NowTicks);
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

        public void SetRemoteServerUrl(string url, string sceneUrl, string sceneName)
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
            _sceneName = sceneName;

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

            _name += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

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

            try
            {
                _tcpClient = new TcpClient(_hostName, _port);
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
                return;
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
            _url = url;
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
    }
}
