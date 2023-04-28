using CSharp.jspxnet;
using System;
using System.Dynamic;

namespace Kingdee.Cyext
{
    public class Test
    {


        public static void Main(string[] args)
        {
            DynamicObject dobj = null;


            TemplateUtil templateUtil = new TemplateUtil();

            Console.WriteLine("-------outStr=" + templateUtil.IsEmpty(dobj));

        }
    }
}