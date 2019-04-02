using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YinXiang
{
    public class iMarkClient
    {
        public TcpClient TcpClient { get; private set; } = new TcpClient();
        public Encoding Encoding { get; set; } = Encoding.ASCII;
        public int EndChar { get; set; } = 3;

        //sucess response is 8 \b
        public static readonly string ResponseString = "\b";

        public async Task<string> SendAsync(string message)
        {
            var endChar = (char)EndChar;
            var msg = message + new string(new[] { endChar });

            Byte[] data = Encoding.GetBytes(msg);
            NetworkStream stream = TcpClient.GetStream();
            await stream.WriteAsync(data, 0, data.Length);

            data = new Byte[256];

            // Read the first batch of the TcpServer response bytes.
            int bytesRead = await stream.ReadAsync(data, 0, data.Length);
            var responseStr = Encoding.GetString(data, 0, bytesRead);
            return responseStr;
        }
    }
}