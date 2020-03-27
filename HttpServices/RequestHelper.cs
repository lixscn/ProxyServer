using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HttpServices
{
    public class RequestHelper
    {
        private HttpListenerRequest request;
        public RequestHelper(HttpListenerRequest request)
        {
            this.request = request;
        }
        public Stream RequestStream { get; set; }
        public void ExtracHeader()
        {
            RequestStream = request.InputStream;
        }

        public delegate void ExecutingDispatchToFs(FileStream fs);
        public delegate void ExecutingDispatchToByte(byte[] data);

        public void DispatchResources(ResponseHelper ResponseHelper)
        {
            //var rawUrl = request.RawUrl;//资源默认放在执行程序的wwwroot文件下，默认文档为index.html
            if (request.HttpMethod == "POST" && request.InputStream != null)
            {
                //POST处理
                RequestPOST(data =>
                {
                    ResponseHelper.WriteToClient(data);  //发送数据包方式
                });
            }
            else
            {
                //GET处理
                RequestGet(fs =>
                {
                    ResponseHelper.WriteToClient(fs);// 对相应的请求做出回应
                });
            }
        }

        private void RequestPOST(ExecutingDispatchToByte action)
        {
            string dataString = GetDatas();
             
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"接收数据完成:{dataString.Trim()},时间：{DateTime.Now.ToString()}");
            //return $"接收数据完成";
            byte[] data = Encoding.UTF8.GetBytes(dataString);

            action?.Invoke(data);

        }

        private void RequestGet(ExecutingDispatchToFs action)
        {
            var rawUrl = request.RawUrl;//资源默认放在执行程序的wwwroot文件下，默认文档为index.html
            string filePath = string.Format(@"{0}/wwwroot{1}", Environment.CurrentDirectory, rawUrl);//这里对应请求其他类型资源，如图片，文本等
            if (rawUrl.Length == 1)
                filePath = string.Format(@"{0}/wwwroot/index.html", Environment.CurrentDirectory);//默认访问文件
            try
            {
                if (File.Exists(filePath))
                {
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

                    action?.Invoke(fs);

                }
            }
            catch{return; }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <returns></returns>
        private string GetDatas()
        {
            string data = null;
            //bool bo = false;
            try
            {
                var byteList = new List<byte>();
                var byteArr = new byte[2048];
                int readLen = 0;
                int len = 0;
                //接收客户端传过来的数据并转成字符串类型
                do
                {
                    readLen = request.InputStream.Read(byteArr, 0, byteArr.Length);
                    len += readLen;
                    byteList.AddRange(byteArr);
                } while (readLen != 0);
                data = Encoding.UTF8.GetString(byteList.ToArray(), 0, len);

                //获取得到数据data可以进行其他操作
               //bo = true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"在接收数据时发生错误:{ex.ToString()}");
                data = string.Format("在接收数据时发生错误:{0}", ex.ToString());

                //bo = false;//把服务端错误信息直接返回可能会导致信息不安全，此处仅供参考
            }
            return data;
        }

        public void ResponseQuerys()
        {
            var querys = request.QueryString;
            foreach (string key in querys.AllKeys)
            {
                VarityQuerys(key, querys[key]);
            }
        }

        private void VarityQuerys(string key, string value)
        {
            switch (key)
            {
                case "pic": Pictures(value); break;
                case "text": Texts(value); break;
                default: Defaults(value); break;
            }
        }

        private void Pictures(string id)
        {

        }

        private void Texts(string id)
        {

        }

        private void Defaults(string id)
        {

        }

    }
}
