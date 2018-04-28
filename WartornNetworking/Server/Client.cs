using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WartornNetworking.SimpleTcp;
using WartornNetworking.SimpleTCP.Server;
using WartornNetworking.Utility;

namespace WartornNetworking.Server
{
    public class Client : IEquatable<Client>
    {
        public readonly long clientID;
        public readonly TcpClient tcpclient;

        public long roomID { get; set; } = -1;
        public IPEndPoint IP { get { return (IPEndPoint)tcpclient.Client.RemoteEndPoint; } }
        public TcpState State { get { return tcpclient.GetState(); } }

        public Client(TcpClient c)
        {
            clientID = RandomIdGenerator.GetBase62inLong(10);
            tcpclient = c;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 91;
                // Suitable nullity checks etc, of course :)
                hash = hash * 101 + clientID.GetHashCode();
                hash = hash * 101 + tcpclient.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj.GetType() == typeof(Client)) && this.Equals((Client)obj);
        }

        public bool Equals(Client other)
        {
            return this.clientID == other.clientID || this.tcpclient.IsEqual(other.tcpclient);
        }

        public static bool operator ==(Client client1, Client client2)
        {
            return client1.Equals(client2);
        }

        public static bool operator !=(Client client1, Client client2)
        {
            return !client1.Equals(client2);
        }
    }
}
