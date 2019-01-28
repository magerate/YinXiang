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

        // GET: ManageDevice
        public ActionResult Index()
        {
            var devices = ApplicationContext.DeviceInfos.ToArray();
            var deviceDtos = devices.Select(GetDeviceDto);

            var daIds = ApplicationContext.DeviceAccounts.Select(da => da.UserId);
            var users = UserManager.Users
                                .Where(u => !daIds.Contains(u.Id));
                                //.ToArray();
            var userSelectItems = users.Select(u => new SelectListItem()
            {
                Text = u.UserName,
                Value = u.Id,
            });

            var model = new ManageDeviceViewModel()
            {
                Devices = deviceDtos,
                AccountItems = userSelectItems,
            };
            return View(model);
        }

        private DeviceDto GetDeviceDto(DeviceInfo deviceInfo)
        {
            var deviceDto = new DeviceDto();
            deviceDto.Device = deviceInfo;

            var userId = ApplicationContext.DeviceAccounts.FirstOrDefault(da => da.Id == deviceInfo.Id).UserId;
            var user = UserManager.FindById(userId);
            deviceDto.User = user;
            return deviceDto;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDevice(CreateDeviceViewModel device)
        {
            var deviceInfo = new DeviceInfo()
            {
                Name = device.Name,
                Type = device.Type,
                IP = device.IP,
                JobFieldName = device.JobFieldName,
            };


            using (var transaction = ApplicationContext.Database.BeginTransaction())
            {
                try
                {
                    ApplicationContext.DeviceInfos.Add(deviceInfo);
                    ApplicationContext.SaveChanges();

                    var da = new DeviceAccount()
                    {
                        Id = deviceInfo.Id,
                        UserId = device.UserId,
                    };
                    ApplicationContext.DeviceAccounts.Add(da);
                    ApplicationContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }


           
            return Redirect("index");
        }
    }
}