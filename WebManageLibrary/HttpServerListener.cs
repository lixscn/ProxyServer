using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace WebManageLibrary
{
    public class HttpServerListener
    {
        public static string ListenerUrl = "http://localhost:8080/";

        public void OnStart()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(ListenerUrl);
            httpListener.Start();

            httpListener.BeginGetContext(new AsyncCallback(GetContextCallback), httpListener);
        }

        private void GetContextCallback(IAsyncResult async)
        {
            HttpListener listener = (HttpListener)async.AsyncState;
            HttpListenerContext context = listener.EndGetContext(async);
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.

            ////如果是js的ajax请求，还可以设置跨域的ip地址与参数
            //context.Response.AppendHeader("Access-Control-Allow-Origin", "*");//后台跨域请求，通常设置为配置文件
            //context.Response.AppendHeader("Access-Control-Allow-Headers", "ID,PW");//后台跨域参数设置，通常设置为配置文件
            //context.Response.AppendHeader("Access-Control-Allow-Method", "post");//后台跨域请求设置，通常设置为配置文件
            context.Response.ContentType = "text/plain;charset=UTF-8";//告诉客户端返回的ContentType类型为纯文本格式，编码为UTF-8
            context.Response.AddHeader("Content-type", "text/plain");//添加响应头信息
            context.Response.ContentEncoding = Encoding.UTF8;

            HttpHandler httpHandler = new HttpHandler(request, response);

            Thread thread = new Thread(httpHandler.HandleRequest);
            thread.Start();

            listener.BeginGetContext(new AsyncCallback(GetContextCallback), listener);
        }

        
    }
}
