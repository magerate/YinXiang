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
                        + "<a class='btn' href='javascript:void(0);' onclick='deleteAccount(\"" + p.Id +"\")'>删除</a>");
                })
                .WithPreloadData(false)
                .WithSorting(true, "UserName")
                .WithPaging(true, 10)
                .WithPreviousButtonCaption("上一页")
                .WithNextButtonCaption("下一页")
                .WithSummaryMessage("<div style='display:none;'>{0}/{1}</div>总条数：{2}")
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
                        .WithValueExpression(p =>  p.Device.IP);
                    cols.Add("User.UserName").WithHeaderText("绑定账号")
                        .WithSorting(false)
                        .WithValueExpression(p => (p.User??new ApplicationUser()).UserName??"");
                    cols.Add("Device.CreateTime").WithHeaderText("添加日期")
                        .WithValueExpression(p => p.Device.CreateTime.ToString("d"));
                    cols.Add("Operate").WithHtmlEncoding(false)
                        .WithSorting(false)
                        .WithHeaderText(" ")
                        .WithValueExpression((p, c) => "<a class='btn' href='javascript:void(0);'onclick='openEditDevice(" + p.Device.Id + ")'>编辑</a>"
                        +"<a class='btn' href='javascript:void(0);'onclick='deleteDevice(" + p.Device.Id + ")'>删除</a>");
                })
                .WithPreloadData(false)
                .WithSorting(true, "Devices.Name")
                .WithPaging(true, 10)
                .WithPreviousButtonCaption("上一页")
                .WithNextButtonCaption("下一页")
                .WithSummaryMessage("<div style='display:none;'>{0}/{1}</div>总条数：{2}")
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