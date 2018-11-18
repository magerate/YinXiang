using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YinXiang.Models
{
    public class DeviceInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public string Account { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}