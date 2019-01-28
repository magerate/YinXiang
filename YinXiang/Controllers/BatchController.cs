using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;

using YinXiang.Models;
using RestSharp;
using YinXiang.Models.Dtos;


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

        public ApplicationUserManager UserManager
        {
            get
            {
                return  HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
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
            var client = new RestClient("http://x97700.iok.la:32611/ycProductionController.do?getListByBatchDate&batchDate=" + startBatchDate.ToString("yyyy-MM-dd"));
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
            {
                return View(new List<BatchDto>());
            }
            var content = response.Content;
            BatchResultDto batchResultDto = JsonHelp.ToObj<BatchResultDto>(response.Content);
            IList<BatchInfo> batchInfoDatas = new List<BatchInfo>();
            foreach (var item in batchResultDto.obj)
            {
                var oldItem = ApplicationContext.BatchInfos.Where(m => m.BatchNo == item.batchNo).FirstOrDefault();
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
                batchResultDto.obj = batchResultDto.obj.Where(m => m.sku.Contains(search.sku)).ToList();
            }
            search.sort = string.IsNullOrEmpty(search.sort) ? "batchNo" : search.sort;
            search.sortdir = string.IsNullOrEmpty(search.sortdir) ? "ASC" : search.sortdir;
            if (search.sort == "name")
            {
                batchResultDto.obj = (search.sortdir == "DESC" ? batchResultDto.obj.OrderByDescending(m => m.name) : batchResultDto.obj.OrderBy(m => m.name)).ToList();
            }
            else if (search.sort == "sku")
            {
                batchResultDto.obj = (search.sortdir == "DESC" ? batchResultDto.obj.OrderByDescending(m => m.sku) : batchResultDto.obj.OrderBy(m => m.sku)).ToList();
            }
            else if (search.sort == "batchDate")
            {
                batchResultDto.obj = (search.sortdir == "DESC" ? batchResultDto.obj.OrderByDescending(m => m.batchDate) : batchResultDto.obj.OrderBy(m => m.batchDate)).ToList();
            }
            else if (search.sort == "createDate")
            {
                batchResultDto.obj = (search.sortdir == "DESC" ? batchResultDto.obj.OrderByDescending(m => m.createDate) : batchResultDto.obj.OrderBy(m => m.createDate)).ToList();
            }
            else
            {
                batchResultDto.obj = (search.sortdir == "DESC" ? batchResultDto.obj.OrderByDescending(m => m.batchNo) : batchResultDto.obj.OrderBy(m => m.batchNo)).ToList();
            }
            ViewBag.TotalRowCounts = batchResultDto.obj.Count;
            batchResultDto.obj = batchResultDto.obj.Skip(((search.page > 0 ? search.page : 1) - 1) * search.pageSize).Take(search.pageSize).ToList();
            return View(batchResultDto.obj);
        }

        [HttpPost]
        public async Task<ActionResult> SendBatchNoes(SendBatchDto sendBatchDto)
        {
            SendBatchDeviceHistory sendBatchDeviceHistory = ApplicationContext.SendBatchDeviceHistories.Where(m => m.BatchNo == sendBatchDto.BatchNo).FirstOrDefault();
            if (sendBatchDeviceHistory != null)
                return Content("此批次码不能重复发送！");

            var device = ApplicationContext.GetDeviceByUserId(User.Identity.GetUserId());
            if(null == device)
            {
                return Content("该账号还未绑定打码设备");
            }

            if(device.Type == DeviceType.X30)
            {
                try
                {
                    var client = new X30Client();
                    await client.ConnectAsync(device.IP);
                    var jobCommand = JobCommand.CreateJobUpdate();
                    jobCommand.Fields.Add(device.JobFieldName, sendBatchDto.BatchNo);
                    await client.UpdateJob(jobCommand);
                    client.TcpClient.Close();
                    return SendSucess(sendBatchDto);
                }
                catch (Exception e)
                {
                    return Content($"发送失败--{e.Message}");
                }
            }else if(device.Type == DeviceType.iMark)
            {
                try
                {
                    var client = new iMarkClient();
                    await client.TcpClient.ConnectAsync(device.IP,device.Port);
                    await client.SendAsync(sendBatchDto.BatchNo);
                    client.TcpClient.Close();
                    return SendSucess(sendBatchDto);
                }
                catch (Exception e)
                {
                    return Content($"发送失败--{e.Message}");
                }
            }
            else
            {
                return Content($"设备类型配置错误 {device.Name} {device.Type}");
            }
        }

        private ContentResult SendSucess(SendBatchDto sendBatchDto)
        {
            var sendBatchDeviceHistory = new SendBatchDeviceHistory();
            sendBatchDeviceHistory.BatchNo = sendBatchDto.BatchNo;
            sendBatchDeviceHistory.DeviceName = sendBatchDto.DeviceName;
            sendBatchDeviceHistory.IP = sendBatchDto.IP;
            sendBatchDeviceHistory.Account = sendBatchDto.Account;
            ApplicationContext.SendBatchDeviceHistories.Add(sendBatchDeviceHistory);
            ApplicationContext.SaveChanges();
            return Content("发送成功");
        }

        [HttpPost]
        public ActionResult UpdateBatchStock(SendBatchStockDto entity)
        {
            var batchItem = ApplicationContext.BatchInfos.Where(m => m.BatchNo == entity.batchNo).FirstOrDefault();
            if (batchItem == null)
            {
                return Content("此批次码不存在");
            }
            var client = new RestClient("http://x97700.iok.la:32611/ycProductionController.do?inByBatchNo");
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
            updateBatchStockHistory.TotalNumber = entity.totalNumber;
            ApplicationContext.UpdateBatchStockHistories.Add(updateBatchStockHistory);
            ApplicationContext.SaveChanges();
            if (batchItem != null)
            {
                batchItem.Quantity = entity.totalNumber;
                ApplicationContext.Entry<BatchInfo>(batchItem).State = EntityState.Modified;
                ApplicationContext.SaveChanges();
            }
            return Content(response.Content);
        }
    }
}