using System;
using System.Net.Sockets;
using System.Text;

namespace YinXiang
{
    public static class TcpClientExtension
    {
        public static string Send(this TcpClient client, Encoding encoding, string message)
        {
            Byte[] data = encoding.GetBytes(message);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            data = new Byte[256];

            // Read the first batch of the TcpServer response bytes.
            int bytesRead = stream.Read(data, 0, data.Length);
            var responseStr = encoding.GetString(data, 0, bytesRead);
            return responseStr;
        }
    }
}