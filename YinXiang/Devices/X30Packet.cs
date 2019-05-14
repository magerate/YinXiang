using System;
using System.Text;
using System.Collections.Generic;

namespace YinXiang
{
    public class X30Packet
    {
        public const string ProtocolVersionRequest = "~PV";

        //default
        public static char PacketStart = '{';
        public static char Delimiter = '|';
        public static char MessageStart = '~';
        public static char CheckFieldStart = '|';
        public static char PacketEnd = '}';

        //traditional 
        //STX 2
        //public static char PacketStart = (char)2;
        //public static char Delimiter = '|';
        //public static char MessageStart = '~';
        //public static char CheckFieldStart = '|';
        ////ETX 3
        //public static char PacketEnd = (char)3;


        //legacy
        //public static char PacketStart = (char)1;
        //public static char Delimiter = '|';
        //public static char MessageStart = '^';
        //public static char CheckFieldStart = '-';
        //public static char PacketEnd = (char)26;


        //The Message Number is an optional fixed length field.
        //It may be logged by the NGE device and used to track job selects.It shall be 4 numeric characters.
        //If it is included in a message to an NGE device it shall be sent in the response to that message.
        public string MessageNumber = null;

        //The ‘Message Identifier’ shall be a fixed length, (3 - character) sequence.
        //The sequence shall always start with the same Message Identifier character.
        public string MessageId = null;

        //The size of the Message Body shall be dictated according to message type(as denoted by the Identifier).
        //A command message does not always require a Message Body.A Response or Reply message shall always have a body.
        //A packet shall only contain one message.
        public string MessageBody = null;
    }

    //~NK1|
    //~JU
    //~FR|Field1|


    //default to job update
    public class JobCommand
    {
        public static JobCommand CreateJobUpdate()
        {
            return new JobCommand()
            {
                Id = "JU",
                ReplyTiming = '0',
                Allocation = "001"
            };
        }

        public string Id { get; set; } = "JU";
        public char ReplyTiming { get; set; } = '0';
        //empty means update current job
        public string JobName { get; set; }
        public string Allocation { get; set; } = "001";
        public Dictionary<string, string> Fields { get; private set; } = new Dictionary<string, string>();

        public string ToPacket()
        {
            //if (string.IsNullOrWhiteSpace(JobName))
            //{
            //    throw new InvalidOperationException();
            //}

            var sb = new StringBuilder();
            sb.Append(X30Packet.PacketStart);
            sb.Append(X30Packet.MessageStart);

            sb.Append(Id);
            sb.Append(ReplyTiming);
            sb.Append(X30Packet.Delimiter);
            if(!string.IsNullOrEmpty(JobName))
            {
                sb.Append(JobName);
            }
            sb.Append(X30Packet.Delimiter);

            if (!string.IsNullOrWhiteSpace(Allocation) && Fields.Count > 0)
            {
                sb.Append(Allocation);

                foreach (var pair in Fields)
                {
                    sb.Append(X30Packet.Delimiter);
                    sb.Append(pair.Key);
                    sb.Append(X30Packet.Delimiter);
                    sb.Append(pair.Value);
                }
                sb.Append(X30Packet.Delimiter);
            }
            sb.Append(X30Packet.PacketEnd);
            return sb.ToString();
        }
    }
}
