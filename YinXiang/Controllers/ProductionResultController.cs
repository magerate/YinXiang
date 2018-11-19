using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using YinXiang.Models;

namespace YinXiang.Controllers
{
    public class ProductionResultController : Controller
    {
        public ApplicationDbContext ApplicationContext
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }

        }

        // GET: ProductionResult
        public ActionResult Index()
        {
            var model = ApplicationContext.ProductionInfos;
            return View(model);
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    var stream = file.InputStream;
                    using (var reader = new StreamReader(stream))
                    {
                        var firstLine = reader.ReadLine();
                        var pi = new ProductionInfo()
                        {
                            Title = firstLine,
                        };
                        ApplicationContext.ProductionInfos.Add(pi);
                        ApplicationContext.SaveChanges();
                        return Redirect("index");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View();
        }
    }
}