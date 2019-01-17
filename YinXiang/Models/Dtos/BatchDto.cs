using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YinXiang.Models.Dtos
{
    public class BatchDto
    {
        public string name { get; set; }
        public string retrospectNo { get; set; }
        public DateTime? createDate { get; set; }
        public DateTime batchDate { get; set; }
        public string sku { get; set; }
        public string batchNo { get; set; }
        public bool IsSent { get; set; }
    }
    public class BatchResultDto
    {
        public string attributes { get; set; }
        public bool success { get; set; }
        public IList<BatchDto> obj { get; set; }
        public string jsonStr { get; set; }
        public string msg { get; set; }
    }
    public class BatchSearchDto
    {
        public string name { get; set; }
        public string retrospectNo { get; set; }
        public DateTime? createDate { get; set; }
        public DateTime? batchDate { get; set; }
        public string sku { get; set; }
        public string batchNo { get; set; }
    }
    public class SendBatchDto
    {
        public string BatchNo { get; set; }
        public string DeviceName { get; set; }
        public string IP { get; set; }
        public string Account { get; set; }
    }
    public class BatchDeviceSearchDto:SendBatchDto
    {
        public DateTime? CreateTime { get; set; }
    }
}