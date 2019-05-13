using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YinXiang.Models;
using YinXiang.Models.Dtos;
using RestSharp;
using System.Data.Entity;

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
                var batchItem = ApplicationContext.BatchInfos.Where(m => m.RetrospectNo == item.RetrospectNo).FirstOrDefault();
                if (batchItem != null)
                {
                    TypeHelp.ObjCopy(batchItem, entity);
                }
                TypeHelp.ObjCopy(item, entity);
                entity.ScannedCounts = ApplicationContext.PrintBatchHistories.Where(m => m.RetrospectNo == entity.RetrospectNo).Count();
                Model.Add(entity);
            }
            return View(Model);
        }

        [HttpPost]
        public ActionResult UpdateBatchStock(SendBatchStockDto entity)
        {
            var batchItem = ApplicationContext.BatchInfos.Where(m => m.RetrospectNo == entity.retrospectNo).FirstOrDefault();
            if (batchItem == null)
            {
                return Content("此溯源码不存在");
            }
            ApiSetting apiSetting = ApplicationContext.ApiSettings.FirstOrDefault() ?? new ApiSetting();
            if (apiSetting.Id == 0)
            {
                apiSetting.ApiUrl = "http://x97700.iok.la:32611/ycProductionController.do";
            }
            var client = new RestClient(apiSetting.ApiUrl + "?inByBatchNo");
            var request = new RestRequest(Method.POST);
            request.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW",
                "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"batchNo\"\r\n\r\n"
                + batchItem.RetrospectNo
                + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"totalNum\"\r\n\r\n"
                + entity.totalNumber + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW--", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
            {
                return Content(response.Content);
            }
            var content = response.Content;
            BatchResultDto batchResultDto = JsonHelp.ToObj<BatchResultDto>(response.Content);
            if (!batchResultDto.success)
            {
                return Content(response.Content);
            }
            UpdateBatchStockHistory updateBatchStockHistory = new UpdateBatchStockHistory();
            updateBatchStockHistory.BatchNo = batchItem.BatchNo;
            updateBatchStockHistory.RetrospectNo = batchItem.RetrospectNo;
            updateBatchStockHistory.TotalNumber = entity.totalNumber;
            ApplicationContext.UpdateBatchStockHistories.Add(updateBatchStockHistory);
            ApplicationContext.SaveChanges();
            if (batchItem != null)
            {
                batchItem.RetrospectNo = entity.retrospectNo;
                batchItem.Quantity = entity.totalNumber;
                ApplicationContext.Entry<BatchInfo>(batchItem).State = EntityState.Modified;
                ApplicationContext.SaveChanges();
            }

            var sendBatchDeviceHistoryItem = ApplicationContext.SendBatchDeviceHistories.Where(m => m.RetrospectNo == entity.retrospectNo).FirstOrDefault();
            if (sendBatchDeviceHistoryItem != null)
            {
                ApplicationContext.SendBatchDeviceHistories.Remove(sendBatchDeviceHistoryItem);
                ApplicationContext.SaveChanges();
            }
            return Content(response.Content);
        }
    }
}