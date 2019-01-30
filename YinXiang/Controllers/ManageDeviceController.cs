using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        public ActionResult DeleteDevice(int id)
        {
            return Redirect("index");
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
                Port = device.Port,
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

        public ActionResult EditDeviceInto(int id)
        {
            var deviceInfo = ApplicationContext.DeviceInfos.Where(m => m.Id == id).FirstOrDefault();
            if (deviceInfo == null)
            {
                deviceInfo = new DeviceInfo();
            }
            DeviceInfoDto deviceInfoDto = new DeviceInfoDto();
            TypeHelp.ObjCopy(deviceInfo, deviceInfoDto);
            var deviceAccount = ApplicationContext.DeviceAccounts.Where(m => m.Id == id).FirstOrDefault();
            if (deviceAccount != null)
            {
                deviceInfoDto.UserId = deviceAccount.UserId;
            }
            var users = UserManager.Users;
            var userSelectItems = users.Select(u => new SelectListItem()
            {
                Text = u.UserName,
                Value = u.Id,
                Selected = deviceInfoDto.UserId == u.Id,
            });
            deviceInfoDto.AccountItems = userSelectItems;
            IList<SelectListItem> deviceTypeItems = new List<SelectListItem>();
            foreach (int i in Enum.GetValues(typeof(DeviceType)))
            {
                deviceTypeItems.Add(new SelectListItem()
                {
                    Text = Enum.GetName(typeof(DeviceType), i),
                    Value = i + "",
                    Selected = (int)deviceInfoDto.Type == i,
                });
            }
            deviceInfoDto.DeviceTypeItems = deviceTypeItems.AsEnumerable();
            return View(deviceInfoDto);
        }

        [HttpPost]
        public ActionResult EditDevice(DeviceInfoDto device)
        {
            var deviceInfo = new DeviceInfo();
            TypeHelp.ObjCopy(device, deviceInfo);

            using (var transaction = ApplicationContext.Database.BeginTransaction())
            {
                try
                {
                    ApplicationContext.Entry<DeviceInfo>(deviceInfo).State = EntityState.Modified;
                    ApplicationContext.SaveChanges();

                    var deviceAccount = ApplicationContext.DeviceAccounts.Where(m => m.Id == device.Id).FirstOrDefault();
                    if (deviceInfo != null)
                    {
                        deviceAccount.UserId = device.UserId;
                        ApplicationContext.Entry<DeviceAccount>(deviceAccount).State = EntityState.Modified;
                        ApplicationContext.SaveChanges();
                    };
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