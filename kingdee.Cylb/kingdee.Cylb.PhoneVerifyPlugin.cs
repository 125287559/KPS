
using System.ComponentModel;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;
using Kingdee.Cyext;
/// <summary>
///
/// </summary>
namespace kingdee.Cylb
{
    [Description("手机号验证"), HotUpdate]
    public class PhoneVerifyPlugin : AbstractOperationServicePlugIn
    {

        public PhoneVerifyPlugin() : base()
        {

           
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            foreach (var order in e.DataEntitys)
            {
                //单据类型：FBillTypeID(必填项)
                string billType = FunUtil.GetDynamicObjectName(order["BillTypeID"]);
                if ("外贸销售出库单".Equals(billType))
                {
                    continue;
                }

                if ("外贸发货通知单".Equals(billType))
                {
                    continue;
                }

                if (billType.Contains("外贸"))
                {
                    continue;
                }


                //数据里边手机号填写很乱，这里坐过滤加工处理 begin FLinkPhone 
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

                string sendPhone = OrderService.GetSelectedPhone(FLinkPhone, F_PCMK_LXRtel, F_PCMK_LXRtel2);
                if (string.IsNullOrEmpty(sendPhone))
                {
                    throw new Exception("联系人手机号必须填写正确，3个中最少保证一个有效可用,号码中间不能有空格等字符");
                }

            }


        }

    }
}