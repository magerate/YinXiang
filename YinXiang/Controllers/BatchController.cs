using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;

using YinXiang.Models;

namespace YinXiang.Controllers
{
    public class BatchController : Controller
    {
        public ApplicationDbContext ApplicationContext
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }
            
        }

        // GET: Batch
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestBatchCode(BatchInfo batchInfo)
        {
            batchInfo.Code = "picima 83928";
            batchInfo.Source = "网络请求";
            ApplicationContext.BatchInfos.Add(batchInfo);
            ApplicationContext.SaveChanges();
            return Redirect("index");
            //return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManualInput(BatchInfo batchInfo)
        {
            batchInfo.Source = "手动输入";
            ApplicationContext.BatchInfos.Add(batchInfo);
            ApplicationContext.SaveChanges();
            return Redirect("index");
            //return View();
        }
    }
}