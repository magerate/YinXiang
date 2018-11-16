using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YinXiang.Models;

namespace YinXiang.Controllers
{
    public class BatchController : Controller
    {
        // GET: Batch
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestBatchCode(BatchInfo batchInfo)
        {
            return Redirect("index");
            //return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManualInput(BatchInfo batchInfo)
        {
            return Redirect("index");
            //return View();
        }
    }
}