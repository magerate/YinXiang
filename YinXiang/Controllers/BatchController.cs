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
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        // GET: Batch
        public ActionResult Index(string batchDate = "")
        {
            ViewBag.BatchDate = string.IsNullOrEmpty(batchDate) ? DateTime.Now.Date : Convert.ToDateTime(batchDate).Date;
            ViewBag.IsLoad = !string.IsNullOrEmpty(batchDate);
            return View();
        }

        public ActionResult List(BatchSearchDto search)
        {
            var batchDtos = GetBatchDtos(search, ApplicationContext);
            ViewBag.TotalRowCounts = batchDtos.Count;
            return View(batchDtos);
        }

        public IList<BatchDto> GetBatchDtos(BatchSearchDto search, ApplicationDbContext applicationDbContext)
        {
            if (search == null)
            {
                search = new BatchSearchDto();
            }
            if (!search.batchDate.HasValue)
            {
                search.batchDate = DateTime.Now;
            }
            DateTime startBatchDate = search.batchDate.Value.Date;
            DateTime endBatchDate = startBatchDate.AddDays(1).AddSeconds(-1);
            ApiSetting apiSetting = applicationDbContext.ApiSettings.FirstOrDefault() ?? new ApiSetting();
            if (apiSetting.Id == 0)
            {
                apiSetting.ApiUrl = "http://188.188.1.5:8080/jeecg/ycProductionController.do";
            }
            var client = new RestClient(apiSetting.ApiUrl + "?getListByBatchDate&batchDate=" + startBatchDate.ToString("yyyy-MM-dd"));
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
            {
                return new List<BatchDto>();
            }
            var content = response.Content;
            BatchResultDto batchResultDto = JsonHelp.ToObj<BatchResultDto>(response.Content);
            IList<BatchInfo> batchInfoDatas = new List<BatchInfo>();
            foreach (var item in batchResultDto.obj)
            {
                var oldItem = applicationDbContext.BatchInfos.Where(m => m.RetrospectNo == item.retrospectNo).FirstOrDefault();
                if (oldItem == null)
                {
                    BatchInfo batchInfo = new BatchInfo();
                    batchInfo.BatchNo = item.batchNo;
                    batchInfo.SKU = item.sku;
                    batchInfo.ProductName = item.name;
                    batchInfo.BatchDate = item.batchDate;
                    batchInfo.CreateDate = item.createDate;
                    batchInfo.RetrospectNo = item.retrospectNo;
                    applicationDbContext.Entry<BatchInfo>(batchInfo).State = EntityState.Added;
                }
                else
                {
                    oldItem.BatchNo = item.batchNo;
                    oldItem.SKU = item.sku;
                    oldItem.ProductName = item.name;
                    oldItem.BatchDate = item.batchDate;
                    oldItem.CreateDate = item.createDate;
                    oldItem.RetrospectNo = item.retrospectNo;
                    applicationDbContext.Entry<BatchInfo>(oldItem).State = EntityState.Modified;
                }
                applicationDbContext.SaveChanges();
                //item.IsSent = false;
                item.IsSent = applicationDbContext.SendBatchDeviceHistories.Any(m => m.RetrospectNo == item.retrospectNo);
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
            batchResultDto.obj = batchResultDto.obj.Skip(((search.page > 0 ? search.page : 1) - 1) * search.pageSize).Take(search.pageSize).ToList();         
            return batchResultDto.obj;
        }

        [HttpPost]
        public async Task<ActionResult> SendBatchNoes(SendBatchDto sendBatchDto)
        {
            //SendBatchDeviceHistory sendBatchDeviceHistory = ApplicationContext.SendBatchDeviceHistories.Where(m => m.BatchNo == sendBatchDto.BatchNo).FirstOrDefault();
            //if (sendBatchDeviceHistory != null)
            //    return Content("此批次码不能重复发送！");

            var device = ApplicationContext.GetDeviceByUserId(User.Identity.GetUserId());
            if (null == device)
            {
                return Content("该账号还未绑定打码设备");
            }
            sendBatchDto.IP = device.IP;
            sendBatchDto.DeviceName = device.Name;
            sendBatchDto.Account = User.Identity.Name;
            if (device.Type == DeviceType.X30)
            {
                try
                {
                    var client = new X30Client();
                    await client.ConnectAsync(device.IP);
                    var jobCommand = JobCommand.CreateJobUpdate();
                    jobCommand.Fields.Add(device.JobFieldName, "02"+sendBatchDto.RetrospectNo+"03");
                    await client.UpdateJob(jobCommand);
                    client.TcpClient.Close();
                    return SendSucess(sendBatchDto);
                }
                catch (Exception e)
                {
                    return Content($"发送失败--{e.Message}");
                }
            }
            else if (device.Type == DeviceType.iMark)
            {
                try
                {
                    //var tcpClient = new System.Net.Sockets.TcpClient();
                    //tcpClient.Connect("188.188.5.5", 9069);
                    //var str = "blah";
                    //var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(str);
                    //tcpClient.GetStream().Write(bytes, 0, bytes.Length);


                    //client.TcpClient.Connect("188.188.4.232", 9069);
                    //var tcpClient = new System.Net.Sockets.TcpClient();
                    //tcpClient.Connect("188.188.4.232", 9069);
                    //tcpClient.Close();

                    var client = new iMarkClient();
                    client.TcpClient.ReceiveTimeout = 500;
                    client.TcpClient.SendTimeout = 500;

                    client.TcpClient.Connect(device.IP, device.Port);
                    //var response = client.Send("02" + sendBatchDto.RetrospectNo + "," + sendBatchDto.PrintCount + "03");

                    var content = $"{sendBatchDto.RetrospectNo},{sendBatchDto.PrintCount}";
                    var response = client.Send(content);
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
            sendBatchDeviceHistory.RetrospectNo = sendBatchDto.RetrospectNo;
            sendBatchDeviceHistory.DeviceName = sendBatchDto.DeviceName;
            sendBatchDeviceHistory.IP = sendBatchDto.IP;
            sendBatchDeviceHistory.Account = sendBatchDto.Account;
            sendBatchDeviceHistory.PrintCount = sendBatchDto.PrintCount;
            ApplicationContext.SendBatchDeviceHistories.Add(sendBatchDeviceHistory);
            ApplicationContext.SaveChanges();
            return Content("发送成功");
        }

        [HttpPost]
        public ActionResult UploadPrintInfo(UploadPrintDto uploadPrintDto)
        {
            if (uploadPrintDto == null || string.IsNullOrEmpty(uploadPrintDto.BatchNo)
                || string.IsNullOrEmpty(uploadPrintDto.IP))
            {
                return Content("BatchNo和IP都不能为空");
            }
            try
            {
                var entity = new PrintBatchHistory();
                entity.BatchNo = uploadPrintDto.BatchNo;
                entity.IP = uploadPrintDto.IP;
                ApplicationContext.PrintBatchHistories.Add(entity);
                ApplicationContext.SaveChanges();
                return Content("上传成功");
            }
            catch (Exception ex)
            {
                return Content($"上传失败--{ex.Message}");
            }
        }
    }
}