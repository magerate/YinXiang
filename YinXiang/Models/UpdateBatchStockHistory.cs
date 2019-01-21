using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YinXiang.Models
{
    public class UpdateBatchStockHistory
    {
        public int Id { get; set; }
        public string BatchNo { get; set; }
        public int TotalNumber { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}