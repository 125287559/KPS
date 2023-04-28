
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using System.ComponentModel;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using CSharp.jspxnet;
using CSharp.jspxnet.Model;
using Newtonsoft.Json;
using CSharp.jspxnet.utils;
using System.Web;
using System;
using Kingdee.BOS.Orm.DataEntity;
using JinianNet.JNTemplate;
using Kingdee.Cyext;
using Kingdee.BOS.Cache;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

/// <summary>
///云部署限制太多，这里开启sql测试
/// </summary>
namespace kingdee.Cylb
{
    [Description("数据服务接口"), HotUpdate]
    public class NotifyService : AbstractWebApiBusinessService
    {
        private static K3CloudApiClient client;
        static TemplateUtil util = new TemplateUtil();
        public NotifyService(KDServiceContext context) : base(context)
        {
            //优先外部参数
            Engine.Configure(c => {
                c.EnableCache = false;
                c.TagPrefix = "${";
                c.TagSuffix = "}";
                c.TagFlag = '$';
            });
           // Engine.UseInterpretationEngine();

        }
        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        private K3CloudApiClient GetK3CloudApiClient(KingdeeAcc kingdeeAcc)
        {

            if (client == null)
            {
                client = new K3CloudApiClient(kingdeeAcc.url);
            }
            var loginResult = client.ValidateLogin(kingdeeAcc.dbId, kingdeeAcc.userName, kingdeeAcc.pwd, kingdeeAcc.lCid);
            var resultType = JObject.Parse(loginResult)["LoginResultType"].Value<int>();
            //登录结果类型等于1，代表登录成功
            if (resultType == 1)
            {

                OrderService.SaveCacheHost(System.Web.HttpContext.Current);
                return client;
            }
            else
            {
                LogHelper.Debug("账号配置有误");
                client = null;
            }
            return client;
        }
        /// <summary>
        /// 短信发送接口
        /// </summary>
        /// <returns></returns>
        public string SendSms(string no)
        {
            RocResponse<string> response = new RocResponse<string>();
            response.success = 0;
            if (KDContext.Session.AppContext == null)
            {
                response.message = "未登陆";
                return JsonConvert.SerializeObject(response);
            }
            int fid = OrderService.GetNoToFid(no);
            if (fid<=0)
            {
                response.message = "no参数错误";
                return JsonConvert.SerializeObject(response);
            }
          
            string phone = OrderService.GetSmsPhone(KDContext.Session.AppContext, fid);
            if (string.IsNullOrEmpty(phone) || !VerifyUtil.IsPhoneNo(phone))
            {
                response.message = "没有绑定正确的的手机号";
                return JsonConvert.SerializeObject(response);
            }

            LogHelper.Debug("phone=" + phone);

            KingdeeAcc kingdeeAcc = OrderService.GetKingdeeAcc(OrderService.GetWebapiPath(HttpContext.Current));
            var random = new Random().Next(100000, 999999).ToString();

            AliyunSmsHelper.AccessKeyId = kingdeeAcc.smsAccessKeyId;
            AliyunSmsHelper.AccessKeySecret = kingdeeAcc.smsAccessKeySecret;
            //random 保存到内存

            string outStr = AliyunSmsHelper.SendSms(phone, kingdeeAcc.smsSignName, kingdeeAcc.smsTemplateCode, "{\"code\":\"" + random + "\"}");
            //{"Message":"OK","RequestId":"21A4D1EC-B088-578E-B433-CF8F5DE64D08","Code":"OK","BizId":"727122474407850833^0"}

            if (!JsonSplit.IsJson(outStr))
            {
                if (string.IsNullOrEmpty(phone))
                    response.message = "短信服务接口返回异常";
            }

            JObject jsonObject = JsonConvert.DeserializeObject<JObject>(outStr);
            if (!"OK".EqualsIgnoreCase(jsonObject.Value<string>("Message")))
            {
                response.message = "发送发生异常：" + outStr;
                return JsonConvert.SerializeObject(response);
            }

            IKCacheManager cacheManager = KCacheManagerFactory.Instance.GetCacheManager(OrderService.GeneratorCache, OrderService.CKD_SMS);
            LogHelper.Debug("保存SMS到缓存" + phone + "=" + random);
            cacheManager.Put(phone, random);
            response.success = 1;
            response.message = "短信发送成功";
            return JsonConvert.SerializeObject(response);
        }


        /// <summary>
        /// 单据数据查询返回,单据号简单加密了的
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public string GetCkdOrder(string no,string ip)
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }

            int fid = OrderService.GetNoToFid(no);
            if (fid < 0)
            {
                return "错误的参数,3";
            }

            string filter = "FID=" + fid;
            DynamicObject order = FunUtil.QuerySingle(KDContext.Session.AppContext, "SAL_OUTSTOCK", filter);
            if (order == null)
            {
                return "单据不存在";
            }

            KingdeeAcc kingdeeAcc = OrderService.GetKingdeeAcc(OrderService.GetWebapiPath(null));
            //单据状态 begin
            string documentStatus = Convert.ToString(order["DocumentStatus"]);
            if (!kingdeeAcc.debug && !"C".EqualsIgnoreCase(documentStatus))
            {
                return "单据未审核";
            }
            //单据状态 end


            //LogHelper.Debug("REMOTE_ADDR=" + Convert.ToString(HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]));
            //LogHelper.Debug("UserHostAddress=" + HttpContext.Current.Request.UserHostAddress);
            
           // string ip = IpUtil.GetIP();
            if (string.IsNullOrEmpty(ip))
            {
                ip = IpUtil.GetIP(HttpContext.Current.Request);
            }
            string ipData = FunUtil.GetIP138Data(kingdeeAcc.ip138Token,ip);

            //销售出库单显示模版路径
            string templatePath = kingdeeAcc.htmlTemplatePath + OrderService.XSCKD_FILENAME;
         
            var template = Engine.LoadTemplate(templatePath);
            template.Context.Debug = true;
            string json = JsonConvert.SerializeObject(order);

            JObject j = JsonConvert.DeserializeObject<JObject>(json);

            System.Collections.Generic.IEnumerable<JProperty> properties = j.Properties();

            foreach (JProperty item in properties)
            {
                if (string.IsNullOrEmpty(item.Name))
                {
                    continue;
                }
                
                if (item.Value == null|| j[item.Name].Type == JTokenType.Null)
                {
                    template.Set(item.Name, string.Empty);
                } else
                {
                    template.Set(item.Name, item.Value);
                }
               
            }

            if (!template.TemplateKey.Contains("F_PCMK_XMMC") && order.Contains("F_PCMK_XMZDD"))
            {
                string F_PCMK_XMMC = string.Empty;
                //F_PCMK_XMZDD
                DynamicObject tmp = order["F_PCMK_XMZDD"] as DynamicObject;
                if (tmp!=null&&tmp.Contains("Name"))
                {
                    F_PCMK_XMMC = Convert.ToString(tmp["Name"]);
                }
                template.Set("F_PCMK_XMMC", F_PCMK_XMMC);
            }
            if (!template.TemplateKey.Contains("F_PCMK_XMMC") && order.Contains("F_PCMK_XMZDD"))
            {
                template.Set("F_PCMK_XMMC", "");
            }

            if (!template.TemplateKey.Contains("SAL_OUTSTOCKENTRY") )
            {
                template.Set("SAL_OUTSTOCKENTRY", new List<Object>());
            }


            template.Set("no", no);
          
            template.Set("Util", util);


            //数据里边手机号填写很乱，这里坐过滤加工处理 begin
            string FLinkPhone = Convert.ToString(order["F_PCMK_LXRTEL"]);

            string F_PCMK_LXRtel = null;

            if (order.Contains("F_PCMK_LXRtel"))
            {
                F_PCMK_LXRtel = Convert.ToString(order["F_PCMK_LXRtel"]);
            }

            string F_PCMK_LXRtel2 = null;
            if (order.Contains("F_PCMK_LXRtel2"))
            {
                F_PCMK_LXRtel2 = Convert.ToString(order["F_PCMK_LXRtel2"]);
            }

            string configStatus = Convert.ToString(order["F_PCMK_ConfigStatus"]);
            string sendPhone = OrderService.GetSelectedPhone(FLinkPhone, F_PCMK_LXRtel, F_PCMK_LXRtel2);
            if (VerifyUtil.IsPhoneNo(sendPhone))
            {
                string mphone = sendPhone.Substring(0, 3) + "***" + sendPhone.Substring(7);
                template.Set("sendPhone", mphone);
                template.Set("sphone", sendPhone);
                template.Set("isBindPhone", true);
            }
            else
            {
                template.Set("sendPhone", "未绑定手机号");
                template.Set("isBindPhone", false);
            }

            if (!string.IsNullOrEmpty(configStatus) && configStatus.Contains("已确认"))
            {
                template.Set("isSubmited", true);
            }
            else
            {
                template.Set("isSubmited", false);
            }
            template.Set("ip", ip);
            template.Set("ipData", ipData);
            return template.Render();

        }

        /// <summary>
        /// 写配置文件
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public string GetConfigPath()
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }
            return OrderService.GetWebapiPath(KDContext.WebContext.Context);
        }

        public string ConfigPath()
        {
            return OrderService.GetWebapiPath(KDContext.WebContext.Context);
        }

        public string SmsSchdule()
        {
            if (KDContext.Session.AppContext == null)
            {
                return "未登陆";
            }
            SmsSchdule smsSchdule = new SmsSchdule();
            smsSchdule.Run(KDContext.Session.AppContext,null);
            return "OK";
        }


        /// <summary>
        /// 提交订单信息
        /// </summary>
        /// <param name="requestBodyString"></param>
        /// <returns></returns>
        public string PostCkd(string requestBodyString,string ip)
        {
            RocResponse<string> response = new RocResponse<string>();
            response.success = 0;
            if (KDContext.Session.AppContext == null)
            {
                response.message = "未登陆";
                return JsonConvert.SerializeObject(response);
            }

            if (string.IsNullOrEmpty(requestBodyString))
            {
                response.message = "错误的参数";
                return JsonConvert.SerializeObject(response);
            }

            if (!JsonSplit.IsJson(requestBodyString))
            {
                response.message = "错误的数据格式";
                return JsonConvert.SerializeObject(response);
            }

            //ip数据begin
            
            KingdeeAcc kingdeeAcc = OrderService.GetKingdeeAcc(OrderService.GetWebapiPath(null));


  

            JObject j = JsonConvert.DeserializeObject<JObject>(requestBodyString);
            string no = j.Value<string>("no");
            int fid = OrderService.GetNoToFid(no);
            if (fid < 0)
            {
                return "错误的参数,3";
            }

            //ip数据 begin
            string ipData = j.Value<string>("ipData");
            if (string.IsNullOrEmpty(ipData))
            {
                if (string.IsNullOrEmpty(ip))
                {
                    ip = IpUtil.GetIP(HttpContext.Current.Request);
                }
                ipData = FunUtil.GetIP138Data(kingdeeAcc.ip138Token, ip);
            }
            //ip数据end

            //签名信息 begin
            string signSendByte = j.Value<string>("SignSendByte");
            if (string.IsNullOrEmpty(signSendByte))
            {
                response.message = "没有手写签名";
                return JsonConvert.SerializeObject(response);
            }
            //签名信息 end

          
            

            //验证码 begin
            string vcode = j.Value<string>("vcode");
            if (string.IsNullOrEmpty(vcode))
            {
                response.message = "验证码没有填写";
                return JsonConvert.SerializeObject(response);
            }

            //验证码 begin
            string phone = OrderService.GetSmsPhone(KDContext.Session.AppContext, fid);
            if (string.IsNullOrEmpty(phone))
            {
                response.message = "错误的手机号";
                return JsonConvert.SerializeObject(response);
            }
            
            IKCacheManager cacheManager = KCacheManagerFactory.Instance.GetCacheManager(OrderService.GeneratorCache, OrderService.CKD_SMS);
            string random = cacheManager.Get<string>(phone);
            if (!kingdeeAcc.debug&&!vcode.EqualsIgnoreCase(random))
            {
                response.message = "验证码错误";
                return JsonConvert.SerializeObject(response);
            }
            //验证码 end


            string filter = "FID=" + fid;
            DynamicObject order = FunUtil.QuerySingle(KDContext.Session.AppContext, "SAL_OUTSTOCK", filter);
            if (order == null)
            {
                response.message = "不存在的单据";
                return JsonConvert.SerializeObject(response);
            }

            //单号
            string billNo = Convert.ToString(order["BillNo"]);

            //单据状态 begin
            string documentStatus = Convert.ToString(order["DocumentStatus"]);
            if (!kingdeeAcc.debug && !"C".EqualsIgnoreCase(documentStatus))
            {
                response.message = "单据未审核";
                return JsonConvert.SerializeObject(response);
            }
            //单据状态 end

            try
            {
               
                //判断是否有附件 begin
                string SendByte = j.Value<string>("SendByte");
                if (!string.IsNullOrEmpty(SendByte))
                {
                    string FileName = j.Value<string>("FileName");
                    if (string.IsNullOrEmpty(FileName))
                    {
                        FileName = DateTime.Now.ToString(DataConvert.DATA_FORMAT);
                    }
                    if (!string.IsNullOrEmpty(SendByte))
                    {
                        string sendByteBase64 = FunUtil.GetUploadBase64Fixed(SendByte);
                        AttachmentDto attachment = new AttachmentDto();
                        attachment.FormId = "SAL_OUTSTOCK";
                        attachment.BillNO = billNo;
                        attachment.IsLast = true;
                        attachment.InterId = Convert.ToString(order["Id"]);
                        attachment.AliasFileName = "签收附件_" + phone + "_" + FileName;
                        attachment.FileName = FileName;
                        attachment.SendByte = sendByteBase64;
                        attachment.EntryinterId = "-1";
                        //新版本8.0 以上才支持 补丁号：PT-146906
                        K3CloudApiClient client = GetK3CloudApiClient(kingdeeAcc);
                        Type myType = client.GetType();
                        MethodInfo attachmentUploadMethod = myType.GetMethod("AttachmentUpload");
                        if (attachmentUploadMethod != null)
                        {
                            object[] parameters = new object[1];
                            parameters[0] = JsonConvert.SerializeObject(attachment);
                            string obj = (string)attachmentUploadMethod.Invoke(client, parameters);
                            if (!FunUtil.IsK3SaveSuccess(obj))
                            {
                                response.message = "单据生成成功," + FunUtil.GetK3ErrorMessage(obj);
                                response.success = 0;
                                LogHelper.Error("签收附件上传返回:" + JsonConvert.SerializeObject(obj));
                                return JsonConvert.SerializeObject(response);
                            }
                            LogHelper.Error("签收附件上传返回:" + JsonConvert.SerializeObject(obj));
                            //client.AttachmentUpload(JsonConvert.SerializeObject(attachment));
                        }
                    }
                }
                //判断是否有附件 end

                CkdVerifyDto dto = new CkdVerifyDto();
                dto.fid = fid + "";
                dto.BillNO = billNo;
                dto.signSendByte = signSendByte;
                dto.ip = ip;
                dto.ipData = ipData;
                dto.phone = phone;

                OrderService.UpdateCkdOrder(KDContext.Session.AppContext, dto);
                response.success = 1;
                response.message = "确认成功";
            }
            catch (Exception e)
            {
                response.success = 0;
                response.message = e.Message;

            }
            string printFormId = "SAL_OUTSTOCK";
            string printTempId =  kingdeeAcc.printTempId;
            string pdfFilePath = AppDomain.CurrentDomain.BaseDirectory + "tempfilePath/" + fid + "_" + DateTime.Now.ToString(DataConvert.DATA_FORMAT) + "" + new Random().Next(100, 999).ToString() + ".pdf";
            bool printStatus = FunUtil.PrintPdf(KDContext.Session.AppContext,printFormId, printTempId, fid, pdfFilePath);
            if (response.success == 1 && printStatus && File.Exists(pdfFilePath))
            {
                byte[] qrdByte = IoUtil.FileStreamRead(pdfFilePath);
                if (qrdByte!=null)
                {
                    AttachmentDto attachment = new AttachmentDto();
                    attachment.FormId = "SAL_OUTSTOCK";
                    attachment.BillNO = billNo;
                    attachment.IsLast = true;
                    attachment.InterId = Convert.ToString(order["Id"]);
                    attachment.AliasFileName = "确认单_" + Convert.ToString(order["Id"]) + "_" + phone;
                    attachment.FileName = "确认单_" + phone + ".pdf";
                    attachment.EntryinterId = "-1";
                    attachment.SendByte = Convert.ToBase64String(qrdByte);

                    //新版本8.0 以上才支持
                    K3CloudApiClient client = GetK3CloudApiClient(kingdeeAcc);
                    Type myType = client.GetType();
                    MethodInfo attachmentUploadMethod = myType.GetMethod("AttachmentUpload");
                    if (attachmentUploadMethod != null)
                    {
                        object[] parameters = new object[1];
                        parameters[0] = JsonConvert.SerializeObject(attachment);
                        string obj = (string)attachmentUploadMethod.Invoke(client, parameters);
                        if (!FunUtil.IsK3SaveSuccess(obj))
                        {
                            response.message =  "单据生成成功," + FunUtil.GetK3ErrorMessage(obj);
                            response.success = 0;
                            LogHelper.Debug("pdf上传返回:" + JsonConvert.SerializeObject(obj));
                            return JsonConvert.SerializeObject(response);
                        }                       
                        printStatus = true;
                    } else
                    {
                        printStatus = false;
                    }
                }
             
            }
            if (response.success==1)
            {
                response.message = "确认成功," + (printStatus == true ? "确认单生成" : "");
            }
            //清理缓存
            if (cacheManager!=null)
            {
                cacheManager.Remove(phone);
            }
            return JsonConvert.SerializeObject(response);
        }

    }
}