

/*
 * 
 
{
  "FormId": "BOS_Attachment",
	"data": {
       "Model": {
            "FFileId": "4a072355a22342d5ae3c569a12d51734",
			"FAttachmentName": "test123.txt",
			"FBillType": "PUR_PurchaseOrder",
			"FInterID": "101827",
			"FBillNo": "CGDD000949-2003",
			"FAttachmentSize": 64.66,
			"FExtName": ".txt",
			"FEntryinterId": "-1",
			"FEntrykey": " ",
			"FaliasFileName": "别名",
			"FCreateMen": {
                "FUserID": "100008"
            },
			"FCreateTime": "2021-12-30 15:15:20"
        }
    }
}
*/
namespace Kingdee.Cyext
{
    public class BillBindAttachmentDto
    {

        //固定 BOS_Attachment
        public string FormId = "BOS_Attachment";

        //FFileId
        public string FFileId;

        //文件名
        public string FAttachmentName;

        //单据类型,单据FROMID
        public string FBillType;

        //单据内码
        public string InterId;


        //单据编号
        public string FBillNo;

        //附件大小 单位kb，上传接口返回
        public double FAttachmentSize;

        //扩展名
        public string FExtName;

        //位置,单据体 -1
        public string FEntryinterId = "-1";

        //表头未空格
        public string FEntrykey = " ";

        //别名
        public string FaliasFileName;

        //分录内码
        public string FCreateTime;
    
    }
}
