using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WartornNetworking.SimpleTCP.Server
{
    internal class ServerListener
    {
        private TcpListenerEx _listener = null;
        private List<TcpClient> _connectedClients = new List<TcpClient>();
        private List<TcpClient> _disconnectedClients = new List<TcpClient>();
        private SimpleTcpServer _parent = null;
        private List<byte> _queuedMsg = new List<byte>();
        private byte _delimiter = 0x13;
        private Thread _rxThread = null;

        public int ConnectedClientsCount
        {
            get { return _connectedClients.Count; }
        }

        public IEnumerable<TcpClient> ConnectedClients { get { return _connectedClients; } }

        internal ServerListener(SimpleTcpServer parentServer, IPAddress ipAddress, int port)
        {
            QueueStop = false;
            _parent = parentServer;
            IPAddress = ipAddress;
            Port = port;
            ReadLoopIntervalMs = 10;

            _listener = new TcpListenerEx(ipAddress, port);
            _listener.Start();

            System.Threading.ThreadPool.QueueUserWorkItem(ListenerLoop);
        }

        private void StartThread()
        {
            if (_rxThread != null) { return; }
            _rxThread = new Thread(ListenerLoop);
            _rxThread.IsBackground = true;
            _rxThread.Start();
        }

        internal bool QueueStop { get; set; }
        internal IPAddress IPAddress { get; private set; }
        internal int Port { get; private set; }
        internal int ReadLoopIntervalMs { get; set; }

        internal TcpListenerEx Listener { get { return _listener; } }


		private void ListenerLoop(object state)
        {
            while (!QueueStop)
            {
                try
                {
                    RunLoopStep();
                }
                catch 
                {

                }

                System.Threading.Thread.Sleep(ReadLoopIntervalMs);
            }
			_listener.Stop();
        }

        private void RunLoopStep()
        {
            if (_disconnectedClients.Count > 0)
            {
                var disconnectedClients = _disconnectedClients.ToArray();
                _disconnectedClients.Clear();

                foreach (var disC in disconnectedClients)
                {
                    _connectedClients.Remove(disC);
                    _parent.NotifyClientDisconnected(this, disC);
                }
            }

            if (_listener.Pending())
            {
				var newClient = _listener.AcceptTcpClient();
				_connectedClients.Add(newClient);

                //string reply = "henlo";
                //var data = (Encoding.UTF8).GetBytes(reply);
                //newClient.GetStream().Write(data, 0, data.Length);

                _parent.NotifyClientConnected(this, newClient);
                System.IO.File.AppendAllText("incomingconnection.txt", ((IPEndPoint)_connectedClients[_connectedClients.Count - 1].Client.RemoteEndPoint).Address + ":" + ((IPEndPoint)_connectedClients[_connectedClients.Count - 1].Client.RemoteEndPoint).Port + " connected" + Environment.NewLine);
            }
            
            _delimiter = _parent.Delimiter;

            foreach (var c in _connectedClients)
            {
                //int bytesAvailable = c.Available;
                //if (bytesAvailable == 0)
                //{
                //    //Thread.Sleep(10);
                //    continue;
                //}

                List<byte> bytesReceived = new List<byte>();

                while (c.Available > 0 && IsConnected(c))
                {
                    byte[] nextByte = new byte[1];
                    c.Client.Receive(nextByte, 0, 1, SocketFlags.None);
                    bytesReceived.AddRange(nextByte);

                    if (nextByte[0] == _delimiter)
                    {
                        byte[] msg = _queuedMsg.ToArray();
                        var daata = (Encoding.UTF8).GetString(msg);
                        System.IO.File.AppendAllText("incomingconnection.txt", ((IPEndPoint)_connectedClients[_connectedClients.Count - 1].Client.RemoteEndPoint).Address + ":" + ((IPEndPoint)_connectedClients[_connectedClients.Count - 1].Client.RemoteEndPoint).Port + " sent : " + daata + Environment.NewLine);
                        _queuedMsg.Clear();
                        _parent.NotifyDelimiterMessageRx(this, c, msg);
                    } else
                    {
                        _queuedMsg.AddRange(nextByte);
                    }
                }

                if (bytesReceived.Count > 0)
                {
                    _parent.NotifyEndTransmissionRx(this, c, bytesReceived.ToArray());
                }

                if (IsDisconnected(c))
                {
                    _disconnectedClients.Add(c);
                }
            }
        }

        private bool IsConnected(TcpClient c)
        {
            return c.GetState() == TcpState.Established;
        }

        private bool IsDisconnected(TcpClient c)
        {
            return c.GetState() != TcpState.Established;
        }
    }

    public static class TcpClientExtensionMethod
    {
        public static bool IsEqual(this TcpClient tcpClient,TcpClient tcpClientOther)
        {
            var Client1localep = ((IPEndPoint)tcpClient.Client.LocalEndPoint);
            var Client1remoteep = ((IPEndPoint)tcpClient.Client.RemoteEndPoint);
            var Client2localep = ((IPEndPoint)tcpClientOther.Client.LocalEndPoint);
            var Client2remoteep = ((IPEndPoint)tcpClientOther.Client.RemoteEndPoint);

            return Client1localep.Equals(Client2localep) && Client1remoteep.Equals(Client2remoteep);
        }

        public static TcpState GetState(this TcpClient c)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections()
                .SingleOrDefault(x => x.RemoteEndPoint.Equals(c.Client.RemoteEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }
    }
}
