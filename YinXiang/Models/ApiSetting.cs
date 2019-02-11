using System;

namespace YinXiang.Models
{
    public class ApiSetting
    {
        public int Id { get; set; }
        public string ApiUrl { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}