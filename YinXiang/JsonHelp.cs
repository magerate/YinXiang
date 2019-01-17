using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace YinXiang
{

    #region override
    public class UTCDateTimeConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        private string TimeZoneID { get; set; }
        private string _defaultFromat = "MM/dd/yyyy HH:mm:ss";
        private string DefaultFromat
        {
            get { return _defaultFromat; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _defaultFromat = value;
                }
            }
        }
        private IDictionary<string, string> FormatList { get; set; }
        private string FormatStr { get; set; }
        /// <summary>
        /// 是否对指定字段进行格式化
        /// </summary>
        private bool AppointFormat = false;
        private bool UserTimeZone = false;

        private bool ExcludeTimeField = false;
        private string[] ExcludeTimeFieldList { get; set; }





        public UTCDateTimeConverter(string formatStr)
        {
            FormatStr = formatStr;
            AppointFormat = false;
            UserTimeZone = false;
            ExcludeTimeField = false;
        }
        public UTCDateTimeConverter(string timeZoneId, string formatStr)
        {
            TimeZoneID = timeZoneId;
            FormatStr = formatStr;
            AppointFormat = false;
            UserTimeZone = true;
            ExcludeTimeField = false;
        }

        public UTCDateTimeConverter(IDictionary<string, string> formatList)
        {
            SetValue("", "", formatList);
        }

        public UTCDateTimeConverter(string timeZoneId,IDictionary<string,string> formatList)
        {
            SetValue(timeZoneId, "", formatList);
        }
        public UTCDateTimeConverter(string timeZoneId,string defaultFormat, IDictionary<string, string> formatList)
        {
            SetValue(timeZoneId, defaultFormat, formatList);
        }

        public UTCDateTimeConverter(string timeZoneId,string defaultFormat, IDictionary<string, string> formatList,params string[] excludeTimeFieldList)
        {
            SetValue(timeZoneId, defaultFormat, formatList, excludeTimeFieldList);
        }

        private void SetValue(string timeZoneId, string defaultFormat, IDictionary<string, string> formatList, params string[] excludeTimeFieldList)
        {
            TimeZoneID = timeZoneId;
            DefaultFromat = defaultFormat;
            FormatList = formatList;
            AppointFormat = true;
            UserTimeZone = string.IsNullOrEmpty(timeZoneId) ? false : true;
            ExcludeTimeField = (excludeTimeFieldList.Length > 0) ? true : false;
            ExcludeTimeFieldList = excludeTimeFieldList;
        }



  
        private string GetFormat(string key)
        {

            if (key.LastIndexOf(".") > -1)
            {
                key = key.Substring(key.LastIndexOf(".") + 1);
            }

            if (!AppointFormat)
            {
                return FormatStr;//所有时间字段使用同一种格式
            }
            else
            {
                if (!FormatList.ContainsKey(key))
                {
                    return DefaultFromat;
                }
                return FormatList[key];
            }
        }

        private bool contains(string key)
        {
            if (!ExcludeTimeField)
            {
                return false;
            }
            return ExcludeTimeFieldList.Contains(key);
        }



        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(DateTime) || objectType == typeof(DateTime?));
        }

        private static IFormatProvider cultureF = new System.Globalization.CultureInfo("fr-FR", true);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }
            if ((FormatStr.Contains("d/MM/yyyy") && FormatStr.Contains("dd/MM/yyyy")) || FormatStr.Contains("d/M/yyyy"))
            {
                return DateTime.Parse(reader.Value.ToString(), cultureF, System.Globalization.DateTimeStyles.NoCurrentDateDefault);
            }
            else
            {
                var pacificTime = DateTime.Parse(reader.Value.ToString());
                return TimeHelp.ToUtc(pacificTime, TimeZoneID);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (UserTimeZone)
            {
                writer.WriteValue(
                    TimeHelp.ToLocal((DateTime)value, TimeZoneID)
                    .ToString(GetFormat(writer.Path)));
            }
            else
            {
                writer.WriteValue(
                   ((DateTime)value).ToString(GetFormat(writer.Path))
                   );
            }
                
        }
    }

    public class DynamicContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        private string[] _propertiesToSerialize = null;
        private bool AllowOrExpellent = true;

        public DynamicContractResolver(bool allowOrExpellent,params string[] propertiesToSerialize)
        {
            _propertiesToSerialize = propertiesToSerialize;
            AllowOrExpellent = allowOrExpellent;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            if (AllowOrExpellent)
            {
                return properties.Where(p => _propertiesToSerialize.Contains(p.PropertyName)).ToList();
            }
            else
            {
                return properties.Where(p => !_propertiesToSerialize.Contains(p.PropertyName)).ToList();
            }
        }
    }




    #endregion

    public class JsonHelp
    {
        #region To Json
        public static string ToJson(object obj)
        {
            if (obj != null)
            {
                IsoDateTimeConverter itc = new IsoDateTimeConverter();
                itc.DateTimeFormat = "MM/dd/yyyy HH:mm:ss";
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, itc);
            }
            return string.Empty;
        }
        public static string ToJson(object obj, string dataformateStr)
        {
            if (obj != null)
            {
                UTCDateTimeConverter timeFormat = new UTCDateTimeConverter(dataformateStr);
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, timeFormat);
            }
            return string.Empty;
        }
        public static string ToJson(object obj, string timeZoneId, string dataformateStr)
        {
            if (obj != null)
            {
                UTCDateTimeConverter timeFormat = new UTCDateTimeConverter(timeZoneId, dataformateStr);
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, timeFormat);
            }
            return string.Empty;
        }


        public static string ToJson(object obj, IDictionary<string, string> dataformateStr)
        {
            if (obj != null)
            {
                UTCDateTimeConverter timeFormat = new UTCDateTimeConverter(dataformateStr);
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, timeFormat);
            }
            return string.Empty;
        }

        public static string ToJson(object obj, string timeZoneId, IDictionary<string, string> dataformateStr)
        {
            if (obj != null)
            {
                UTCDateTimeConverter timeFormat = new UTCDateTimeConverter(timeZoneId, dataformateStr);
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, timeFormat);
            }
            return string.Empty;
        }
        public static string ToJson(object obj, string timeZoneId, string defaultFromat, IDictionary<string, string> dataformateStr)
        {
            if (obj != null)
            {
                UTCDateTimeConverter timeFormat = new UTCDateTimeConverter(timeZoneId, defaultFromat,dataformateStr);
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, timeFormat);
            }
            return string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="timeZoneId"></param>
        /// <param name="dataformateStr"></param>
        /// <param name="allowOrExpellent">排除或者</param>
        /// <param name="propertieName"></param>
        /// <returns></returns>
        public static string ToJson(object obj, string timeZoneId, string dataformateStr, bool allowOrExpellent, params string[] propertieName)
        {
            if (obj != null)
            {
                Newtonsoft.Json.JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.ContractResolver = new DynamicContractResolver(allowOrExpellent, propertieName);
                jss.Converters.Add(new UTCDateTimeConverter(timeZoneId, dataformateStr));
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, jss);
            }
            return string.Empty;
        }
        public static string ToJson(object obj, string timeZoneId, IDictionary<string, string> dataformateStr, bool allowOrExpellent, params string[] propertieName)
        {
            if (obj != null)
            {
                Newtonsoft.Json.JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.ContractResolver = new DynamicContractResolver(allowOrExpellent, propertieName);
                jss.Converters.Add(new UTCDateTimeConverter(timeZoneId, dataformateStr));
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, jss);
            }
            return string.Empty;
        }
        public static string ToJson(object obj, string timeZoneId, string defaultFormat,IDictionary<string, string> dataformateStr, bool allowOrExpellent, params string[] propertieName)
        {
            if (obj != null)
            {
                Newtonsoft.Json.JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.ContractResolver = new DynamicContractResolver(allowOrExpellent, propertieName);
                jss.Converters.Add(new UTCDateTimeConverter(timeZoneId,defaultFormat, dataformateStr));
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, jss);
            }
            return string.Empty;
        }
        public static string ToJson(object obj, string timeZoneId, string[] excludeTimeField, string defaultFormat, IDictionary<string, string> dataformateStr, bool allowOrExpellent, params string[] propertieName)
        {
            if (obj != null)
            {
                Newtonsoft.Json.JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.ContractResolver = new DynamicContractResolver(allowOrExpellent, propertieName);
                jss.Converters.Add(new UTCDateTimeConverter(timeZoneId, defaultFormat, dataformateStr));
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, jss);
            }
            return string.Empty;
        }
        #endregion

        #region ToObj
        public static T ToObj<T>(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
            {
                return default(T);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonStr);
        }
        public static T ToObj<T>(string jsonStr, string timezoneId)
        {
            if (string.IsNullOrEmpty(jsonStr))
            {
                return default(T);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonStr, new UTCDateTimeConverter(timezoneId, ""));
        }
        public static T ToObj<T>(byte[] jsonbyt)
        {
            if (jsonbyt == null && jsonbyt.Length == 0)
            {
                return default(T);
            }
            return ToObj<T>(System.Text.ASCIIEncoding.ASCII.GetString(jsonbyt));
        }
        public static T ToObj<T>(byte[] jsonbyt,string timezoneId)
        {
            if (jsonbyt == null && jsonbyt.Length == 0)
            {
                return default(T);
            }
            return ToObj<T>(System.Text.ASCIIEncoding.ASCII.GetString(jsonbyt), timezoneId);
        }
        public static T ToObjByDateFormat<T>(string jsonStr, string dateFormat)
        {
            if (string.IsNullOrEmpty(jsonStr))
            {
                return default(T);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonStr, new JsonSerializerSettings() { DateFormatString = dateFormat, DateFormatHandling = DateFormatHandling.IsoDateFormat });
        }

        public static T ToObj<T>(string jsonStr, string timezoneId,string dateFormat)
        {
            if (string.IsNullOrEmpty(jsonStr))
            {
                return default(T);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonStr, new UTCDateTimeConverter(timezoneId, dateFormat));
        }



        #endregion

        #region GetJson Field
        public static IList<string> Field<T>(string jsonStr) where T : class
        {
            IList<string> JsonField = Field(jsonStr);
            return typeof(T).GetProperties().Where(m => JsonField.Contains(m.Name)).Select(m => m.Name).ToList();
        }
        public static IList<string> Field(string jsonStr)
        {
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonStr);
            JObject jObject = null;
            if (jsonObj!=null && jsonObj.Count != null && jsonObj.Count > 0)
            {
                jObject = (Newtonsoft.Json.Linq.JObject)jsonObj[0];
            }
            else if (jsonObj != null && jsonObj.Count == null)
            {
                jObject = (Newtonsoft.Json.Linq.JObject)jsonObj;
            }
            if (jObject == null)
            {
                return new List<string>();
            }
           return jObject.Properties().Select(m => m.Name).ToList();
        }
        #endregion
    }

    public class TimeHelp
    {

        #region To Local
        public static DateTime ToLocal(string utcTime, string timeZoneId)
        {
            return ToLocal(Convert.ToDateTime(utcTime), timeZoneId);
        }

        public static DateTime ToLocal(DateTime utcTime, string timeZoneId)
        {
            utcTime = utcTime.Kind == DateTimeKind.Local ? DateTime.SpecifyKind(utcTime, DateTimeKind.Unspecified) : utcTime;
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }
        public static T ToLocal<T>(T entity, string timeZoneId) where T : class
        {
            return ToLocal<T>(entity, timeZoneId, false);
        }

        public static T ToLocal<T>(T entity, string timeZoneId, bool onlyAllowOrExpellent, params string[] propertieName) where T : class
        {
            return propertieHandel<T>(entity, timeZoneId, false, onlyAllowOrExpellent, propertieName);
        }
        #endregion

        #region To Utc
        public static DateTime ToUtc(DateTime localTime, string timeZoneId)
        {
            if (localTime.Kind == DateTimeKind.Utc)
            {
                return localTime;
            }

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            if (tzi.IsInvalidTime(localTime))
            {
                // 夏令时 切时，时间将被跳过
                localTime = localTime.AddHours(1);
            }
            localTime = localTime.Kind == DateTimeKind.Local ? DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified) : localTime;
            return TimeZoneInfo.ConvertTimeToUtc(localTime, tzi);
        }
        public static DateTime ToUtc(string utcTime, string timeZoneId)
        {
            return ToUtc(Convert.ToDateTime(utcTime), timeZoneId);
        }
        public static T ToUtc<T>(T entity, string timeZoneId) where T : class
        {
            return ToUtc<T>(entity, timeZoneId, false);
        }

        public static T ToUtc<T>(T entity, string timeZoneId, bool AllowOrExpellent, params string[] propertieName) where T : class
        {
            return propertieHandel<T>(entity, timeZoneId, true, AllowOrExpellent, propertieName);
        }
        #endregion

        #region propertie Handel
        private static T propertieHandel<T>(T entity, string timeZoneId, bool isToUtc, bool allowOrExpellent, params string[] propertieName) where T : class
        {
            var properties = entity.GetType().GetProperties().Where(x => x.PropertyType == typeof(DateTime) || x.PropertyType == typeof(DateTime?));

            if (propertieName.Length > 0 && allowOrExpellent)
            {
                properties = properties.Where(m => propertieName.Contains(m.Name));
            }
            else if (propertieName.Length > 0 && !allowOrExpellent)
            {
                properties = properties.Where(m => !propertieName.Contains(m.Name));
            }
            foreach (var property in properties)
            {
                ConvertHandel<T>(entity, timeZoneId, property, isToUtc);
            }
            return entity;
        }
        private static void ConvertHandel<T>(T entity, string timeZoneId, PropertyInfo propertyInfo, bool toUtc)
        {
            var dt = propertyInfo.PropertyType == typeof(DateTime?) ? (DateTime?)propertyInfo.GetValue(entity) : (DateTime)propertyInfo.GetValue(entity);
            if (dt == null)
            {
                return;
            }
            if (toUtc)
            {
                propertyInfo.SetValue(entity, ToUtc(dt.Value, timeZoneId));
            }
            else
            {
                propertyInfo.SetValue(entity, ToLocal(dt.Value, timeZoneId));
            }
        }
        #endregion

    }
}
