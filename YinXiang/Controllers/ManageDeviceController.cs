using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using YinXiang.Models;

namespace YinXiang.Controllers
{
    public class ManageDeviceController : Controller
    {
        public ApplicationDbContext ApplicationContext
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }

        }

        // GET: ManageDevice
        public ActionResult Index()
        {
            var model = ApplicationContext.DeviceInfos;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDevice(DeviceInfo device)
        {
            ApplicationContext.DeviceInfos.Add(device);
            ApplicationContext.SaveChanges();
            return Redirect("index");
        }
    }
}