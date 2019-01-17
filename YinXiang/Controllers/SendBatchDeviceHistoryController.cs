using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YinXiang.Models;
using YinXiang.Models.Dtos;

namespace YinXiang.Controllers
{
    public class SendBatchDeviceHistoryController : Controller
    {
        public ApplicationDbContext ApplicationContext
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }

        }
        // GET: SendBatchDeviceHistory
        public ActionResult Index()
        {
            ViewBag.CreateTime = DateTime.Now.Date;
            return View();
        }

        public ActionResult List(BatchDeviceSearchDto search)
        {
            if (search == null)
            {
                search = new BatchDeviceSearchDto();
            }
            var Model = ApplicationContext.SendBatchDeviceHistories.ToList();
            if (!search.CreateTime.HasValue)
            {
                DateTime startBatchDate = search.CreateTime.Value.Date;
                DateTime endBatchDate = startBatchDate.AddDays(1).AddSeconds(-1);
                Model.Where(m => m.CreateTime >= startBatchDate && m.CreateTime <= endBatchDate);
            }
            if (!string.IsNullOrEmpty(search.BatchNo))
            {
                Model = Model.Where(m => m.BatchNo.Contains(search.BatchNo)).ToList();
            }
            if (!string.IsNullOrEmpty(search.DeviceName))
            {
                Model = Model.Where(m => m.DeviceName.Contains(search.DeviceName)).ToList();
            }
            return View(Model);
        }
    }
}