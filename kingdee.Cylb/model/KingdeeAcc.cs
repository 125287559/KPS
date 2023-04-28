using CSharp.jspxnet;
using CSharp.jspxnet.utils;
using System;
using System.IO;
using System.Xml;

/// <summary>
/// 加载数据
/// </summary>
public struct KingdeeAcc
{
    public string dbId;
    public string userName;
    public string pwd;
    public int lCid;
    public string url;
    public string key;
    public bool debug;
    public bool load;

    //短信配置
    public string smsAccessKeyId;
    public string smsAccessKeySecret;
    public string smsSignName; 
    //短信验证吗模版
    public string smsTemplateCode;

    //短信通知模版
    public string smsTzTemplateCode;


    //模版路径
    public string htmlTemplatePath;

    //ip138Token
    public string ip138Token;


    //发货时间字段，没有就默认审核时间
    public string sendDateField;

    //短信发送时间，默认未6天
    public int smsDay;

    public string printTempId;

 

    public void Init(string path)
    {
         
        //监控代码段
        if (File.Exists(path))
        try
        {
            //创建对象
            XmlDocument xmlDoc = new XmlDocument();
            //加载Xml文件
            xmlDoc.Load(path);
            
            //获取节点信息
            XmlNode rootNode = xmlDoc.SelectSingleNode("config");
            XmlNode webRootNode = rootNode.SelectSingleNode("WebApiConfig");
            //读取相应配置
            dbId = XmlUtil.GetXmlNodeValue(webRootNode, "dbId");
            userName = XmlUtil.GetXmlNodeValue(webRootNode, "userName");
            pwd = XmlUtil.GetXmlNodeValue(webRootNode, "pwd");
            lCid = Convert.ToInt16(XmlUtil.GetXmlNodeValue(webRootNode, "lCid"));
            url = XmlUtil.GetXmlNodeValue(webRootNode, "url");
            debug =DataConvert.ToBoolean(XmlUtil.GetXmlNodeValue(webRootNode, "debug"));
                
            smsAccessKeyId = XmlUtil.GetXmlNodeValue(webRootNode, "smsAccessKeyId");

            smsAccessKeySecret = XmlUtil.GetXmlNodeValue(webRootNode, "smsAccessKeySecret");

            smsSignName = XmlUtil.GetXmlNodeValue(webRootNode, "smsSignName");

            smsTemplateCode = XmlUtil.GetXmlNodeValue(webRootNode, "smsTemplateCode");


            smsTzTemplateCode = XmlUtil.GetXmlNodeValue(webRootNode, "smsTzTemplateCode");


            printTempId = XmlUtil.GetXmlNodeValue(webRootNode, "printTempId");
   

            htmlTemplatePath = XmlUtil.GetXmlNodeValue(webRootNode, "htmlTemplatePath");
            if (!string.IsNullOrEmpty(htmlTemplatePath) && !htmlTemplatePath.EndsWith("/"))
            {
                    htmlTemplatePath = htmlTemplatePath + "/";
            }
            
            ip138Token = XmlUtil.GetXmlNodeValue(webRootNode, "ip138Token");

            sendDateField = XmlUtil.GetXmlNodeValue(webRootNode, "sendDateField");
            if (string.IsNullOrEmpty(sendDateField))
            {
                sendDateField = "FAPPROVEDATE";
            }

            smsDay = DataConvert.ToInt(XmlUtil.GetXmlNodeValue(webRootNode, "smsDay"),6);

            load = true;
        }
        catch (Exception ex)
        {
                load = false;
                LogHelper.Debug("KingdeeAcc 读取配置文件失败," + ex.Message);
        }
        if (string.IsNullOrEmpty(userName))
        {
            DefaultConfig();
        }
    }

    public void DefaultConfig()
    {
        LogHelper.Debug("KingdeeAcc 读取配置文件失败");
        load = false;
    }

    override
    public string ToString()
    {
        string str = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<config>\n" +
                "\t<WebApiConfig>\n" +
                "\t\t    <dbId>" + dbId + "</dbId>\n" +
                "\t\t\t<userName>" + userName + "</userName>\n" +
                "\t\t\t<pwd>" + pwd + "</pwd>\n" +
                "\t\t\t<lCid>" + lCid + "</lCid>\n" +
                "\t\t\t<url>" + url + "</url>\n" +
                "\t</WebApiConfig>\n" +
                "\t<key>" + key + "</key>\n" +
                "\t<debug>" + debug.ToString() + "</debug>\n" +
                "</config>";
        return str;
    }
}