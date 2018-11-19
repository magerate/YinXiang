using System;

namespace YinXiang.Models
{
    public class ProductionInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}