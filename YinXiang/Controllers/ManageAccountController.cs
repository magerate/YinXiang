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
    public class ManageAccountController : Controller
    {
        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        public ApplicationDbContext ApplicationContext
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }

        }

        // GET: ManageAccount
        public ActionResult Index()
        {
            IEnumerable<ApplicationUser> model = ApplicationContext.Users;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAccount(CreateAccountViewModel model)
        {
            var user = new ApplicationUser();
            user.UserName = model.UserName;

            var result = UserManager.Create(user, model.Password);
            return Redirect("index");
        }
    }
}