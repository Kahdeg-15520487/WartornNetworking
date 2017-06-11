using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using WartornNetworking.SimpleTCP;
using Newtonsoft.Json;
using WartornNetworking.Utility;
using System.IO;
using WartornNetworking.SimpleTCP.Server;

namespace WartornNetworking.Server
{
    public class Server
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

        public SimpleTcpServer server { get; private set;}

        //list of client that is connected
        public Dictionary<string, Client> clients { get; private set; }
        //room for client that is not in any room
        public Room hall { get; private set; }
        //list of room
        public Dictionary<string, Room> rooms { get; private set; }

        public event EventHandler<ServerEventArgs> ClientConnected;
        public event EventHandler<ServerEventArgs> ClientDisconnected;
        public event EventHandler<ServerEventArgs> PackageDataReceived;

        public Server(int Port)
        {
            File.AppendAllText("log.txt", "=========" + Environment.NewLine + DateTime.Now.ToString(@"dd\/MM\/yyyy HH:mm") + Environment.NewLine);

            server = new SimpleTcpServer();
            server.Start(Port);

            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.DelimiterDataReceived += Server_DelimiterDataReceived;

            clients = new Dictionary<string, Client>();
            rooms = new Dictionary<string, Room>();

            hall = new Room();
            hall.name = "Hall";
            rooms.Add(hall.roomID, hall);
        }

        private void Server_ClientConnected(object sender, TcpClient e)
        {
            Client client = new Client(e);
            clients.Add(client.clientID,client);
            hall.AddClient(client);
            ClientConnected?.Invoke(sender, new ServerEventArgs(client, null));
        }

        #region client communication wrapper
        private void SendPackageToRoom(Room room,Package package)
        {
            foreach (KeyValuePair<string,Client> kvp in room.clients)
            {
                SendPackageToClient(kvp.Value, package);
            }
        }

        private void SendPackageToClient(Client client,Package package)
        {
            server.WriteLine(client.tcpclient, JsonConvert.SerializeObject(package));
        }
        #endregion

        #region finder helper
        private Room FindRoomThatHaveClient(Client client)
        {
            return FindRoom(client.roomID);
        }

        private Room FindRoom(string roomId)
        {
            if (rooms.ContainsKey(roomId))
            {
                return rooms[roomId];
            }
            else
            {
                return null;
            }
        }

        private Client FindClient(string clientId)
        {
            if (clients.ContainsKey(clientId))
            {
                return clients[clientId];
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region handle package receive
        private void Server_DelimiterDataReceived(object sender, Message e)
        {
            Package msg = JsonConvert.DeserializeObject<Package>(e.MessageString);
            Package reply;
            Client client = clients.First(c => { return c.Value.tcpclient.IsEqual(e.TcpClient); }).Value;
            Room room = FindRoomThatHaveClient(client);

            File.AppendAllText("log.txt", string.Format("{0},{1} : {2}{3}", client.clientID, (IPEndPoint)client.tcpclient.Client.RemoteEndPoint, msg, Environment.NewLine));
            PackageDataReceived?.Invoke(sender, new ServerEventArgs(client, msg));

            if (msg.messages == Messages.Request)
            {
                switch (msg.commands)
                {
                    //disconnect from the server
                    case Commands.Disconnect:
                        reply = new Package(Messages.Success, Commands.Inform, "bye bye");
                        SendPackageToClient(client, reply);
                        Server_ClientDisconnected(this, client.tcpclient);
                        break;

                    //send a private message to another client
                    //package data content:
                    //<receiver's clientID>|<message>
                    case Commands.Message:
                        var datas = msg.data.Split('|');
                        //parse receiver clientid...
                        Client receiver = FindClient(datas[0]);
                        //check if receiver is connected
                        if (receiver != null)
                        {
                            //send success to the client
                            SendPackageToClient(client, new Package(Messages.Success, Commands.Inform, ""));
                            //send the message to the receiver
                            SendPackageToClient(receiver, new Package(Messages.Request, Commands.Message, client.clientID + "|" + datas[1]));
                        }
                        else
                        {
                            //send fail to the client
                            SendPackageToClient(client, new Package(Messages.Fail, Commands.Inform, "There is no such client"));
                        }
                        break;

                    //send a broadcast message to everyone who are in the same room as client
                    //package data content:
                    //<message>
                    case Commands.Broadcast:
                        reply = new Package(Messages.Request, Commands.Message, client.clientID + "|" + msg.data);
                        //check if the client is not in the hall
                        if (hall.ContainClient(client))
                        {
                            //send the message to everyone in the hall
                            SendPackageToRoom(hall, reply);
                        }
                        else
                        {
                            //send the data to everyone in the same roomid
                            SendPackageToRoom(room, reply);
                        }
                        //return Success
                        //return Inform
                        //return data empty
                        SendPackageToClient(client, new Package(Messages.Success, Commands.Inform, ""));
                        break;

                    //get the roomid of the client
                    //package data content:
                    //<Empty>
                    case Commands.GetRoom:
                        //send the roomid to the client if the client is already in a room
                        reply = new Package(Messages.Success, Commands.Inform, client.roomID);
                        //and send that roomid to the client
                        //return Success
                        //return Inform
                        //return data = roomId
                        SendPackageToClient(client, reply);
                        break;

                    //create a new room and move into it
                    //package data content:
                    //<Empty>
                    case Commands.CreateRoom:
                        //create a new room
                        room = new Room();
                        rooms.Add(room.roomID, room);
                        //remove the client from the currently resided room
                        FindRoomThatHaveClient(client).RemoveClient(client);
                        //move client to the newly created room
                        room.AddClient(client);
                        reply = new Package(Messages.Success, Commands.Inform, room.roomID);
                        //and send that roomid to the client
                        SendPackageToClient(client, reply);
                        break;

                    //join another room
                    //package data content:
                    //<roomID>
                    case Commands.JoinRoom:
                        //check if that room exist
                        room = FindRoom(msg.data);
                        if (room != null)
                        {
                            //remove the client from the currently resided room
                            FindRoomThatHaveClient(client).RemoveClient(client);
                            //add the client into that room
                            room.AddClient(client);
                            //return Success
                            //return Inform
                            //return data empty
                            reply = new Package(Messages.Success, Commands.Inform, "");
                        }
                        else
                        {
                            //return Fail
                            //return Inform
                            //return data = roomId
                            reply = new Package(Messages.Fail, Commands.Inform, "No such room!");
                        }
                        SendPackageToClient(client, reply);
                        break;

                    //get the clientID of the client
                    case Commands.GetClientID:
                        reply = new Package(Messages.Success, Commands.Inform, client.clientID);
                        SendPackageToClient(client, reply);
                        break;

                    default:
                        break;
                }
            }
        }
        #endregion

        private void Server_ClientDisconnected(object sender, TcpClient e)
        {
            Client client = clients.First(c => { return c.Value.tcpclient.IsEqual(e); }).Value;
            clients.Remove(client.clientID);

            Room room = FindRoomThatHaveClient(client);
            room.RemoveClient(client);

            ClientDisconnected?.Invoke(sender,new ServerEventArgs(client,null));
        }

        #region public method

        public void BroadcastMessage(string message)
        {
            Package package = new Package(Messages.Request, Commands.Message, "server" + "|" + message);
            clients.ToList().ForEach(kvp =>
            {
                SendPackageToClient(kvp.Value, package);
            });
        }

        #endregion
    }

    public class ServerEventArgs : EventArgs
    {
        public Client client { get; set; }
        public Package package { get; set; }
        public ServerEventArgs(Client client,Package package)
        {
            this.client = client;
            this.package = package;
        }
    }
}
