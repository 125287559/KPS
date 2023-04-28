using Kingdee.BOS.Contracts;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using CSharp.jspxnet;
using System.ComponentModel;
using Kingdee.BOS.Util;

/// <summary>
/// 定时任插件 kingdee.Cylb.SmsSchdule
/// https://vip.kingdee.com/article/72775810716762112?productLineId=1
/// 5. 系统中可以筛选出哪些发货单还没有签收（比如筛选已发货满6天还未签收），批量给这些客户发送短信，提醒签收，附上二维码的链接，客户在短信里点一下链接就可以进去签收页面。
/// </summary>
namespace kingdee.Cylb
{
    [Description("定时判断是否超期发送短信"), HotUpdate]
    public class SmsSchdule : IScheduleService
    {
        public SmsSchdule()
        {

        }
      
        public void Run(Context ctx, Schedule schedule)
        {
            LogHelper.Debug("SmsSchdule Run");
            OrderService.CheckSendSms(ctx);
        }
    }
}
