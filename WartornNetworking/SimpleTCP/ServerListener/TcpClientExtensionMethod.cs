using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WartornNetworking.SimpleTcp
{
    public static class TcpClientExtensionMethod
    {
        public static bool IsEqual(this TcpClient tcpClient, TcpClient tcpClientOther)
        {
            var Client1localep = ((IPEndPoint)tcpClient.Client.LocalEndPoint);
            var Client1remoteep = ((IPEndPoint)tcpClient.Client.RemoteEndPoint);
            var Client2localep = ((IPEndPoint)tcpClientOther.Client.LocalEndPoint);
            var Client2remoteep = ((IPEndPoint)tcpClientOther.Client.RemoteEndPoint);

            return Client1localep.Equals(Client2localep) && Client1remoteep.Equals(Client2remoteep);
        }



        public static bool IsConnected(this TcpClient c)
        {
            return c.GetState() == TcpState.Established;
        }

        public static bool IsDisconnected(this TcpClient c)
        {
            return c.GetState() != TcpState.Established;
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
