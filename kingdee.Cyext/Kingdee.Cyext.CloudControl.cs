
using Newtonsoft.Json;
using System;
using CSharp.jspxnet;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using System.ComponentModel;
using Kingdee.BOS.Util;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using System.IO;

/// <summary>
///云部署限制太多，这里开启sql测试
/// </summary>
namespace Kingdee.Cyext
{
    [Description("云部署控制"), HotUpdate]
    public class CloudControl : AbstractWebApiBusinessService
    {
        public CloudControl(KDServiceContext context)
            : base(context)
        {

        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="inputSql"></param>
        /// <returns></returns>
        public string Sql(string inputSql)
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }
            if (string.IsNullOrEmpty(inputSql))
            {
                return "no data";
            }
            bool isSelect = inputSql.ToLower().IndexOf("select ") != -1;
            bool isUpdate = inputSql.ToLower().IndexOf("update ") != -1;
            try
            {
                if (isSelect && !isUpdate)
                {
                    DataEntityCollection<DynamicObject> dynamicObjects = DBUtils.ExecuteDynamicObject(KDContext.Session.AppContext, inputSql);
                    return JsonConvert.SerializeObject(dynamicObjects);
                }
                else
                {
                    return DBUtils.Execute(KDContext.Session.AppContext, inputSql) + "";
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("执行错误 Sql=" + inputSql + "  " + e.Message);
                return e.Message;
            }
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="inputSql"></param>
        /// <returns></returns>
        public string RunSql(string inputSql)
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }
            if (string.IsNullOrEmpty(inputSql))
            {
                return "no data";
            }
            string sql = Base64Helper.getBase64DecodeString(inputSql);
            try
            {
                bool isSelect = sql.ToLower().IndexOf("select ") != -1;
                if (isSelect)
                {
                    DataEntityCollection<DynamicObject> dynamicObjects = DBUtils.ExecuteDynamicObject(KDContext.Session.AppContext, sql);
                    string outStr = JsonConvert.SerializeObject(dynamicObjects);
                    return Base64Helper.ToBase64String(outStr);
                }
                else
                {
                    int x = DBUtils.Execute(KDContext.Session.AppContext, sql);
                    return Base64Helper.ToBase64String(x + "");
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("执行错误 RunSql=" + inputSql + "," + e.Message);
                return Base64Helper.ToBase64String(e.Message);
            }
        }

      
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <returns></returns>
        public string logs()
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }
            return LogHelper.getText();
        }

        public string ClearToDayLog()
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }
            LogHelper.ClearToDayLog();
            return "清理完成";
        }



        /// <summary>
        /// 写配置文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public string Write(string path, string html)
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }
            string content = Base64Helper.getBase64DecodeString(html);
            string wLen = IoUtil.FileStreamWrite(path, content, System.Text.Encoding.UTF8);
            return "写完成" + wLen;
        }

        /// <summary>
        /// 读配置文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string Read(string path)
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }
            string txt = IoUtil.FileStreamRead(path, System.Text.Encoding.UTF8);
            return Base64Helper.ToBase64String(txt);
        }

        public string Delete(string path, string key)
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }
            if (!"993665".Equals(key))
            {
                return "no";
            }
            FileInfo fi = new FileInfo(path);
            if (fi.Name.ToLower().EndsWith(".dll"))
            {
                return "no";
            }
            try
            {
                File.Delete(path);
            }
            catch (System.Exception e)
            {
                return e.Message;
            }
            return "ok";
        }


        
    }
}