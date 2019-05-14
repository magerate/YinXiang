using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using YinXiang.Models;
using System.Data.Entity;

namespace YinXiang.Controllers
{
    public class ApiSettingController : Controller
    {
        public ApplicationDbContext ApplicationContext
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }

        }

        // GET: ApiSetting
        public ActionResult Index()
        {
            ApiSetting entity = ApplicationContext.ApiSettings.FirstOrDefault() ?? new ApiSetting();
            if (entity.Id == 0)
            {
                entity.ApiUrl = "http://188.188.1.5:8080/jeecg/ycProductionController.do";
            }
            return View(entity);
        }

        [HttpPost]
        public ActionResult SaveApiSetting(ApiSetting entity)
        {
            if (string.IsNullOrEmpty(entity.ApiUrl))
            {
                return Content("Api地址必填");
            }
            try
            {
                if (entity.Id == 0)
                {
                    ApplicationContext.ApiSettings.Add(entity);
                }
                else
                {
                    ApplicationContext.Entry<ApiSetting>(entity).State = EntityState.Modified;
                }
                ApplicationContext.SaveChanges();

            }
            catch (Exception ex)
            {
                return Content($"保存失败--{ex.Message}");
            }
            return Content("保存成功");
        }
    }
}