using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WartornNetworking.Server
{
    public class Room
    {
        public string name { get; set; }
        public readonly string roomID;
        public Dictionary<string,Client> clients { get; private set; }

        public int ClientsCount { get { return clients.Keys.Count; } }

        public Room()
        {
            roomID = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            name = roomID;
            clients = new Dictionary<string, Client>();
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
            client.roomID = string.Empty;
            clients.Remove(client.clientID);
        }

        public void RemoveClient(TcpClient tcpclient)
        {
            string markedForRemove = string.Empty;
            foreach (string key in clients.Keys)
            {
                if (clients[key].tcpclient == tcpclient)
                {
                    markedForRemove = key;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(markedForRemove))
            {
                clients.Remove(markedForRemove);
            }
        }

        internal bool ContainClient(Client client)
        {
            return clients.ContainsKey(client.clientID);
        }
    }
}
