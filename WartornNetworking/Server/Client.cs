using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WartornNetworking.SimpleTCP.Server;

namespace WartornNetworking.Server
{
    public class Client : IEquatable<Client>
    {
        public readonly string clientID;
        public readonly TcpClient tcpclient;

        public string roomID { get; set; } = string.Empty;

        public Client(TcpClient c)
        {
            clientID = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            tcpclient = c;
        }

        public bool Equals(Client other)
        {
            return string.Compare(clientID, other.clientID) == 0 || tcpclient == other.tcpclient;
        }

        public IPEndPoint IP
        {
            get
            {
                return (IPEndPoint)tcpclient.Client.RemoteEndPoint;
            }
        }

        public TcpState State
        {
            get
            {
                return tcpclient.GetState();
            }
        }
    }
}
