using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HttpServices
{
    public class ServerHelper
    {
        HttpListener httpListener = new HttpListener();
        public void Setup(int port = 8080)
        {
            try
            {
                httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                //httpListener.Prefixes.Add(string.Format("http://*:{0}/", port));//如果发送到8080 端口没有被处理，则这里全部受理，+是全部接收，有可能拒绝访问，可以试一下管理员
                httpListener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", port));
                httpListener.Start();//开启服务

                Receive();//异步接收请求
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Receive()
        {
            httpListener.BeginGetContext(new AsyncCallback(EndReceive), null);
        }

        void EndReceive(IAsyncResult ar)
        {
            var context = httpListener.EndGetContext(ar);
            Dispather(context);//解析请求
            Receive();
        }

        RequestHelper RequestHelper;
        ResponseHelper ResponseHelper;
        void Dispather(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;


            RequestHelper = new RequestHelper(request);
            ResponseHelper = new ResponseHelper(response);
            RequestHelper.DispatchResources(ResponseHelper);
        }
    }
}
