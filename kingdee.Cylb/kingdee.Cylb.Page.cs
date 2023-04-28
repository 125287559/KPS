using CSharp.jspxnet;
using CSharp.jspxnet.utils;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.IO;
using System.Web;

/// <summary>
/// 出库单页面
/// https://www.ik3cloud.com/K3cloud/kingdee.Cylb.Page.Ckd,kingdee.Cylb.common.kdsvc
/// 发短信
/// https://www.ik3cloud.com/K3cloud/kingdee.Cylb.Page.Sms,kingdee.Cylb.common.kdsvc
/// </summary>
namespace kingdee.Cylb
{
    [Description("页面接口"), HotUpdate]
    public class Page : AbstractWebApiBusinessService
    {
        private static K3CloudApiClient client;
        private  KingdeeAcc kingdeeAcc;
        public Page(KDServiceContext context)
            : base(context)
        {
            kingdeeAcc = OrderService.GetKingdeeAcc(OrderService.GetWebapiPath(context.WebContext.Context));
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        private K3CloudApiClient GetK3CloudApiClient()
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
                if (kingdeeAcc.load)
                {
                    OrderService.SaveCacheKingDeeAcc(kingdeeAcc);
                }
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
        public void Sms()
        {
            string no = HttpContext.Current.Request.QueryString["n"];
            LogHelper.Debug("no=" + no);
            client = GetK3CloudApiClient();
            string outStr = client.Execute<string>("kingdee.Cylb.NotifyService.SendSms,kingdee.Cylb", new object[] {no});
            WriteResponseJson(outStr);
        }

        /// <summary>
        /// 看单据
        /// n is id虚混   http://nbkps.gnway.cc/k3cloud/cv/z.aspx?n=961000037F
        /// UPDATE T_SAL_OUTSTOCK SET F_PCMK_QRCoder=concat('http://192.168.0.220/K3cloud/kingdee.Cylb.Page.Ckd,kingdee.Cylb.common.kdsvc?n=',RIGHT(newid(),2),FID-123,RIGHT(newid(),2))
        /// </summary>
        public void Ckd()
        {
            string no = System.Web.HttpContext.Current.Request.QueryString["n"];
            LogHelper.Debug("no=" + no);
            string ip = IpUtil.GetIP(HttpContext.Current.Request);
            client = GetK3CloudApiClient();
            string outStr = client.Execute<string>("kingdee.Cylb.NotifyService.GetCkdOrder,kingdee.Cylb", new object[] {no,ip});
            WriteResponseHtml(outStr);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        public void PostCkd()
        {
            Stream postData = HttpContext.Current.Request.InputStream;
            postData.Position = 0; //这一句很重要，不然一直是空
            StreamReader sRead = new StreamReader(postData,System.Text.Encoding.UTF8);
            string requestBodyString = sRead.ReadToEnd();
            sRead.Close();

            string ip = IpUtil.GetIP(HttpContext.Current.Request);
            if (string.IsNullOrEmpty(requestBodyString))
            {
                return;
            }
            client = GetK3CloudApiClient();
            string outStr = client.Execute<string>("kingdee.Cylb.NotifyService.PostCkd,kingdee.Cylb", new object[] { requestBodyString, ip});

            WriteResponseJson(outStr);
        }

        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="data"></param>
        private void WriteResponseHtml(string data)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.ClearHeaders();
            response.Charset = "UTF-8";
            response.ContentType = "text/html; charset=UTF-8";
            response.AddHeader("Content-Type", "text/html; charset=UTF-8");
            response.AddHeader("ContentType", "text/html");
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.Write(data);
            response.Flush();
            response.Close();
        }

        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="data"></param>
        private void WriteResponseJson(string data)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.ClearHeaders();
            response.Charset = "UTF-8";
            response.ContentType = "text/html; charset=UTF-8";
            response.AddHeader("Content-Type", "application/json; charset=UTF-8");
            response.AddHeader("ContentType", "application/json");
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.Write(data);
            response.Flush();
            response.Close();
        }
    }
}