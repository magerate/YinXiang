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

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
        }

        // GET: ManageAccount
        public ActionResult Index()
        {
            IEnumerable<ApplicationUser> model = ApplicationContext.Users;
            var roles = ApplicationContext.Roles.Where(m=>m.Name=="Admin").OrderBy(m => m.Name).ToList();
            ViewBag.RoleList = new SelectList(roles, "Name", "Name"); ;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAccount(CreateAccountViewModel model)
        {
            var user = new ApplicationUser();
            user.UserName = model.UserName;
            user.BindingIp = model.BindingIp;

            var result = UserManager.Create(user, model.Password);
            if (result.Succeeded)
            {
                UserManager.AddToRole(user.Id, string.IsNullOrEmpty(model.SelectedRole)?"Owner": model.SelectedRole);
            }
            return Redirect("index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePwdViewModel model)
        {
            var result = UserManager.ChangePassword(model.Id, model.OldPassword, model.NewPassword);
            return Redirect("index");
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var user = UserManager.FindById(id);
            var result = UserManager.Delete(user);
            if (!result.Succeeded)
            {
                return Content("删除失败");
            }
            return Content("删除成功");
        }

    }
}