using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 支付宝支付.Help
{
    public class AlipayConfig
    {
        // 合作身份者ID，签约账号，以2088开头由16位纯数字组成的字符串，查看地址：https://openhome.alipay.com/platform/keyManage.htm?keyType=partner
        public static string partner = "";
        // 收款支付宝账号，以2088开头由16位纯数字组成的字符串，一般情况下收款账号就是签约账号
        public static string seller_id = partner;
        //商户的私钥,原始格式，RSA公私钥生成：https://doc.open.alipay.com/doc2/detail.htm?spm=a219a.7629140.0.0.nBDxfy&treeId=58&articleId=103242&docType=1
        public static string private_keyPem = GetCurrentPath() + "rsa_private_key.pem";
        //应用公钥，查看地址：https://b.alipay.com/order/pidAndKey.htm 
        public static string alipay_public_keyPem = GetCurrentPath() + "rsa_public_key.pem";
        //支付宝公钥,为填写（配置）应用公钥后支付宝生成的
        public static string Alipay_public_key = GetCurrentPath() + "rsa_Alipay_public_key.pem";
        // 签名方式
        public static string sign_type = "RSA";
        // 字符编码格式 目前支持 gbk 或 utf-8
        public static string input_charset = "utf-8";

        public static string URL = "https://openapi.alipaydev.com/gateway.do";//调用的接口，此接口为沙盒接口 正式接口：https://openapi.alipay.com/gateway.do
        // 调用接口版本 ，无需修改
        public static string version = "1.0";
        // 调用的接口名
        public static string service = "alipay.trade.page.pay";//网页在线支付
        public static string notify_url = "http://localhost/Home/NotifyURL"; //异步通知地址
        public static string return_url = "http://localhost/Home/SignaturesURL";//同步通知页面
        private static string GetCurrentPath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            return basePath + "/App_Data/";
        }
    }

}