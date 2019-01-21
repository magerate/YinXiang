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
            var dataList = ApplicationContext.SendBatchDeviceHistories.ToList();
            if (search.CreateTime.HasValue)
            {
                DateTime startBatchDate = search.CreateTime.Value.Date;
                DateTime endBatchDate = startBatchDate.AddDays(1).AddSeconds(-1);
                dataList.Where(m => m.CreateTime >= startBatchDate && m.CreateTime <= endBatchDate);
            }
            if (!string.IsNullOrEmpty(search.BatchNo))
            {
                dataList = dataList.Where(m => m.BatchNo.Contains(search.BatchNo)).ToList();
            }
            if (!string.IsNullOrEmpty(search.DeviceName))
            {
                dataList = dataList.Where(m => m.DeviceName.Contains(search.DeviceName)).ToList();
            }
            search.sort = string.IsNullOrEmpty(search.sort) ? "BatchNo" : search.sort;
            search.sortdir = string.IsNullOrEmpty(search.sortdir) ? "ASC" : search.sortdir;
            if (search.sort == "DeviceName")
            {
                dataList = (search.sortdir == "DESC" ? dataList.OrderByDescending(m => m.DeviceName) : dataList.OrderBy(m => m.DeviceName)).ToList();
            }
            else if (search.sort == "Account")
            {
                dataList = (search.sortdir == "DESC" ? dataList.OrderByDescending(m => m.Account) : dataList.OrderBy(m => m.Account)).ToList();
            }
            else if (search.sort == "IP")
            {
                dataList = (search.sortdir == "DESC" ? dataList.OrderByDescending(m => m.IP) : dataList.OrderBy(m => m.IP)).ToList();
            }
            else if (search.sort == "CreateTime")
            {
                dataList = (search.sortdir == "DESC" ? dataList.OrderByDescending(m => m.CreateTime) : dataList.OrderBy(m => m.CreateTime)).ToList();
            }
            else
            {
                dataList = (search.sortdir == "DESC" ? dataList.OrderByDescending(m => m.BatchNo) : dataList.OrderBy(m => m.BatchNo)).ToList();
            }
            ViewBag.TotalRowCounts = dataList.Count;
            dataList = dataList.Skip(((search.page > 0 ? search.page : 1) - 1) * search.pageSize).Take(search.pageSize).ToList();

            IList<SendBatchDeviceHistoryDto> Model = new List<SendBatchDeviceHistoryDto>();
            foreach (var item in dataList)
            {
                SendBatchDeviceHistoryDto entity = new SendBatchDeviceHistoryDto();
                var batchItem = ApplicationContext.BatchInfos.Where(m => m.BatchNo == item.BatchNo).FirstOrDefault();
                if (batchItem != null)
                {
                    TypeHelp.ObjCopy(batchItem, entity);
                }
                TypeHelp.ObjCopy(item, entity);
                Model.Add(entity);
            }
            return View(Model);
        }
    }
}