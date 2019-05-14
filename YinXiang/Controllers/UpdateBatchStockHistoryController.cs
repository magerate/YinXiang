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
    public class UpdateBatchStockHistoryController : Controller
    {
        public ApplicationDbContext ApplicationContext
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }

        }
        // GET: UpdateBatchStockHistory
        public ActionResult Index(string CreateTime = "")
        {
            ViewBag.CreateTime = string.IsNullOrEmpty(CreateTime) ? DateTime.Now.Date : Convert.ToDateTime(CreateTime).Date;
            return View();
        }

        public ActionResult List(BatchDeviceSearchDto search)
        {
            if (search == null)
            {
                search = new BatchDeviceSearchDto();
            }
            var dataList = ApplicationContext.UpdateBatchStockHistories.ToList();
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
            search.sort = string.IsNullOrEmpty(search.sort) ? "BatchNo" : search.sort;
            search.sortdir = string.IsNullOrEmpty(search.sortdir) ? "ASC" : search.sortdir;
            if (search.sort == "TotalNumber")
            {
                dataList = (search.sortdir == "DESC" ? dataList.OrderByDescending(m => m.TotalNumber) : dataList.OrderBy(m => m.TotalNumber)).ToList();
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

            IList<UpdateBatchStockHistoryDto> Model = new List<UpdateBatchStockHistoryDto>();
            foreach (var item in dataList)
            {
                UpdateBatchStockHistoryDto entity = new UpdateBatchStockHistoryDto();
                var batchItem = ApplicationContext.BatchInfos.Where(m => m.RetrospectNo == item.RetrospectNo).FirstOrDefault();
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