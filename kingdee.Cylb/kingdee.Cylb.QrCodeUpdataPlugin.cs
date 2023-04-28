
using System.ComponentModel;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core;
using System;

/// <summary>
///云部署限制太多，这里开启sql测试
/// </summary>
namespace kingdee.Cylb
{
    [Description("更新二维码"), HotUpdate]
    public class QrCodeUpdataPlugin : AbstractOperationServicePlugIn
    {

        public QrCodeUpdataPlugin() : base()
        {

           
        }

        /**
        * 数据写入
        * */

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            //更新数据
            if (e.DataEntitys != null && this.FormOperation.Operation.EqualsIgnoreCase("save"))
            {
                foreach (ExtendedDataEntity extended in e.SelectedRows)
                {
                    DynamicObject dataEntity = extended.DataEntity;
                    int FId = Convert.ToInt32(dataEntity["Id"]);
                    OrderService.UpdateOrderQr(this.Context, FId);
                }
                    
            }
        }

    }
}