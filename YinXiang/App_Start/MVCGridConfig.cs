using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCGrid.Models;
using MVCGrid.Web;
using YinXiang.Models;
using Microsoft.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using YinXiang.Models.Dtos;

namespace YinXiang
{
    public class MVCGridConfig
    {
        public static ApplicationDbContext ApplicationContext
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            }
        }

        public static void RegisterGrids()
        {
            ColumnDefaults colDefauls = new ColumnDefaults()
            {
                EnableSorting = true
            };

            #region ManageAccountGrid
            MVCGridDefinitionTable.Add("ManageAccountGrid", new MVCGridBuilder<ApplicationUser>(colDefauls)
                .AddColumns(cols =>
                {
                    cols.Add("UserName").WithHeaderText("用户名")
                        .WithValueExpression(p => p.UserName);
                    cols.Add("BindingIp").WithHeaderText("IP地址")
                        .WithValueExpression(p => p.BindingIp);
                    cols.Add("CreateTime").WithHeaderText("创建日期")
                        .WithValueExpression(p => p.CreateTime.ToString("d"));
                    cols.Add("Operate").WithHtmlEncoding(false)
                        .WithSorting(false)
                        .WithHeaderText(" ")
                        .WithValueExpression((p, c) => "<a class='btn' href='javascript:void(0);' data-toggle='modal' data-target='#changePassword' data-whatever='" + p.Id + "'>修改密码</a>"
                        + "<a class='btn' href='javascript:void(0);' onclick='deleteAccount(\"" + p.Id + "\")'>删除</a>");
                })
                .WithPreloadData(false)
                .WithSorting(true, "UserName")
                .WithPaging(true, 10)
                .WithPreviousButtonCaption("上一页")
                .WithNextButtonCaption("下一页")
                .WithSummaryMessage("<div style='display:none;'>{0}/{1}</div>总条数：{2}")
                .WithNoResultsMessage("")
                .WithProcessingMessage("加载中...")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;
                    string sortColumn = options.GetSortColumnData<string>();
                    var result = new QueryResult<ApplicationUser>();
                    using (var db = ApplicationContext)
                    {
                        var query = db.Users.AsQueryable();

                        result.TotalRecords = query.Count();

                        if (!String.IsNullOrWhiteSpace(options.SortColumnName))
                        {
                            switch (options.SortColumnName.ToLower())
                            {
                                case "username":
                                    query = query.OrderBy(p => p.UserName, options.SortDirection);
                                    break;
                                case "bindingip":
                                    query = query.OrderBy(p => p.BindingIp, options.SortDirection);
                                    break;
                                case "createtime":
                                    query = query.OrderBy(p => p.CreateTime, options.SortDirection);
                                    break;
                            }
                        }

                        if (options.GetLimitOffset().HasValue)
                        {
                            query = query.Skip(options.GetLimitOffset().Value).Take(options.GetLimitRowcount().Value);
                        }

                        result.Items = query.ToList();
                    }

                    return result;
                })
            );
            #endregion

            #region ManageDeviceGrid
            MVCGridDefinitionTable.Add("ManageDeviceGrid", new MVCGridBuilder<DeviceDto>(colDefauls)
                .AddColumns(cols =>
                {
                    cols.Add("Devices.Name").WithHeaderText("设备名称")
                        .WithValueExpression(p => p.Device.Name);
                    cols.Add("Devices.Type").WithHeaderText("设备类型")
                        .WithValueExpression(p => Enum.GetName(typeof(DeviceType), p.Device.Type));
                    cols.Add("Devices.IP").WithHeaderText("IP地址")
                        .WithValueExpression(p => p.Device.IP);
                    cols.Add("User.UserName").WithHeaderText("绑定账号")
                        .WithSorting(false)
                        .WithValueExpression(p => (p.User ?? new ApplicationUser()).UserName ?? "");
                    cols.Add("Device.CreateTime").WithHeaderText("添加日期")
                        .WithValueExpression(p => p.Device.CreateTime.ToString("d"));
                    cols.Add("Operate").WithHtmlEncoding(false)
                        .WithSorting(false)
                        .WithHeaderText(" ")
                        .WithValueExpression((p, c) => "<a class='btn' href='javascript:void(0);'onclick='openEditDevice(" + p.Device.Id + ")'>编辑</a>"
                        + "<a class='btn' href='javascript:void(0);'onclick='deleteDevice(" + p.Device.Id + ")'>删除</a>");
                })
                .WithPreloadData(false)
                .WithSorting(true, "Devices.Name")
                .WithPaging(true, 10)
                .WithPreviousButtonCaption("上一页")
                .WithNextButtonCaption("下一页")
                .WithSummaryMessage("<div style='display:none;'>{0}/{1}</div>总条数：{2}")
                .WithNoResultsMessage("")
                .WithProcessingMessage("加载中...")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;

                    string sortColumn = options.GetSortColumnData<string>();

                    var result = new QueryResult<DeviceDto>();
                    using (var db = ApplicationContext)
                    {
                        var devices = ApplicationContext.DeviceInfos.ToArray();

                        var query = devices.Select(GetDeviceDto).AsQueryable();

                        result.TotalRecords = query.Count();

                        if (!String.IsNullOrWhiteSpace(options.SortColumnName))
                        {
                            switch (options.SortColumnName.ToLower())
                            {
                                case "devices.name":
                                    query = query.OrderBy(p => p.Device.Name, options.SortDirection);
                                    break;
                                case "devices.ip":
                                    query = query.OrderBy(p => p.Device.IP, options.SortDirection);
                                    break;
                                case "devices.type":
                                    query = query.OrderBy(p => p.Device.Type, options.SortDirection);
                                    break;
                                //case "user.username":
                                //    query = query.OrderBy(p => p.User.UserName, options.SortDirection);
                                //    break;
                                case "devices.createtime":
                                    query = query.OrderBy(p => p.Device.CreateTime, options.SortDirection);
                                    break;
                            }
                        }

                        if (options.GetLimitOffset().HasValue)
                        {
                            query = query.Skip(options.GetLimitOffset().Value).Take(options.GetLimitRowcount().Value);
                        }

                        result.Items = query.ToList();
                    }

                    return result;
                })
            );
            #endregion

            #region BatchGrid
            MVCGridDefinitionTable.Add("BatchGrid", new MVCGridBuilder<BatchDto>(colDefauls)
                .AddColumns(cols =>
                {
                    cols.Add("batchNo").WithHeaderText("批次码")
                        .WithValueExpression(p => p.batchNo);
                    cols.Add("name").WithHeaderText("产品名称")
                        .WithValueExpression(p => p.name);
                    cols.Add("batchDate").WithHeaderText("日期")
                        .WithValueExpression(p => p.batchDate.ToString("yyyy-MM-dd"))
                        .WithFiltering(true);
                    cols.Add("createDate").WithHeaderText("创建时间")
                        .WithValueExpression(p => p.createDate.HasValue ? p.createDate.Value.ToString("yyyy-MM-dd HH:dd:ss") : "");
                    cols.Add("Operate").WithHtmlEncoding(false)
                        .WithSorting(false)
                        .WithHeaderText(" ")
                        .WithValueExpression((p, c) => p.IsSent ? "已发送" : ("<a id='btn_" + p.batchNo
                        + "' class='btn' href='javascript:void(0);'onclick='sendBatchNoToDevice(\""
                        + p.retrospectNo + "\")'>发送</a>"));
                })
                .WithFiltering(true)
                .WithPreloadData(false)
                .WithSorting(true, "batchNo")
                .WithPaging(true, 10)
                .WithPreviousButtonCaption("上一页")
                .WithNextButtonCaption("下一页")
                .WithSummaryMessage("<div style='display:none;'>{0}/{1}</div>总条数：{2}")
                .WithNoResultsMessage("")
                .WithProcessingMessage("加载中...")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;
                    string sortColumn = options.GetSortColumnData<string>();
                    var result = new QueryResult<BatchDto>();

                    string batchDate = options.GetFilterString("batchDate");
                    BatchSearchDto search = new BatchSearchDto();
                    search.batchDate = Convert.ToDateTime(batchDate);
                    var query = new Controllers.BatchController().GetBatchDtos(search, ApplicationContext).AsQueryable();

                    result.TotalRecords = query.Count();
                    if (!String.IsNullOrWhiteSpace(options.SortColumnName))
                    {
                        switch (options.SortColumnName.ToLower())
                        {
                            case "batchno":
                                query = query.OrderBy(p => p.batchNo, options.SortDirection);
                                break;
                            case "name":
                                query = query.OrderBy(p => p.name, options.SortDirection);
                                break;
                            case "batchdate":
                                query = query.OrderBy(p => p.batchDate, options.SortDirection);
                                break;
                            case "createdate":
                                query = query.OrderBy(p => p.createDate, options.SortDirection);
                                break;
                        }
                    }

                    if (options.GetLimitOffset().HasValue)
                    {
                        query = query.Skip(options.GetLimitOffset().Value).Take(options.GetLimitRowcount().Value);
                    }
                    result.Items = query.ToList();
                    return result;
                })
            );
            #endregion

            #region SendBatchDeviceHistoryGrid
            MVCGridDefinitionTable.Add("SendBatchDeviceHistoryGrid", new MVCGridBuilder<SendBatchDeviceHistoryDto>(colDefauls)
                .AddColumns(cols =>
                {
                    cols.Add("BatchNo").WithHeaderText("批次码")
                        .WithValueExpression(p => p.BatchNo);
                    cols.Add("ProductName").WithHeaderText("产品名称")
                        .WithValueExpression(p => p.ProductName);
                    cols.Add("SKU").WithHeaderText("产品规格")
                        .WithValueExpression(p => p.SKU);
                    cols.Add("BatchDate").WithHeaderText("日期")
                        .WithValueExpression(p => p.BatchDate != null ? p.BatchDate.ToString("yyyy-MM-dd") : "");
                    cols.Add("DeviceName").WithHeaderText("设备名称")
                        .WithValueExpression(p => p.DeviceName);
                    cols.Add("IP").WithHeaderText("ip地址")
                        .WithValueExpression(p => p.IP);
                    cols.Add("Account").WithHeaderText("绑定账号")
                        .WithValueExpression(p => p.Account);
                    cols.Add("CreateTime").WithHeaderText("时间")
                        .WithValueExpression(p => p.CreateTime!=null ? p.CreateTime.ToString("yyyy-MM-dd HH:dd:ss") : "");
                    cols.Add("ScannedCounts").WithHeaderText("扫描次数")
                        .WithValueExpression(p => p.ScannedCounts.ToString());
                    cols.Add("Operate").WithHtmlEncoding(false)
                        .WithSorting(false)
                        .WithHeaderText(" ")
                        .WithValueExpression((p, c) => "<a class='btn' href='javascript:void(0);' data-toggle='modal' "
                        +"data-target='#updateBatchStockModal' data-whatever='{\"BatchNo\":\"" + p.BatchNo 
                        + "\",\"ScannedCounts\":" + p.ScannedCounts + "}'>入库</a>");
                })
                .WithPreloadData(false)
                .WithSorting(true, "BatchNo")
                .WithPaging(true, 10)
                .WithPreviousButtonCaption("上一页")
                .WithNextButtonCaption("下一页")
                .WithSummaryMessage("<div style='display:none;'>{0}/{1}</div>总条数：{2}")
                .WithNoResultsMessage("")
                .WithProcessingMessage("加载中...")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;
                    string sortColumn = options.GetSortColumnData<string>();
                    var result = new QueryResult<SendBatchDeviceHistoryDto>();

                    BatchSearchDto search = new BatchSearchDto();
                    var query = GetSendBatchDeviceHistoryDtos().AsQueryable();

                    result.TotalRecords = query.Count();
                    if (!String.IsNullOrWhiteSpace(options.SortColumnName))
                    {
                        switch (options.SortColumnName.ToLower())
                        {
                            case "batchno":
                                query = query.OrderBy(p => p.BatchNo, options.SortDirection);
                                break;
                            case "productname":
                                query = query.OrderBy(p => p.ProductName, options.SortDirection);
                                break;
                            case "sku":
                                query = query.OrderBy(p => p.SKU, options.SortDirection);
                                break;
                            case "account":
                                query = query.OrderBy(p => p.Account, options.SortDirection);
                                break;
                            case "ip":
                                query = query.OrderBy(p => p.IP, options.SortDirection);
                                break;
                            case "scannedcounts":
                                query = query.OrderBy(p => p.ScannedCounts, options.SortDirection);
                                break;
                            case "batchdate":
                                query = query.OrderBy(p => p.BatchDate, options.SortDirection);
                                break;
                            case "createtime":
                                query = query.OrderBy(p => p.CreateTime, options.SortDirection);
                                break;
                        }
                    }

                    if (options.GetLimitOffset().HasValue)
                    {
                        query = query.Skip(options.GetLimitOffset().Value).Take(options.GetLimitRowcount().Value);
                    }
                    result.Items = query.ToList();
                    return result;
                })
            );
            #endregion

            #region UpdateBatchStockHistoryGrid
            MVCGridDefinitionTable.Add("UpdateBatchStockHistoryGrid", new MVCGridBuilder<UpdateBatchStockHistoryDto>(colDefauls)
                .AddColumns(cols =>
                {
                    cols.Add("BatchNo").WithHeaderText("批次码")
                        .WithValueExpression(p => p.BatchNo);
                    cols.Add("ProductName").WithHeaderText("产品名称")
                        .WithValueExpression(p => p.ProductName);
                    cols.Add("SKU").WithHeaderText("产品规格")
                      .WithValueExpression(p => p.SKU);
                    cols.Add("TotalNumber").WithHeaderText("入库数量")
                      .WithValueExpression(p => p.TotalNumber.ToString());
                    cols.Add("BatchDate").WithHeaderText("日期")
                        .WithValueExpression(p => p.BatchDate != null ? p.BatchDate.ToString("yyyy-MM-dd") : "");
                    cols.Add("CreateTime").WithHeaderText("创建时间")
                        .WithValueExpression(p => p.CreateTime != null ? p.CreateTime.ToString("yyyy-MM-dd HH:dd:ss") : "");
                   })
                .WithPreloadData(false)
                .WithSorting(true, "BatchNo")
                .WithPaging(true, 10)
                .WithPreviousButtonCaption("上一页")
                .WithNextButtonCaption("下一页")
                .WithSummaryMessage("<div style='display:none;'>{0}/{1}</div>总条数：{2}")
                .WithNoResultsMessage("")
                .WithProcessingMessage("加载中...")
                .WithRetrieveDataMethod((context) =>
                {
                    var options = context.QueryOptions;
                    string sortColumn = options.GetSortColumnData<string>();
                    var result = new QueryResult<UpdateBatchStockHistoryDto>();

                    var query = GetUpdateBatchStockHistoryDtos().AsQueryable();

                    result.TotalRecords = query.Count();
                    if (!String.IsNullOrWhiteSpace(options.SortColumnName))
                    {
                        switch (options.SortColumnName.ToLower())
                        {
                            case "batchno":
                                query = query.OrderBy(p => p.BatchNo, options.SortDirection);
                                break;
                            case "productname":
                                query = query.OrderBy(p => p.ProductName, options.SortDirection);
                                break;
                            case "sku":
                                query = query.OrderBy(p => p.SKU, options.SortDirection);
                                break;
                            case "totalnumber":
                                query = query.OrderBy(p => p.TotalNumber, options.SortDirection);
                                break;
                            case "batchdate":
                                query = query.OrderBy(p => p.BatchDate, options.SortDirection);
                                break;
                            case "createtime":
                                query = query.OrderBy(p => p.CreateTime, options.SortDirection);
                                break;
                        }
                    }

                    if (options.GetLimitOffset().HasValue)
                    {
                        query = query.Skip(options.GetLimitOffset().Value).Take(options.GetLimitRowcount().Value);
                    }
                    result.Items = query.ToList();
                    return result;
                })
            );
            #endregion

        }

        private static DeviceDto GetDeviceDto(DeviceInfo deviceInfo)
        {
            ApplicationDbContext ApplicationContext = new ApplicationDbContext();
            ApplicationUserManager UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var deviceDto = new DeviceDto();
            deviceDto.Device = deviceInfo;

            var userId = ApplicationContext.DeviceAccounts.FirstOrDefault(da => da.DeviceId == deviceInfo.Id).UserId;
            var user = UserManager.FindById(userId);
            deviceDto.User = user;
            return deviceDto;
        }

        private static IList<SendBatchDeviceHistoryDto> GetSendBatchDeviceHistoryDtos()
        {
            var dataList = ApplicationContext.SendBatchDeviceHistories.ToList();

            IList<SendBatchDeviceHistoryDto> dtoList = new List<SendBatchDeviceHistoryDto>();
            foreach (var item in dataList)
            {
                SendBatchDeviceHistoryDto entity = new SendBatchDeviceHistoryDto();
                var batchItem = ApplicationContext.BatchInfos.Where(m => m.BatchNo == item.BatchNo).FirstOrDefault();
                if (batchItem != null)
                {
                    TypeHelp.ObjCopy(batchItem, entity);
                }
                TypeHelp.ObjCopy(item, entity);
                entity.ScannedCounts = ApplicationContext.PrintBatchHistories.Where(m => m.BatchNo == entity.BatchNo).Count();
                dtoList.Add(entity);
            }
            return dtoList;
        }

        private static IList<UpdateBatchStockHistoryDto> GetUpdateBatchStockHistoryDtos()
        {
            var dataList = ApplicationContext.UpdateBatchStockHistories.ToList();

            IList<UpdateBatchStockHistoryDto> dtoList = new List<UpdateBatchStockHistoryDto>();
            foreach (var item in dataList)
            {
                UpdateBatchStockHistoryDto entity = new UpdateBatchStockHistoryDto();
                var batchItem = ApplicationContext.BatchInfos.Where(m => m.BatchNo == item.BatchNo).FirstOrDefault();
                if (batchItem != null)
                {
                    TypeHelp.ObjCopy(batchItem, entity);
                }
                TypeHelp.ObjCopy(item, entity);
                dtoList.Add(entity);
            }
            return dtoList;
        }
    }    

    public static class Extensions
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source,
            System.Linq.Expressions.Expression<Func<TSource, TKey>> keySelector,
            SortDirection order)
        {
            switch (order)
            {
                case SortDirection.Unspecified:
                case SortDirection.Asc: return source.OrderBy(keySelector);
                case SortDirection.Dsc: return source.OrderByDescending(keySelector);
            }

            throw new ArgumentOutOfRangeException("order");
        }
    }
}