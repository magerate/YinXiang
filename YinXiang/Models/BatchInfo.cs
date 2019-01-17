using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace YinXiang.Models
{
    public class BatchInfo
    {
        public int Id { get; set; }
        [Display(Name = "BatchNo")]
        public string BatchNo { get; set; }

        [Display(Name = "ProductName")]
        public string ProductName { get; set; }

        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [Display(Name = "BatchDate")]
        public DateTime BatchDate { get; set; } = DateTime.Now;

        [Display(Name = "CreateDate")]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "RetrospectNo")]
        public string RetrospectNo { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

    }
}