using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace YinXiang
{
    //Print Status Request
    //~PS|0|

//   5.21 Print Status Response
//Sent by NGE device to report if device head is ready to print
//Response Field Field Length Notes
//Message Identifier ~PR
//Message Body
//Status 1 character Optional
//Print Status 1 character
//0 – Ready to print
//1 – Unable to print, incorrect state
//2 – Unable to print, printing
//3 – Unable to print, fault
//Example
//~PR0|
//~PR0|0| (if ‘Status’ field enabled)

    public enum PrinterStatus
    {
        ReadyToPrint = 0,
        IncorrectState = 1,
        Printing = 2,
        Fault = 3
    }


    //    Message Identifier ~DV
    //Message Body
    //Status 1 character Optional
    //Clear to Send status 1 character 0 – Ready to accept new data
    //1 – Not ready to accept new data
    //Example
    //~DV1|
    //~DV0|1| (if ‘Status’ field enabled)
    //Note: If the NextGen device is not in the Producing state this response will be returned immediately
    //(whatever the value of the ‘Response Timing’ field in the request) and report ‘Not Ready’.
    public enum ClearSendStatus
    {
        Ready = 0,
        NotReady = 1
    }

    public class X30Client
    {
        private const int PORT = 21000;
        public TcpClient TcpClient { get; private set; }

        public Encoding Encoding { get; private set; } = Encoding.ASCII;

        public X30Client()
        {
            TcpClient = new TcpClient();
        }

        public Task ConnectAsync(string host)
        {
            return TcpClient.ConnectAsync(host, PORT);
        }

        public async Task<ClearSendStatus> GetClearSendStatusAsync()
        {
            var packet = "{~DC0|}";
            var response = await SendAsync(TcpClient, packet, Encoding);

            //sample response {~DV0|1|}
            if (response.StartsWith("{~DV0|") && response.Length > 6)
            {
                var statusChar = response[6];
                int statusValue = Convert.ToInt32(statusChar) - Convert.ToInt32('0');
                if (statusValue >= 0 && statusValue <= 1)
                {
                    return (ClearSendStatus)statusValue;
                }
            }
            throw new InvalidDataException();
        }

        public async Task StateChangeAsync()
        {
            //~ST | 04 |
            var packet = "{~ST|04|}";
            var response = await SendAsync(TcpClient, packet, Encoding);

            //if (response.StartsWith("{~DV0|") && response.Length > 6)
            //{
            //    var statusChar = response[6];
            //    int statusValue = Convert.ToInt32(statusChar) - Convert.ToInt32('0');
            //    if (statusValue >= 0 && statusValue <= 1)
            //    {
            //        return (ClearSendStatus)statusValue;
            //    }
            //}
            //throw new InvalidDataException();
        }

        public async Task<PrinterStatus> GetPrinterStatusAsync()
        {
            var packet = "{~PS0|}";
      
            var response = await SendAsync(TcpClient, packet, Encoding);

            //sample response {~PR0|0|}
            if(response.StartsWith("{~PR0|") && response.Length > 6)
            {
                var statusChar = response[6];
                int statusValue = Convert.ToInt32(statusChar) - Convert.ToInt32('0');
                if (statusValue >= 0 && statusValue <= 3)
                {
                    return (PrinterStatus)statusValue;
                }
            }
            throw new InvalidDataException();
        }

        public async Task UpdateJob(JobCommand command)
        {
            var packet = command.ToPacket();
            var response = await SendAsync(TcpClient, packet, Encoding);
            if(response != "{~JU0|}")
            {
                throw new InvalidOperationException();
            }
        }

        private async Task<string> SendAsync(TcpClient tcpClient, string packet, Encoding encoding)
        {
            var stream = tcpClient.GetStream();
            var bytes = encoding.GetBytes(packet);
            await stream.WriteAsync(bytes, 0, bytes.Length);

            var bufferSize = 1024 * 8;
            var readerBuffer = new byte[bufferSize];
            var count = await stream.ReadAsync(readerBuffer, 0, bufferSize);
            var text = encoding.GetString(readerBuffer, 0, count);
            return text;
        }
    }
}
