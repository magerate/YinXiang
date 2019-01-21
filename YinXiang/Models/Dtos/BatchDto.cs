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
        public string sort { get; set; }
        public string sortdir { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }
    public class SendBatchDto
    {
        public string BatchNo { get; set; }
        public string DeviceName { get; set; }
        public string IP { get; set; }
        public string Account { get; set; }
    }
    public class BatchDeviceSearchDto : SendBatchDto
    {
        public DateTime? CreateTime { get; set; }
        public string sort { get; set; }
        public string sortdir { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }
    public class SendBatchStockDto
    {
        public string batchNo { get; set; }
        public int totalNumber { get; set; }
    }
    public class SendBatchDeviceHistoryDto
    {
        public int Id { get; set; }
        public string BatchNo { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public DateTime BatchDate { get; set; } = DateTime.Now;
        public string DeviceName { get; set; }
        public string IP { get; set; }
        public string Account { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
    public class UpdateBatchStockHistoryDto
    {
        public int Id { get; set; }
        public string BatchNo { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public DateTime BatchDate { get; set; } = DateTime.Now;
        public int TotalNumber { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}