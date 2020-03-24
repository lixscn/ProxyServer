using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WebManageLibrary
{
    public class HttpHandler
    {
        HttpListenerRequest _request = null;
        HttpListenerResponse _response = null;

        public HttpListenerRequest Request { get => _request; set => _request = value; }
        public HttpListenerResponse Response { get => _response; set => _response = value; }

        public HttpHandler(HttpListenerRequest request, HttpListenerResponse response)
        {
            Request = request;
            Response = response;
        }

        /// <summary>
        /// 
        /// </summary>
        public void HandleRequest()
        {
            

            string guid = Guid.NewGuid().ToString();
            string responseString = "";
            if (Request.HttpMethod == "POST" && Request.InputStream != null)
            {
                //处理客户端发送的请求并返回处理信息
                responseString = GetRequestPost();
            }
            else
            {
                responseString = GetRequestGet();
            }
            var returnByteArr = Encoding.UTF8.GetBytes(responseString);//设置客户端返回信息的编码
            try
            {
                using (var stream = Response.OutputStream)
                {
                    //把处理信息返回到客户端
                    stream.Write(returnByteArr, 0, returnByteArr.Length);
                    stream.Flush();
                }
                Response.Close();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"网络蹦了：{ex.ToString()}");
            }
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"请求处理完成：{guid},时间：{ DateTime.Now.ToString()}\r\n");

            
        }

        private string GetRequestPost()
        {
            string data = null;
            try
            {
                var byteList = new List<byte>();
                var byteArr = new byte[2048];
                int readLen = 0;
                int len = 0;
                //接收客户端传过来的数据并转成字符串类型
                do
                {
                    readLen = Request.InputStream.Read(byteArr, 0, byteArr.Length);
                    len += readLen;
                    byteList.AddRange(byteArr);
                } while (readLen != 0);
                data = Encoding.UTF8.GetString(byteList.ToArray(), 0, len);

                //获取得到数据data可以进行其他操作
            }
            catch (Exception ex)
            {
                Response.StatusDescription = "404";
                Response.StatusCode = 404;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"在接收数据时发生错误:{ex.ToString()}");
                return $"在接收数据时发生错误:{ex.ToString()}";//把服务端错误信息直接返回可能会导致信息不安全，此处仅供参考
            }
            Response.StatusDescription = "200";//获取或设置返回给客户端的 HTTP 状态代码的文本说明。
            Response.StatusCode = 200;// 获取或设置返回给客户端的 HTTP 状态代码。
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"接收数据完成:{data.Trim()},时间：{DateTime.Now.ToString()}");
            return $"接收数据完成";
        }

        private string GetRequestGet()
        {
            string rawUrl = Request.RawUrl; ;
            try
            {
                //进行URL请求处理，读取硬盘里的网页文件
                int counter = 0;
                string line;

                System.IO.StreamReader file = new System.IO.StreamReader(@"D:\workspace\netcore\public\index.html");
                while ((line = file.ReadLine()) != null)
                {
                    System.Console.WriteLine(line);
                    counter++;
                }

                file.Close();
                Console.WriteLine("There were {0} lines.", counter);
            }
            catch (Exception ex)
            {
                Response.StatusDescription = "404";
                Response.StatusCode = 404;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"在接收数据时发生错误:{ex.ToString()}");
                return $"在接收数据时发生错误:{ex.ToString()}";//把服务端错误信息直接返回可能会导致信息不安全，此处仅供参考
            }
            Response.StatusDescription = "200";//获取或设置返回给客户端的 HTTP 状态代码的文本说明。
            Response.StatusCode = 200;// 获取或设置返回给客户端的 HTTP 状态代码。
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"取URL:{rawUrl.Trim()},时间：{DateTime.Now.ToString()}");
            return $"接收数据完成";
        }

    }
}
