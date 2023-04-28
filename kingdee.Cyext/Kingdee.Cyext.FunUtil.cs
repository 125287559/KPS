
using CSharp.jspxnet;
using Kingdee.BOS;
using Kingdee.BOS.Cache;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.NotePrint;
using Kingdee.BOS.Web.Core;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Web.List;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.ListFilter;
using System.Net;
using System.Text;

namespace Kingdee.Cyext
{
    [Description("CY扩展的金蝶常用功能"), HotUpdate]
    public class FunUtil
    {
        public FunUtil()
        {

        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="thisContent"></param>
        /// <param name="formId"></param>
        /// <param name="Filter"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static DynamicObject[] Query(Context thisContent, string formId, string Filter, int limit)
        {
            FormMetadata materialMetada = MetaDataServiceHelper.GetFormMetaData(thisContent, formId);
            QueryBuilderParemeter queryParameter = new QueryBuilderParemeter();
            queryParameter.BusinessInfo = materialMetada.BusinessInfo;
            queryParameter.FilterClauseWihtKey = Filter;
            queryParameter.FormId = formId;
            queryParameter.Limit = limit;
            return BusinessDataServiceHelper.Load(thisContent, materialMetada.BusinessInfo.GetDynamicObjectType(), queryParameter);
        }

        /// <summary>
        /// 查询单据类型
        /// </summary>
        /// <param name="thisContent"></param>
        /// <param name="Filter"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static DynamicObject QuerySingle(Context thisContent, string formId, string Filter)
        {
            //FBillTypeId,FNumber,FName
            FormMetadata materialMetada = MetaDataServiceHelper.GetFormMetaData(thisContent, formId);
            QueryBuilderParemeter queryParameter = new QueryBuilderParemeter();
            queryParameter.BusinessInfo = materialMetada.BusinessInfo;
            queryParameter.FilterClauseWihtKey = Filter;
            queryParameter.FormId = formId;
            queryParameter.Limit = 1;
            DynamicObject[] dataList = BusinessDataServiceHelper.Load(thisContent, materialMetada.BusinessInfo.GetDynamicObjectType(), queryParameter);
            foreach (DynamicObject data in dataList)
            {
                return data;
            }
            return null;
        }


        /// <summary>
        /// 取DynamicObject 的 Number
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string GetDynamicObjectNumber(object o)
        {
            try
            {
                if (o == null)
                {
                    return "";
                }

                DynamicObject d = (o as DynamicObject);
                if (d.Contains("Number"))
                {
                    return (o as DynamicObject)["Number"] + "";
                }
                return string.Empty;
            }
            catch
            {
                return "";
            }
        }


        public static string GetDynamicObjectName(object o)
        {
            try
            {
                if (o == null)
                {
                    return "";
                }
                DynamicObject d = (o as DynamicObject);
                if (d.Contains("Name"))
                {
                    return (o as DynamicObject)["Name"] + "";
                }
                return string.Empty;
            }
            catch
            {
                return "";
            }
        }

        public static string GetDynamicObjectUseOrgId(object o)
        {
            try
            {
                if (o == null)
                {
                    return "";
                }
                return (o as DynamicObject)["UseOrgId"] + "";
            }
            catch
            {
                return "";
            }
        }


        /**
        * K3 通用查询
        * */
        public static List<List<object>> K3Query(K3CloudApiClient client, string formId, string fieldKeys, string filterString, int limit)
        {
            Dictionary<string, object> dtn = new Dictionary<string, object> {
                {"FormId",formId },
                {"FieldKeys",fieldKeys },//返回列
                {"FilterString",filterString },//过滤条件
                {"OrderString","" },
                {"TopRowCount",0 },
                {"StartRow",0 },
                {"Limit",limit },
            };
            return client.ExecuteBillQuery(JsonConvert.SerializeObject(dtn));
        }

     
        /**
         * 判断查询是否错误
         * */
        public static bool IsK3Error(List<List<object>> list)
        {
            string result = JsonConvert.SerializeObject(list);
            return result == null || (result.Contains("IsSuccess\":false")) && result.Contains("ErrorCode");
        }

        /**
         * 得到错误信息
         */
        public static string GetK3ErrorInfo(List<List<object>> list)
        {
            string result = JsonConvert.SerializeObject(list);
            return GetK3ErrorMessage(result);
        }

        /**
        * 保存成功判断
        * */
        public static bool IsK3SaveSuccess(string outStr)
        {
            return (outStr.Contains("IsSuccess\":true")) && outStr.Contains("SuccessEntitys");
        }

        /**
         * 得到成功信息返回
         */
        public static string GetK3SaveSuccessData(string outStr)
        {
            if (!outStr.Contains("SuccessEntitys"))
            {
                return null;
            }
            JObject ja = (JObject)JsonConvert.DeserializeObject(outStr);
            /*
                {"Result":{"ResponseStatus":{"IsSuccess":true,"Errors":[],"SuccessEntitys":[{"Id":374095,"Number":"QLGXC202112240035","DIndex":0}],"SuccessMessages":[],"MsgCode":0},"Id":374095,"Number":"QLGXC202112240035","NeedReturnData":[{}]}}
            */
            if (ja != null && ja.Count > 0)
            {
                JToken jToken = ja["Result"];
                JToken resultToken = jToken["ResponseStatus"]["SuccessEntitys"].ToString();
                return resultToken.Value<string>();
            }
            return null;
        }


        /**
        * 得到错误信息返回
        * 
        {"Result":{"ResponseStatus":{"IsSuccess":true,"Errors":[],"SuccessEntitys":[{"Id":374095,"Number":"QLGXC202112240035","DIndex":0}],"SuccessMessages":[],"MsgCode":0},"Id":374095,"Number":"QLGXC202112240035","NeedReturnData":[{}]}}
        */
        public static string GetK3ErrorMessage(string outStr)
        {
            object obj = JsonConvert.DeserializeObject(outStr);
            if (obj.GetType() == typeof(JObject))
            {
                JObject ja = (JObject)obj;
                if (ja != null && ja.Count > 0)
                {
                    JToken jToken = ja["Result"];
                    JToken errorToken = jToken["ResponseStatus"]["Errors"];
                    if (errorToken.Value<JArray>().Count > 0)
                    {
                        return errorToken[0]["Message"].ToString();
                    }
                }
            }
            else
            {
                JArray ja = (JArray)obj;
                if (ja != null && ja.Count > 0)
                {
                    if (ja[0].GetType() == typeof(JArray))
                    {
                        return GetK3ErrorMessage(ja[0].ToString());
                    }

                    JToken jToken = ja[0]["Result"];
                    JToken errorToken = jToken["ResponseStatus"]["Errors"];
                    if (errorToken.Value<JArray>().Count > 0)
                    {
                        return errorToken[0]["Message"].ToString();
                    }
                }
            }
            return null;
        }


        public static string GeneratorCache = "GeneratorCache";
        public static string SERVER_HOST = "SERVER_HOST";


        public static void SaveCacheHost(HttpContext httpContext)
        {
            string host = "";
            try
            {
                HttpRequest request = null;
                if (httpContext != null)
                {
                    request = httpContext.Request;
                }
                if (request != null && request.Url != null)
                {
                    host = request.Url.Host;
                }
                LogHelper.Debug("保存 host 1=" + host);
                if (!string.IsNullOrEmpty(host))
                {
                    IKCacheManager cacheManager = KCacheManagerFactory.Instance.GetCacheManager(GeneratorCache, SERVER_HOST);
                    LogHelper.Debug("保存 host 2=" + (cacheManager != null));
                    cacheManager.Put(SERVER_HOST, host);
                }
            }
            catch
            {


            }
        }

        public static string GetWebapiPath(HttpContext httpContext)
        {
            string host = "";
            try
            {
                HttpRequest request = null;
                if (httpContext != null)
                {
                    request = httpContext.Request;
                }
                if (request != null && request.Url != null)
                {
                    host = request.Url.Host;
                }
                if (string.IsNullOrEmpty(host))
                {
                    HttpContext httpContext2 = HttpContext.Current;
                    if (httpContext2 != null)
                    {
                        request = httpContext2.Request;
                    }
                    if (request != null && request.Url != null)
                    {
                        host = request.Url.Host;
                    }
                }

                if (string.IsNullOrEmpty(host))
                {
                    IKCacheManager cacheManager = KCacheManagerFactory.Instance.GetCacheManager(GeneratorCache, SERVER_HOST);
                    host = cacheManager.Get<string>(SERVER_HOST);
                }
            }
            catch
            {

            }
            if (string.IsNullOrEmpty(host))
            {
                host = "127.0.0.1";
            }
            LogHelper.Debug("获取host=" + host);
            return AppDomain.CurrentDomain.BaseDirectory + "logs\\" + host + "_webapi.xml";
        }

        public static string GetSessionId()
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                HttpSessionState session = null;
                if (httpContext != null)
                {
                    session = httpContext.Session;
                }
                if (session != null)
                {
                    return session.SessionID;
                }
            }
            catch
            {

            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model">this.View.Model</param>
        /// <param name="formIdDef">如果为空,默认后边</param>
        /// <returns></returns>
        public static string GetFormId(IDynamicFormModel model,string formIdDef)
        {
            if (model==null)
            {
                return formIdDef;
            }
            return model.BillBusinessInfo.GetForm().Id;
        }

   

        /// <summary>
        /// 动作权限判断
        /// </summary>
        /// <param name="thisContent">上下文</param>
        /// <param name="formId">单据ID,如果没有可以是空</param>
        /// <param name="itemId">权限项,bos里边配置对应</param>
        /// <param name="itemIdDef">上边一个找不到,用这个在去找一下</param>
        /// <returns>true 有权限</returns>
        public static bool PermissionAuth(Context thisContent,string formId, string itemId, string itemIdDef = "")
        {
            //权限项内码，通过 T_SEC_PermissionItem 权限项表格进行查询。
            //权限配置验证begin
            string formPermissionId = "SEC_PermissionItem";
            string filterPermission = "FNumber='"+ itemId + "'";
            DynamicObject permissionObject = QuerySingle(thisContent, formPermissionId, filterPermission);
            string permissionItemId = "";
            if (permissionObject != null)
            {
                permissionItemId = Convert.ToString(permissionObject["id"]);
            }
            else if (!string.IsNullOrEmpty(itemIdDef))
            {
                filterPermission = "FNumber='"+ itemIdDef + "'";
                permissionObject = QuerySingle(thisContent, formPermissionId, filterPermission);
                if (permissionObject != null)
                {
                    permissionItemId = Convert.ToString(permissionObject["id"]);
                }
            }

            if (!string.IsNullOrEmpty(permissionItemId))
            {
                //得到单据类型权限id begin
                if (permissionObject != null)
                {
                    permissionItemId = Convert.ToString(permissionObject["id"]);
                }
                //得到单据类型权限id end


                //权限判断begin
                BusinessObject permissionBizObj = new BusinessObject();
                permissionBizObj.Id = formId; //"PRD_MO" 
                PermissionAuthResult permissionAuthResult = PermissionServiceHelper.FuncPermissionAuth(thisContent, permissionBizObj, permissionItemId);
                if (permissionAuthResult != null && !permissionAuthResult.Passed)
                {
                    return false;
                 
                }
                //权限判断end
            }
            //权限配置验证end
            return true;
        }


        //保存附件
        //http://[IP]/K3Cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.AttachmentUpLoad.common.kdsvc
        public static string SaveAttachment(K3CloudApiClient client, AttachmentDto dto)
        {
            return client.AttachmentUpload(JsonConvert.SerializeObject(dto));
        }



        /// <summary>
        /// 返回IP地址位置
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ip"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public static string GetIP138Data(string token, string ip = null, string datatype = "jsonp")
        {
            if (string.IsNullOrEmpty(ip) && HttpContext.Current != null)
            {
                ip = HttpContext.Current.Request.UserHostAddress;
            }
            if (string.IsNullOrEmpty(ip))
            {
                return string.Empty;
            }
            string url = string.Format("https://api.ip138.com/ip/?ip={0}&datatype={1}&token={2}", ip, datatype, token);
            string ipData = "";
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                ipData = client.DownloadString(url);
            }
            if (!JsonSplit.IsJson(ipData))
            {
                return ipData;
            }

            //"{\"ret\":\"err\",\"msg\":\"请添加token\"}"
            try
            {
                JObject j = JsonConvert.DeserializeObject<JObject>(ipData);
                string result = string.Empty;
                string ret = j.Value<string>("ret");
                if ("err".EqualsIgnoreCase(ret))
                {
                    return j.Value<string>("msg");
                }

                JArray ja = j.Value<JArray>("data");

                for (int i = 0; i < ja.Count; i++)
                {
                    string val = ja.Value<string>(i);
                    if ("中国".Equals(val))
                    {
                        continue;
                    }
                    if ("".Equals(val))
                    {
                        break;
                    }
                    result = result + val + " ";
                }
                if (string.IsNullOrEmpty(result))
                {
                    return string.Empty;
                }
                return result.Trim();
            }
            catch (Exception e)
            {
                LogHelper.Error(e.Message);
            }
            return null;
        }

        /// <summary>
        /// 上传的base64修复，只要数据部分
        /// </summary>
        /// <param name="base64Str"></param>
        /// <returns></returns>
        public static string GetUploadBase64Fixed(string base64Str)
        {
            if (base64Str == null)
            {
                return string.Empty;
            }
            //签名数据格式转 begin
            string str = base64Str.Trim();

            if (!string.IsNullOrEmpty(str))
            {
                int pos = str.IndexOf("base64,");
                if (pos == -1)
                {
                    pos = str.ToLower().IndexOf("base64,");
                }
                str = str.Substring(pos + 7);

            }
            //签名数据格式转 end
            return str;
        }



        //打印pdf
        //“套打配置”参数NotePrint_Export_PrintWithImage = True;
        //https://vip.kingdee.com/article/378226648006817792?productLineId=1&isKnowledge=2
        public static bool PrintPdf(Context ctx, string formId, string tempId, int fid, string pdfFilePath)
        {

            /*
                        Kingdee.BOS.Core.Import.IImportView importView = sBillView as Kingdee.BOS.Core.Import.IImportView;

                        importView.AddViewSession();

                        importView.RemoveViewSession();
            */

            IDynamicFormView view = CreateView(ctx, formId);
            CommonSession.GetCurrent(ctx.UserToken).SessionManager.GetOrAdd(view.PageId, view.GetType().Name, view);

            //单据内码与套打模板标识一一对应
            List<string> billIds = new List<string> { fid + "" }; //单据内码
            List<string> tempIds = new List<string> { tempId };//套打模板标识 "1e8cae2e-875d-4a45-a037-4f3be418e6a6"

            try
            {
                PrintExportInfo pExInfo = new PrintExportInfo();
                pExInfo.PageId = view.PageId;
                pExInfo.FormId = view.BillBusinessInfo.GetForm().Id;
                pExInfo.BillIds = billIds;//单据内码
                pExInfo.TemplateIds = tempIds;//套打模板ID
                pExInfo.FileType = ExportFileType.PDF;//文件格式
                pExInfo.ExportType = ExportType.ByPage;//导出格式
                //string temppath = "D:\\";
                //string filePath = Path.Combine(temppath, Guid.NewGuid().ToString() + ".PDF");
                pExInfo.FilePath = pdfFilePath;//文件输出路径
                if (view is IListViewService)
                {
                    ListView list = (ListView)view;
                    list.ExportNotePrint(pExInfo);
                    return true;
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e.Message);
            }
            return false;
        }


        public static IDynamicFormView CreateView(Context ctx, string formId)
        {
            FormMetadata _metadata = (FormMetadata)MetaDataServiceHelper.Load(ctx, formId);
            var OpenParameter = CreateOpenParameter(OperationStatus.VIEW, ctx, formId, _metadata);
            var Provider = GetListServiceProvider(OpenParameter);

            string importViewClass = "Kingdee.BOS.Web.List.ListView,Kingdee.BOS.Web";
            Type type = Type.GetType(importViewClass);
            IListViewService View = (IListViewService)Activator.CreateInstance(type);

            ((IListViewService)View).Initialize(OpenParameter, Provider);
            ((IListViewService)View).LoadData();
            return (IDynamicFormView)View;
        }

        private static ListOpenParameter CreateOpenParameter(OperationStatus status, Context ctx, string formId, FormMetadata _metadata)
        {

            ListOpenParameter openPara = new ListOpenParameter(formId, _metadata.GetLayoutInfo().Id);
            Form form = _metadata.BusinessInfo.GetForm();
            openPara = new ListOpenParameter(formId, string.Empty);
            openPara.Context = ctx;
            openPara.ServiceName = form.FormServiceName;
            openPara.PageId = Guid.NewGuid().ToString();

            // 单据
            openPara.FormMetaData = _metadata;
            openPara.LayoutId = _metadata.GetLayoutInfo().Id;
            openPara.ListFormMetaData = (FormMetadata)FormMetaDataCache.GetCachedFormMetaData(ctx, FormIdConst.BOS_List);

            // 操作相关参数
            openPara.SetCustomParameter(FormConst.PlugIns, form.CreateListPlugIns());
            openPara.SetCustomParameter("filterschemeid", "");
            openPara.SetCustomParameter("listfilterparameter", new ListRegularFilterParameter());
            // 修改主业务组织无须用户确认
            openPara.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);

            openPara.SetCustomParameter("SessionManager", CommonSession.GetCurrent(ctx.UserToken).SessionManager);
            return openPara;
        }

        private static IResourceServiceProvider GetListServiceProvider(DynamicFormOpenParameter param)
        {
            FormServiceProvider provider = new FormServiceProvider();
            provider.Add(typeof(IDynamicFormView), CreateListView(param));
            provider.Add(typeof(DynamicFormViewPlugInProxy), new ListViewPlugInProxy());
            provider.Add(typeof(DynamicFormModelPlugInProxy), new ListModelPlugInProxy());
            provider.Add(typeof(IDynamicFormModelService), GetListModel(param));
            provider.Add(typeof(IListFilterModelService), GetListFilterModel());
            var type = TypesContainer.GetOrRegister("Kingdee.BOS.Business.DynamicForm.DefaultValue.DefaultValueCalculator,Kingdee.BOS.Business.DynamicForm");
            provider.Add(typeof(IDefaultValueCalculator), Activator.CreateInstance(type));
            // 注册IDBModelService
            type = TypesContainer.GetOrRegister("Kingdee.BOS.Business.DynamicForm.DBModel.DBModelService,Kingdee.BOS.Business.DynamicForm");
            provider.Add(typeof(IDBModelService), Activator.CreateInstance(type));
            return provider;
        }


        /// 获取视图
        ///
        #region 创建listview
        ///
        private static IDynamicFormView CreateListView(DynamicFormOpenParameter param)
        {
            Form form = param.FormMetaData.BusinessInfo.GetForm();
            if (form.FormGroups != null && form.FormGroups.Count > 0)
            {
                return new TreeListView();
            }
            else
            {
                return new ListView();
            }
        }
        ///


        /// 获取视图模型
        ///

        ///
        private static IDynamicFormModelService GetListModel(DynamicFormOpenParameter param)
        {
            Form form = param.FormMetaData.BusinessInfo.GetForm();
            if (form.FormGroups != null && form.FormGroups.Count > 0)
            {
                var type = TypesContainer.GetOrRegister("Kingdee.BOS.Model.List.TreeListModel,Kingdee.BOS.Model");
                return (IDynamicFormModelService)Activator.CreateInstance(type);
            }
            else
            {
                var type = TypesContainer.GetOrRegister("Kingdee.BOS.Model.List.ListModel,Kingdee.BOS.Model");
                return (IDynamicFormModelService)Activator.CreateInstance(type);
            }
        }
        ///


        /// 创建过滤条件模型
        ///

        ///
        private static IListFilterModelService GetListFilterModel()
        {
            Type type = TypesContainer.GetOrRegister("Kingdee.BOS.Model.ListFilter.ListFilterModel,Kingdee.BOS.Model");
            return (IListFilterModelService)Activator.CreateInstance(type);
        }
        #endregion

    }
}
