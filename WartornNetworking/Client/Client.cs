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
            return reply.messages == Messages.Success;
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
        /// request that the server send the roomId in which the client is currently reside in
        /// </summary>
        /// <returns>the roomID</returns>
        public string GetRoomID()
        {
            Package package = new Package(Messages.Request, Commands.GetRoom, "");
            Package reply = SendPackageToServer(package,isGetReply: true);
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
            return reply.messages == Messages.Success;
        }

        /// <summary>
        /// get the clientid of this client
        /// </summary>
        /// <returns>the clientid</returns>
        public string GetClientID()
        {
            Package package = new Package(Messages.Request, Commands.GetClientID, "");
            Package reply = SendPackageToServer(package, isGetReply: true);
            return reply.data;
        }

        private Package SendPackageToServer(Package package, bool isGetReply = false)
        {
            string packageConverted = JsonConvert.SerializeObject(package);
            if (isGetReply)
            {
                Message reply = client.WriteLineAndGetReply(packageConverted, TimeSpan.FromSeconds(Constants.MaxTimeOut));
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
