using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimplePing
{

    class SimplePing
    {
        static void Main(string[] args)
        {
            byte[] data = new byte[1024];
            int recv;
            Socket host = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            IPAddress[] ipv4Addresses = Array.FindAll(
                Dns.GetHostEntry(string.Empty).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            int port = 0;
            IPEndPoint ip = new IPEndPoint(ipv4Addresses[0], port);
            EndPoint ep = ip;
            ICMP packet = new ICMP();
            packet.Type = 8;
            packet.Code = 0;
            packet.Checksum = 0;
            Buffer.BlockCopy(
                BitConverter.GetBytes((short)1), 0, packet.Message, 0, 2);
            Buffer.BlockCopy(
                BitConverter.GetBytes((short)1), 0, packet.Message, 2, 2);
            data = Encoding.ASCII.GetBytes("test packet");
            Buffer.BlockCopy(data, 0, packet.Message, 4, data.Length);
            packet.MessageSize = data.Length + 4;
            int packetsize = packet.MessageSize + 4;
            UInt16 chcksum = packet.getChecksum();
            packet.Checksum = chcksum;
            
            host.SendTo(packet.getBytes(), packetsize, SocketFlags.None, ip);
            try
            {
                data = new byte[1024];
                recv = host.ReceiveFrom(data, ref ep);
            }
            catch (SocketException)
            {
                Console.WriteLine("No response from remote host");
                return;
            }
            ICMP response = new ICMP(data, recv);
            Console.WriteLine("response from: {0}", ep.ToString());
            Console.WriteLine(" Type {0}", response.Type);
            Console.WriteLine(" Code: {0}", response.Code);
            int Identifier = BitConverter.ToInt16(response.Message, 0);
            int Sequence = BitConverter.ToInt16(response.Message, 2);
            Console.WriteLine(" Identifier: {0}", Identifier);
            Console.WriteLine(" Sequence: {0}", Sequence);
            string stringData = Encoding.ASCII.GetString(response.Message,
                4, response.MessageSize - 4);
            Console.WriteLine(" data: {0}", stringData);
            host.Close();
        }
    }
}
