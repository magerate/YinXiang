using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;

using YinXiang.Models;
using RestSharp;
using YinXiang.Models.Dtos;
using System.Data.Entity;

namespace YinXiang.Controllers
{
    public class BatchController : Controller
    {
        public ApplicationDbContext ApplicationContext
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }
            
        }

        // GET: Batch
        public ActionResult Index()
        {
            ViewBag.BatchDate = DateTime.Now.Date;
            return View();
        }

        public ActionResult List(BatchSearchDto search)
        {
            if (search == null)
            {
                search = new BatchSearchDto();
            }
            if (!search.batchDate.HasValue)
                search.batchDate = DateTime.Now;
            DateTime startBatchDate = search.batchDate.Value.Date;
            DateTime endBatchDate = startBatchDate.AddDays(1).AddSeconds(-1);
            var client = new RestClient("http://x97700.iok.la:32611/ycProductionController.do?getListByBatchDate&batchDate="+ startBatchDate.ToString("yyyy-MM-dd"));
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
            {
                return View(new List<BatchDto>());
            }
            var content = response.Content;
            BatchResultDto batchResultDto = JsonHelp.ToObj<BatchResultDto>(response.Content);
            IList<BatchInfo> batchInfoDatas= new List<BatchInfo>();            
            foreach (var item in batchResultDto.obj)
            {
                var oldItem= ApplicationContext.BatchInfos.Where(m=>m.BatchNo==item.batchNo).FirstOrDefault();
                if (oldItem == null)
                {
                    BatchInfo batchInfo = new BatchInfo();
                    batchInfo.BatchNo = item.batchNo;
                    batchInfo.SKU = item.sku;
                    batchInfo.ProductName = item.name;
                    batchInfo.BatchDate = item.batchDate;
                    batchInfo.CreateDate = item.createDate;
                    batchInfo.RetrospectNo = item.retrospectNo;
                    ApplicationContext.Entry<BatchInfo>(batchInfo).State = EntityState.Added;
                }
                else
                {
                    oldItem.BatchNo = item.batchNo;
                    oldItem.SKU = item.sku;
                    oldItem.ProductName = item.name;
                    oldItem.BatchDate = item.batchDate;
                    oldItem.CreateDate = item.createDate;
                    oldItem.RetrospectNo = item.retrospectNo;
                    ApplicationContext.Entry<BatchInfo>(oldItem).State = EntityState.Modified;
                }
                ApplicationContext.SaveChanges();
                item.IsSent = ApplicationContext.SendBatchDeviceHistories.Any(m => m.BatchNo == item.batchNo);
            }
            batchResultDto.obj.Where(m => m.batchDate >= startBatchDate && m.batchDate <= endBatchDate);
            if (!string.IsNullOrEmpty(search.batchNo))
            {
                batchResultDto.obj = batchResultDto.obj.Where(m => m.batchNo.Contains(search.batchNo)).ToList();
            }
            if (!string.IsNullOrEmpty(search.name))
            {
                batchResultDto.obj = batchResultDto.obj.Where(m => m.name.Contains(search.name)).ToList();
            }
            if (!string.IsNullOrEmpty(search.sku))
            {
                batchResultDto.obj= batchResultDto.obj.Where(m => m.sku.Contains(search.sku)).ToList();
            }
            return View(batchResultDto.obj);
        }

        [HttpPost]
        public ActionResult SendBatchNoes(SendBatchDto sendBatchDto)
        {
            SendBatchDeviceHistory sendBatchDeviceHistory= ApplicationContext.SendBatchDeviceHistories.Where(m => m.BatchNo == sendBatchDto.BatchNo).FirstOrDefault();
            if(sendBatchDeviceHistory!=null)
                return Content("此批次码不能重复发送！");
            sendBatchDeviceHistory = new SendBatchDeviceHistory();
            sendBatchDeviceHistory.BatchNo = sendBatchDto.BatchNo;
            sendBatchDeviceHistory.DeviceName = sendBatchDto.DeviceName;
            sendBatchDeviceHistory.IP = sendBatchDto.IP;
            sendBatchDeviceHistory.Account = sendBatchDto.Account;
            ApplicationContext.SendBatchDeviceHistories.Add(sendBatchDeviceHistory);
            ApplicationContext.SaveChanges();
            return Content("发送成功");
        }
    }
}