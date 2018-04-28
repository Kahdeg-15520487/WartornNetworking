using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WartornNetworking.Utility;

namespace WartornNetworking.Server
{
    public class Room : IEquatable<Room>
    {
        public string name { get; set; }
        public readonly long roomID;
        public Dictionary<long, Client> clients { get; private set; }

        public int ClientsCount { get { return clients.Keys.Count; } }

        public Room()
        {
            roomID = RandomIdGenerator.GetBase62inLong(10);
            name = roomID.ToString();
            clients = new Dictionary<long, Client>();
        }

        public void AddClient(Client client)
        {
            if (!clients.ContainsKey(client.clientID))
            {
                client.roomID = roomID;
                clients.Add(client.clientID, client);
            }
        }

        public void RemoveClient(Client client)
        {
            client.roomID = -1;
            clients.Remove(client.clientID);
        }

        public void RemoveClient(TcpClient tcpclient)
        {
            long markedForRemove = -1;
            foreach (long key in clients.Keys)
            {
                if (clients[key].tcpclient == tcpclient)
                {
                    markedForRemove = key;
                    break;
                }
            }
            if (markedForRemove != -1)
            {
                clients.Remove(markedForRemove);
            }
        }

        internal bool ContainClient(Client client)
        {
            return clients.ContainsKey(client.clientID);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 91;
                // Suitable nullity checks etc, of course :)
                hash = hash * 101 + roomID.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object o)
        {
            return (o.GetType() == typeof(Room)) && this.Equals((Room)o);
        }

        public bool Equals(Room other)
        {
            return this.roomID == other.roomID;
        }

        public static bool operator ==(Room room1, Room room2)
        {
            return room1.Equals(room2);
        }

        public static bool operator !=(Room room1, Room room2)
        {
            return !room1.Equals(room2);
        }
    }
}
