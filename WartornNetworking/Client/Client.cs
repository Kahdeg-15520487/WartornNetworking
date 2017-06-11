using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using WartornNetworking.SimpleTCP;
using WartornNetworking.Utility;
using Newtonsoft.Json;

namespace WartornNetworking.Client
{
    public class Client
    {
        public static void Init()
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new MessageJsonConverter());
                return settings;
            };
        }

        public SimpleTcpClient client { get; private set; }

        public event EventHandler<ClientEventArts> MessageReceived;
        public event EventHandler<ClientEventArts> Disconnected;

        public Client(IPAddress ipaddress,int Port)
        {
            client = new SimpleTcpClient();

            client.Connect(ipaddress.ToString(), Port);

            client.DelimiterDataReceived += Tcpclient_DelimiterDataReceived;
            client.Disconnected += OnDisconnected;
        }

        /// <summary>
        /// Send a private message to another client
        /// </summary>
        /// <param name="clientId">the receiver's clientID</param>
        /// <param name="message">the message</param>
        /// <returns>true if the message is successfully sent</returns>
        public bool SendMessage(string clientId,string message)
        {
            Package package = new Package(Messages.Request, Commands.Message, clientId + "|" + message);
            Package reply = SendPackageToServer(package, isGetReply: true);
            return reply != null && reply.messages == Messages.Success;
        }

        /// <summary>
        /// broadcast a message to everyone in the room
        /// </summary>
        /// <param name="message">the message</param>
        public void BroadcastMessage(string message)
        {
            Package package = new Package(Messages.Request, Commands.Broadcast,message);
            SendPackageToServer(package);
        }

        /// <summary>
        /// request the roomid list from server
        /// </summary>
        /// <returns>the roomid list</returns>
        public IEnumerable<string> GetRooms()
        {
            Package package = new Package(Messages.Request, Commands.GetRooms, "");
            Package reply = SendPackageToServer(package, isGetReply: true);
            if (reply == null)
            {
                yield return null;
                yield break;
            }

            var datas = reply.data.Split('|');
            int roomCount = int.Parse(datas[0]);
            for (int i = 1; i <= roomCount; i++)
            {
                yield return datas[i];
            }
        }

        /// <summary>
        /// request that the server send the roomId in which the client is currently reside in
        /// </summary>
        /// <returns>the roomID</returns>
        public string GetRoomID()
        {
            Package package = new Package(Messages.Request, Commands.GetRoomID, "");
            Package reply = SendPackageToServer(package,isGetReply: true);
            if (reply == null)
            {
                return null;
            }
            string roomId = reply.data;
            //do something with the received roomId;
            return roomId;
        }

        /// <summary>
        /// request that the server create a room
        /// </summary>
        /// <returns>the roomID</returns>
        public string CreateRoom()
        {
            Package package = new Package(Messages.Request, Commands.CreateRoom, "");
            Package reply = SendPackageToServer(package, isGetReply: true);
            if (reply == null)
            {
                return null;
            }
            string roomId = reply.data;
            //do something with the received roomId
            return roomId;
        }

        /// <summary>
        /// join a room on the server with roomId
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns>true if successfully join the room</returns>
        public bool JoinRoom(string roomId)
        {
            Package package = new Package(Messages.Request, Commands.JoinRoom, roomId);
            Package reply = SendPackageToServer(package, isGetReply: true);
            return reply != null && reply.messages == Messages.Success;
        }

        /// <summary>
        /// request the clientID list from the server
        /// </summary>
        /// <returns>the clientID list</returns>
        public IEnumerable<string> GetClients()
        {
            Package package = new Package(Messages.Request, Commands.GetClients, "");
            Package reply = SendPackageToServer(package, isGetReply: true);
            if (reply == null)
            {
                yield return null;
                yield break;
            }
            var datas = reply.data.Split('|');
            int roomCount = int.Parse(datas[0]);
            for (int i = 1; i <= roomCount; i++)
            {
                yield return datas[i];
            }
        }

        /// <summary>
        /// get the clientid of this client
        /// </summary>
        /// <returns>the clientid</returns>
        public string GetClientID()
        {
            Package package = new Package(Messages.Request, Commands.GetClientID, "");
            Package reply = SendPackageToServer(package, isGetReply: true);
            if (reply == null)
            {
                return null;
            }
            return reply.data;
        }

        /// <summary>
        /// send a package to server
        /// </summary>
        /// <param name="package">the package to be sent</param>
        /// <param name="isGetReply">true if want a reply</param>
        /// <returns>a package if isGetReply = true or null if isGetReply = false</returns>
        private Package SendPackageToServer(Package package, bool isGetReply = false)
        {
            string packageConverted = JsonConvert.SerializeObject(package);
            if (isGetReply)
            {
                Message reply = client.WriteLineAndGetReply(packageConverted, TimeSpan.FromSeconds(Constants.MaxTimeOut));
                if (reply == null)
                {
                    return null;
                }
                string message = reply.MessageString.Remove(reply.MessageString.Length - 1);
                Package replyPackage = JsonConvert.DeserializeObject<Package>(message);
                return replyPackage;
            }
            else
            {
                client.WriteLine(packageConverted);
                return new Package(Messages.Accept, Commands.Inform, "");
            }
        }

        private void Tcpclient_DelimiterDataReceived(object sender, Message e)
        {
            Package package = JsonConvert.DeserializeObject<Package>(e.MessageString);
            switch (package.messages)
            {
                case Messages.Request:
                    if (package.commands == Commands.Message)
                    {
                        OnMessageReceived(package);
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnMessageReceived(Package package)
        {
            MessageReceived?.Invoke(this, new ClientEventArts(package));
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Disconnected?.Invoke(this, new ClientEventArts(null));
        }
    }

    public class ClientEventArts: EventArgs
    {
        public Package package { get; private set; }
        public ClientEventArts(Package package)
        {
            this.package = package;
        }
    }
}
