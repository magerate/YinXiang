using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YinXiang.Models
{
    public class SendBatchDeviceHistory
    {
        public int Id { get; set; }
        public string BatchNo { get; set; }
        public string RetrospectNo { get; set; }
        public string DeviceName { get; set; }
        public string IP { get; set; }
        public string Account { get; set; }
        public int PrintCount { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    public class PrintBatchHistory
    {
        public int Id { get; set; }
        public string BatchNo { get; set; }
        public string RetrospectNo { get; set; }
        public string IP { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}