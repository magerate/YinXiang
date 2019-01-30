using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace YinXiang.Models
{
    public enum DeviceType
    {
        X30,
        iMark,
    }

    public class ManageDeviceViewModel
    {
        public IEnumerable<DeviceDto> Devices { get; set; }
        public IEnumerable<SelectListItem> AccountItems { get; set; }
    }

    public class DeviceInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public string JobFieldName { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        //public string Account { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    public class DeviceAccount
    {
        public int Id { get; set; }
        public string UserId { get; set; }
    }

    public class DeviceDto
    {
        public DeviceInfo Device { get; set; }
        public ApplicationUser User { get;set; }
    }

    public class CreateDeviceViewModel
    {
        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public string JobFieldName { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string UserId { get; set; }
    }

    public class DeviceInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public string JobFieldName { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string UserId { get; set; }
        public IEnumerable<SelectListItem> DeviceTypeItems { get; set; }
        public IEnumerable<SelectListItem> AccountItems { get; set; }
    }
}