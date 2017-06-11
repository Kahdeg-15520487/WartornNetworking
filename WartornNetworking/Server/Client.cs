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

namespace WartornNetworking.Server
{
    public class Client : IEquatable<Client>
    {
        public readonly string clientID;
        public readonly TcpClient tcpclient;

        public string roomID { get; set; } = string.Empty;
        public IPEndPoint IP { get { return (IPEndPoint)tcpclient.Client.RemoteEndPoint; } }
        public TcpState State { get { return tcpclient.GetState(); } }

        public Client(TcpClient c)
        {
            clientID = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
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
            return string.Compare(this.clientID, other.clientID) == 0 || this.tcpclient.IsEqual(other.tcpclient);
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
