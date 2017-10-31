using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aop.Api.Util;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Parser;
using Aop.Api;
using 支付宝支付.Help;
using System.Collections.Specialized;

namespace 支付宝支付.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Query()
        {
            return View();
        }
        public ActionResult Refund()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AlipayTradeRefund(string trade_no, string out_trade_no, string refund_amount, string refund_reason, string out_request_no, string operator_id)
        {
            if (string.IsNullOrEmpty(refund_amount))
            {
                Response.Write("<script language='javascript'>alert('金额必须填写！');</script>");
                return null;
            }
            if (string.IsNullOrEmpty(trade_no) && string.IsNullOrEmpty(out_trade_no))
            {
                Response.Write("<script language='javascript'>alert('至少填写一种订单号，二者不可同时为空！');</script>");
                return null;
            }
            IAopClient client = new DefaultAopClient(AlipayConfig.URL, AlipayConfig.seller_id, AlipayConfig.private_keyPem, "JSON", AlipayConfig.version, AlipayConfig.sign_type, AlipayConfig.Alipay_public_key, AlipayConfig.input_charset, true);
            AlipayTradeRefundRequest request = new AlipayTradeRefundRequest();
            request.BizContent = "{\"out_trade_no\":\"" + out_trade_no + "\","
                + "\"trade_no\":\"" + trade_no + "\","
                + "\"refund_amount\":\""+ refund_amount+"\","
                + "\"refund_reason\":\""+refund_reason+"\","
                + "\"out_request_no\":\""+out_request_no+"\","
                + "\"operator_id\":\""+operator_id+"\"}";
            AlipayTradeRefundResponse response = client.Execute(request);
            return Json(response.Body);
        }

        [HttpPost]
        public JsonResult AlipayTradeQuery(string trade_no, string out_trade_no)
        {
            if (string.IsNullOrEmpty(trade_no) && string.IsNullOrEmpty(out_trade_no))
            {
                Response.Write("<span style='color:#FF0000;font-size:20px'>" + "至少填写一种订单号，二者不可同时为空" + "</span>");
                return null;
            }
            IAopClient client = new DefaultAopClient(AlipayConfig.URL, AlipayConfig.seller_id, AlipayConfig.private_keyPem, "JSON", AlipayConfig.version, AlipayConfig.sign_type, AlipayConfig.Alipay_public_key, AlipayConfig.input_charset, true);
            AlipayTradeQueryRequest request = new AlipayTradeQueryRequest();
            request.BizContent = "{\"out_trade_no\":\"" + out_trade_no + "\"," +"\"trade_no\":\"" + trade_no + "\"}";
            AlipayTradeQueryResponse response = client.Execute(request);
            return Json(response.Body);

        }

        public void AlipayTradePay()
        {
            string out_trade_no = Guid.NewGuid().ToString();
            string total_amount = "50000.00";
            string subject = "测试项目";
            string body = "测试项目";
            IAopClient client = new DefaultAopClient(AlipayConfig.URL, AlipayConfig.seller_id, AlipayConfig.private_keyPem, "JSON", AlipayConfig.version, AlipayConfig.sign_type, AlipayConfig.Alipay_public_key, AlipayConfig.input_charset, true);
            AlipayTradePagePayRequest request = new AlipayTradePagePayRequest();
            request.SetReturnUrl(AlipayConfig.return_url);
            request.BizContent = "{\"out_trade_no\":\"" + out_trade_no + "\","
            + "\"total_amount\":\"" + total_amount + "\","
            + "\"subject\":\"" + subject + "\","
            + "\"body\":\"" + body + "\","
            + "\"product_code\":\"FAST_INSTANT_TRADE_PAY\"}";
            AlipayTradePagePayResponse response = client.pageExecute(request);
            string form = response.Body;
            Response.Write(form);
        }
        [HttpGet]
        public void SignaturesURL()
        {
            //接收客户端发来的订单信息.
            SortedDictionary<string, string> sPara = GetRequestPara();
            //获取订单信息
            string partner = sPara["app_id"];
            string seller_id = sPara["seller_id"];
            string total_amount = sPara["total_amount"];
            string out_trade_no = sPara["out_trade_no"];
            //判断partner和service信息匹配成功.
            if (partner != null && seller_id != null && out_trade_no != null && out_trade_no!=null)
            {
                if (partner.Replace("\"", "") == AlipayConfig.partner)//seller_id == seller_id && out_trade_no == out_trade_no && out_trade_no==out_trade_no
                {
                    #region MyRegion  一般把写成功把支付成功的页面提示展示给用户，不要处理业务代码，业务代码一定要放到 异步通知 里处理
                    //将获取的订单信息，按照“参数=参数值”的模式用“&”字符拼接成字符串.
                    string data = WebUtils.BuildQuery(sPara, AlipayConfig.input_charset);
                    //使用商户的私钥进行RSA签名，并且把sign做一次urleccode.
                    string sign = HttpUtility.UrlEncode(AopUtils.SignAopRequest(sPara, AlipayConfig.private_keyPem, AlipayConfig.input_charset, true, AlipayConfig.sign_type));
                    //拼接请求字符串（注意：不要忘记参数值的引号）.
                    data = data + "&sign=\"" + sign + "\"&sign_type=\"" + AlipayConfig.sign_type + "\"";
                    //返回给客户端请求.
                    Response.Write(data); 
                    #endregion

                }
                else
                {
                    Response.Write("订单信息不匹配!");
                }
            }
            else
            {
                Response.Write("无客户端请求!");
            }
        }
        [HttpPost]
        public void NotifyURL(HttpRequestBase request)
        {
            SortedDictionary<string, string> sPara = GetRequestPara();
            string notify_id = Request.Form["notify_id"];//获取notify_id

            string sign = Request.Form["sign"];//获取sign
            
            if (notify_id != null && notify_id != "")//判断是否有带返回参数
            {
                Notify aliNotify = new Notify();
                if (aliNotify.GetResponseTxt(notify_id) == "true")
                {
                    if (AlipaySignature.RSACheckV1(sPara, AlipayConfig.alipay_public_keyPem,AlipayConfig.input_charset,AlipayConfig.sign_type,true))
                    {
                        //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                        //获取支付宝的通知返回参数，可参考技术文档中服务器异步通知参数列表

                        //商户订单号
                        string out_trade_no = Request.Form["out_trade_no"];

                        //支付宝交易号
                        string trade_no = Request.Form["trade_no"];

                        //交易状态
                        string trade_status = Request.Form["trade_status"];

                        if (Request.Form["trade_status"] == "TRADE_FINISHED")
                        {
                            //判断该笔订单是否在商户网站中已经做过处理
                            //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                            //请务必判断请求时的out_trade_no、total_fee、seller_id与通知时获取的out_trade_no、total_fee、seller_id为一致的
                            //如果有做过处理，不执行商户的业务程序

                            //注意：
                            //退款日期超过可退款期限后（如三个月可退款），支付宝系统发送该交易状态通知
                        }
                        else if (Request.Form["trade_status"] == "TRADE_SUCCESS")
                        {
                            //判断该笔订单是否在商户网站中已经做过处理
                            //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                            //请务必判断请求时的out_trade_no、total_fee、seller_id与通知时获取的out_trade_no、total_fee、seller_id为一致的
                            //如果有做过处理，不执行商户的业务程序

                            //注意：
                            //付款完成后，支付宝系统发送该交易状态通知
                        }
                        //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——
                        Response.Write("success");  //请不要修改或删除
                    }
                    else
                    {
                        Response.Write("sign fail!");
                    }
                }
                else
                {
                    Response.Write("response fail!");
                }
            }
            else
            {
                Response.Write("非通知参数!");
            }
        }
        /// <returns>request回来的信息组成的数组</returns>
        public SortedDictionary<string, string> GetRequestPara()
        {
            int i = 0;
            SortedDictionary<string, string> sArraytemp = new SortedDictionary<string, string>();
            NameValueCollection coll;
            coll = (!(Request.HttpMethod == "POST") ? Request.QueryString : Request.Form);

			SortedDictionary<string, string> strs = new SortedDictionary<string, string>();
			string[] allKeys = coll.AllKeys;
			for (i = 0; i < allKeys.Length; i++)
			{
				strs.Add(allKeys[i], coll[allKeys[i]]);
			}
			return strs;
		}

    }
}
