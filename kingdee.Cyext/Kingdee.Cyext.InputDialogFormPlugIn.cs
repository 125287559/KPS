
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kingdee.Cyext
{
    [Description("通用输入对话框"), HotUpdate]
    public class InputDialogFormPlugIn : AbstractDynamicFormPlugIn
    {
        public InputDialogFormPlugIn() : base()
        {

        }

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            BOS.Orm.DataEntity.DynamicObject dynamicObject = this.View.Model.DataObject;
            IDataEntityType entityType = dynamicObject.GetDataEntityType();
            List<string> fieldList = new List<string>();
            //遍历所有字段begin
            IDataEntityPropertyCollection propertyCollection = entityType.Properties;
            foreach (IDataEntityProperty entityProperty in propertyCollection)
            {
                fieldList.Add(entityProperty.Name);
            }
            //遍历所有字段end

            //取值begin
            Dictionary<string, object> returnData = new Dictionary<string, object>();
            foreach (string key in fieldList)
            {
                var value = this.View.Model.GetValue(key);
                returnData.Add(key, value);
            }
            //取值end

            var result = new
            {
                key = e.Key,
                data = returnData
            };

            this.View.ReturnToParentWindow(JsonConvert.SerializeObject(result));
        }

    }
}
