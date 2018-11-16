using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YinXiang.Models
{
    public class BatchInfo
    {
        [Display(Name = "ProductType")]
        public string ProductType { get; set; }

        [Display(Name = "Source")]
        public string Source { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }
    }
}