
using System;
using CSharp.jspxnet;
using System.ComponentModel;
using Kingdee.BOS.Util;
using System.Web;
using Kingdee.BOS.Cache;
using System.Collections.Generic;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json.Linq;
using CSharp.jspxnet.utils;
using Newtonsoft.Json;
using Kingdee.Cyext;
using System.IO;

namespace kingdee.Cylb
{
    [Description("单据数据处理"), HotUpdate]
    public class OrderService
    {

        public static string GeneratorCache = "GeneratorCache";
        public static string SERVER_HOST = "SERVER_HOST";
        public static string CKD_SMS = "CKD_SMS";
        public static string KINGDEE_CONFIG = "Kingdee_Acc";


        public static string XSCKD_FILENAME = "xsckd.html";


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
                LogHelper.Debug("保存 host=" + host);
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

        public static void SaveCacheKingDeeAcc(KingdeeAcc kingdeeAcc)
        {
            try
            {
                IKCacheManager cacheManager = KCacheManagerFactory.Instance.GetCacheManager(GeneratorCache, KINGDEE_CONFIG);
                if (cacheManager != null)
                {
                    cacheManager.Put(KINGDEE_CONFIG, kingdeeAcc);
                }
            }
            catch
            {


            }
        }

        private static KingdeeAcc kingdeeAcc = new KingdeeAcc();
        public static KingdeeAcc GetKingdeeAcc(string path)
        {
            try
            {
                kingdeeAcc.Init(path);
                if (kingdeeAcc.load)
                {
                    return kingdeeAcc;
                }
                IKCacheManager cacheManager = KCacheManagerFactory.Instance.GetCacheManager(GeneratorCache, KINGDEE_CONFIG);
                if (cacheManager != null)
                {
                    Object obj = cacheManager.Get(KINGDEE_CONFIG);
                    if (obj!=null)
                    {
                        kingdeeAcc = (KingdeeAcc)obj;
                    }
                }
            }
            catch
            {


            }
            return kingdeeAcc;
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
                host = "nbkps.gnway.cc";
            }
            if (string.IsNullOrEmpty(host))
            {
                host = "nbkps.gnway.cc";
            }
            string path = AppDomain.CurrentDomain.BaseDirectory + "logs\\" + host + "_lbwebapi.xml";
            if (File.Exists(path))
            {
                return path;
            }

            return @"C:\Program Files (x86)\Kingdee\K3Cloud\WebSite\logs\nbkps.gnway.cc_lbwebapi.xml";
        }



        /// <summary>
        /// 返回IP地址位置
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ip"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public static string GetSelectedPhone(string FLinkPhone, string F_PCMK_LXRtel, string F_PCMK_LXRtel2)
        {

            //数据里边手机号填写很乱，这里坐过滤加工处理 begin
            // string FLinkPhone = Convert.ToString(obj["FLinkPhone"]);
            if (!string.IsNullOrEmpty(FLinkPhone))
            {
                FLinkPhone = DataConvert.GetNumber(FLinkPhone);
            }

            //  string F_PCMK_LXRtel = Convert.ToString(obj["F_PCMK_LXRtel"]);
            if (!string.IsNullOrEmpty(F_PCMK_LXRtel) && VerifyUtil.IsChinese(F_PCMK_LXRtel))
            {
                F_PCMK_LXRtel = DataConvert.GetNumber(F_PCMK_LXRtel);
            } 
            //   string F_PCMK_LXRtel2 = Convert.ToString(obj["F_PCMK_LXRtel2"]);
            if (!string.IsNullOrEmpty(F_PCMK_LXRtel2) && VerifyUtil.IsChinese(F_PCMK_LXRtel2))
            {
                F_PCMK_LXRtel2 = DataConvert.GetNumber(F_PCMK_LXRtel2);
            }

            string sendPhone = FLinkPhone;
            if (!VerifyUtil.IsPhoneNo(sendPhone))
            {
                sendPhone = F_PCMK_LXRtel;
            }

            if (!VerifyUtil.IsPhoneNo(sendPhone))
            {
                sendPhone = F_PCMK_LXRtel2;
            }
            if (!VerifyUtil.IsPhoneNo(sendPhone))
            {

                return string.Empty;
            }
            //数据里边手机号填写很乱，这里坐过滤加工处理 end
            return sendPhone;
        }

        /// <summary>
        /// 通过FID得到手机号
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public static string GetSmsPhone(Context context, int fid)
        {
            try
            {
                string sql = "/*dialect*/select top 10 * from T_SAL_OUTSTOCK WHERE FID=" + fid + " ORDER BY FID DESC";
                DataEntityCollection<DynamicObject> dynamicObjects = DBUtils.ExecuteDynamicObject(context, sql);
                foreach (DynamicObject obj in dynamicObjects)
                {
                    //数据里边手机号填写很乱，这里坐过滤加工处理 begin
                    string FLinkPhone = Convert.ToString(obj["F_PCMK_LXRTEL"]);
                    string F_PCMK_LXRtel = Convert.ToString(obj["F_PCMK_LXRtel"]);
                    string F_PCMK_LXRtel2 = Convert.ToString(obj["F_PCMK_LXRtel2"]);
                    return GetSelectedPhone(FLinkPhone, F_PCMK_LXRtel, F_PCMK_LXRtel2);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e.Message);
            }
            return string.Empty;
        }

        public static int GetNoToFid(string no)
        {
            if (string.IsNullOrEmpty(no))
            {
                return -1;
            }
            if (no.Length < 5)
            {
                return -1;
            }
            if (no.Length > 20)
            {
                return -1;
            }
            string fidStr = no.Substring(2, no.Length - 4);
            int fid = DataConvert.ToInt(fidStr, 0);
            if (fid < 123)
            {
                return -1;
            }
            return fid + 123;
        }

        // F_ttt_Image
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dto"></param>
        public static int UpdateCkdOrder(Context context, CkdVerifyDto dto)
        {

            /*
            F_TTT_IMAGE	image	
            F_TTT_IP	
            F_TTT_IPDATA	
            F_PCMK_CONFIGSTATUS
            F_TTT_DATE
            */
            //签名数据格式转 begin
            string str = FunUtil.GetUploadBase64Fixed(dto.signSendByte);// dto.signSendByte.Trim();
            byte[] signSendByte = Base64Helper.getBase64Decode(str);
            
            string sql = "UPDATE T_SAL_OUTSTOCK set F_PCMK_CONFIGSTATUS=@F_PCMK_CONFIGSTATUS,F_TTT_IMAGE=@F_TTT_IMAGE,F_TTT_IP=@F_TTT_IP,F_TTT_IPDATA=@F_TTT_IPDATA,F_TTT_DATE=@F_TTT_DATE WHERE FID=@FID";
            List<SqlParam> paramList = new List<SqlParam>();
            paramList.Add(new SqlParam("@F_PCMK_CONFIGSTATUS", KDDbType.String, "已确认"));
            paramList.Add(new SqlParam("@F_TTT_IMAGE", KDDbType.Binary, signSendByte));
            paramList.Add(new SqlParam("@F_TTT_IP", KDDbType.String, dto.ip));
            paramList.Add(new SqlParam("@F_TTT_IPDATA", KDDbType.String, dto.ipData));
            paramList.Add(new SqlParam("@F_TTT_DATE", KDDbType.DateTime, DateTime.Now));
            paramList.Add(new SqlParam("@FID", KDDbType.Int32, dto.fid));
            return DBUtils.Execute(context, sql, paramList);
        }
        public static int UpdateOrderQr(Context context,int FId)
        {
         
            string surl = "http://nbkps.gnway.cc:8089/xsckd.html?n=";
            string vurl = "http://nbkps.gnway.cc:8089/vxsckd.html?n=";

            string sql = "/*dialect*/UPDATE T_SAL_OUTSTOCK SET F_PCMK_QRCoder=concat('" + surl + "', RIGHT(newid(), 2), FID-123, RIGHT(newid(), 2)), F_PCMK_URL=concat('" + vurl + "', RIGHT(newid(), 2), FID-123, RIGHT(newid(), 2)) WHERE FID=@FID";
            List<SqlParam> paramList = new List<SqlParam>();
            paramList.Add(new SqlParam("@FID", KDDbType.Int32, FId));
            return DBUtils.Execute(context, sql, paramList);
        }

        /// <summary>
        /// 更新短信发送次数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public static int UpdateSmsTimes(Context context, string fid)
        {
            string sql = "/*dialect*/UPDATE T_SAL_OUTSTOCK set F_SMS_TIMES=F_SMS_TIMES+1  WHERE FID=@FID";
            List<SqlParam> paramList = new List<SqlParam>();
            paramList.Add(new SqlParam("@FID", KDDbType.String, fid));
            return DBUtils.Execute(context, sql, paramList);
        }

        /// <summary>
        /// 检查未确认收货超时的发送短信
        /// </summary>
        /// <param name="context"></param>
        public static void CheckSendSms(Context context)
        {
            KingdeeAcc kingdeeAcc = OrderService.GetKingdeeAcc(OrderService.GetWebapiPath(null));
            if (!kingdeeAcc.load)
            {

                LogHelper.Error("定时任务中不能得到配置");
                return;
            }
       
            if (string.IsNullOrEmpty(kingdeeAcc.smsTzTemplateCode))
            {
                LogHelper.Error("短信通知模版没有配置");
                return;
            }

            AliyunSmsHelper.AccessKeyId = kingdeeAcc.smsAccessKeyId;
            AliyunSmsHelper.AccessKeySecret = kingdeeAcc.smsAccessKeySecret;
            //random 保存到内存
            if (string.IsNullOrEmpty(AliyunSmsHelper.AccessKeyId) || string.IsNullOrEmpty(AliyunSmsHelper.AccessKeySecret))
            {
                LogHelper.Error("短信密钥没有配置");
                return;
            }
            try
            {
                string sql = "/*dialect*/select top 100 * from T_SAL_OUTSTOCK WHERE fid in (select m.fid from T_SAL_OUTSTOCK m left join PCMK_JBXX j on m.F_PCMK_XMZDD = j.FID left join T_SAL_OUTSTOCKENTRY e on e.FID = m.FID left join T_BD_MATERIAL s on s.FMATERIALID = e.FMATERIALID left join T_BD_MATERIALBASE b on s.FMATERIALID = b.FMATERIALID where b.FCategoryID <> 242 AND j.F_PCMK_AZXZ <> '不安装' and e.FENTRYID > 0 AND m.FAPPROVEDATE > '2023-02-06' group by m.FID) AND "  + kingdeeAcc.sendDateField + " is not null AND " + kingdeeAcc.sendDateField + " > '2023-02-06' AND DATEDIFF(day ," + kingdeeAcc.sendDateField + " , getdate() )> " + kingdeeAcc.smsDay + " AND F_PCMK_CONFIGSTATUS not like '%确认' AND (F_SMS_TIMES = 0 OR F_SMS_TIMES is null) ORDER BY FID DESC";
               // LogHelper.Debug("sms sql=" + sql);
                DataEntityCollection<DynamicObject> dynamicObjects = DBUtils.ExecuteDynamicObject(context, sql);

                LogHelper.Debug("sms dynamicObjects=" + JsonConvert.SerializeObject(dynamicObjects));
                foreach (DynamicObject obj in dynamicObjects)
                {

                    string FID = Convert.ToString(obj["FID"]);
                    if (string.IsNullOrEmpty(FID))
                    {
                        continue;
                    }

                    string billNo = Convert.ToString(obj["FBILLNO"]);

                    //数据里边手机号填写很乱，这里坐过滤加工处理 begin
                    string FLinkPhone = Convert.ToString(obj["F_PCMK_LXRTEL"]);

                    string F_PCMK_LXRtel = Convert.ToString(obj["F_PCMK_LXRtel"]);

                    string F_PCMK_LXRtel2 = Convert.ToString(obj["F_PCMK_LXRtel2"]);

                    string sendPhone = GetSelectedPhone(FLinkPhone, F_PCMK_LXRtel, F_PCMK_LXRtel2);
                    if (!VerifyUtil.IsPhoneNo(sendPhone))
                    {
                        LogHelper.Error(billNo + ",找不到联系人的手机号");
                        continue;
                    }
                    
                    //数据里边手机号填写很乱，这里坐过滤加工处理 end

                    string F_PCMK_QRCoder = Convert.ToString(obj["F_PCMK_QRCoder"]);
                    if (string.IsNullOrEmpty(F_PCMK_QRCoder))
                    {
                        LogHelper.Error(billNo + ",二维码数据不能为空");
                        continue;
                    }

                    string F_PCMK_SHDW = null;
                    
                    if (obj.Contains("F_PCMK_SHDW"))
                    {
                        F_PCMK_SHDW = Convert.ToString(obj["F_PCMK_SHDW"]);
                    }
                    if (string.IsNullOrEmpty(F_PCMK_SHDW))
                    {
                        string filter = "FID=" + FID;
                        DynamicObject order = FunUtil.QuerySingle(context, "SAL_OUTSTOCK", filter);
                        if (order == null)
                        {
                            continue;
                        }

                        F_PCMK_SHDW = FunUtil.GetDynamicObjectName(order["CustomerID"]);
                    }

                    if (string.IsNullOrEmpty(F_PCMK_SHDW))
                    {
                        F_PCMK_SHDW = "请完善信息";
                    }
                    
                    var v = new
                    {
                        code = billNo,
                        name = F_PCMK_SHDW
                    };
                    
                    string outStr = AliyunSmsHelper.SendSms(sendPhone, kingdeeAcc.smsSignName, kingdeeAcc.smsTzTemplateCode, JsonConvert.SerializeObject(v));
                    if (!JsonSplit.IsJson(outStr))
                    {
                        LogHelper.Error(billNo + ",短信接口异常,"+ outStr);
                        continue;
                    }

                    JObject jsonObject = JsonConvert.DeserializeObject<JObject>(outStr);
                    if (jsonObject["Message"]!=null&&"OK".EqualsIgnoreCase(jsonObject["Message"].ToString()))
                    {
                        //LogHelper.Debug(billNo+"---------update=" + jsonObject.ToString());
                        UpdateSmsTimes(context, FID);
                    }
                    break;
                    
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e.Message);
            }
        }
    }
}
