using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HttpServices
{
    public class ResponseHelper
    {
        private HttpListenerResponse response;
        public ResponseHelper(HttpListenerResponse response)
        {
            this.response = response;
            OutputStream = response.OutputStream;
        }
        public Stream OutputStream { get; set; }
        public class FileObject
        {
            public FileStream fs;
            public byte[] buffer;
        }
        public void WriteToClient(FileStream fs)
        {
            response.StatusCode = 200;
            byte[] buffer = new byte[1024];
            FileObject obj = new FileObject() { fs = fs, buffer = buffer };
            fs.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(EndWrite), obj);
        }
        void EndWrite(IAsyncResult ar)
        {
            var obj = ar.AsyncState as FileObject;
            var num = obj.fs.EndRead(ar);
            OutputStream.Write(obj.buffer, 0, num);
            if (num < 1)
            {
                obj.fs.Close(); //关闭文件流
                OutputStream.Close();//关闭输出流，如果不关闭，浏览器将一直在等待状态
                return;
            }
            obj.fs.BeginRead(obj.buffer, 0, obj.buffer.Length, new AsyncCallback(EndWrite), obj);
        }

        public void WriteToClient(byte[] data)
        {
            response.StatusCode = 200;
            ////如果是js的ajax请求，还可以设置跨域的ip地址与参数
            //response.AppendHeader("Access-Control-Allow-Origin", "*");//后台跨域请求，通常设置为配置文件
            //response.AppendHeader("Access-Control-Allow-Headers", "ID,PW");//后台跨域参数设置，通常设置为配置文件
            //response.AppendHeader("Access-Control-Allow-Method", "post");//后台跨域请求设置，通常设置为配置文件
            response.ContentType = "text/plain;charset=UTF-8";//告诉客户端返回的ContentType类型为纯文本格式，编码为UTF-8
            response.AddHeader("Content-type", "text/plain;charset=UTF-8");//添加响应头信息
            response.ContentEncoding = Encoding.UTF8;

            try
            {
                OutputStream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"网络蹦了：{ex.ToString()}");
            }
            finally
            {
                OutputStream.Close();//关闭输出流，如果不关闭，浏览器将一直在等待状态
            }

        }

        
    }
}
