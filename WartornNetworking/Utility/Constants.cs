namespace WartornNetworking
{
    namespace Utility
    {
        public enum Messages
        {
            Accept,     //accept a request
            Deny,       //deny a request
            Request,    //request doing a command
            Success,    //Success in doing a command
            Fail        //Failed to do a command
        }

        public enum Commands
        {
            Disconnect, //disconnect from the server
            Message,    //send to a client in the same room with <clientId> the message of <msg>
            Broadcast,  //send the message of <msg> to every client in the same room
            Inform,     //inform the server/client about something
            GetRooms,   //request the roomid list from the server
            GetRoomID,  //request the roomid of this client
            CreateRoom, //request to create a room
            JoinRoom,   //request to join the room with <roomId>
            GetClientID,//request the clientid of this client
            GetClients  //request the clientid list of every client in the same room
        }

        public static class Constants
        {
            public static int MaxTimeOut = 10;
        }
    }
}