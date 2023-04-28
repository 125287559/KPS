

using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace Kingdee.Cyext
{
    [Description("CY扩展模版功能扩展"), HotUpdate]
    public class TemplateUtil
    {
        public TemplateUtil()
        {

        }

        public bool IsEmpty(Object o)
        {
            if (o==null)
            {
                return true;
            }
            if (o.GetType().Equals("System.String"))
            {
                if (string.IsNullOrEmpty((string)o))
                {
                    return true;
                }
            }

            return false;
        }


    }
}
