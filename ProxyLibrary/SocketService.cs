﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static ProxyLibrary.StateObject;

namespace ProxyLibrary
{
    public class SocketService : BaseSocketService
    {
        public Socket listener = null;
        public string RemoteHost = "127.0.0.1";
        public Int32 RemotePost = 1080;
        

        public SocketService(string address,Int32 port, CONNMODEL connModel, string remoteHost, Int32 remotePost)
        {
            Address = address;
            Port = port;
            ConnModel = connModel;
            RemoteHost = remoteHost;
            RemotePost = remotePost;
        }

        public SocketService(string address, Int32 port, CONNMODEL connModel)
        {
            Address = address;
            Port = port;
            ConnModel = connModel;
        }

        public SocketService()
        {

        }

        public bool OnStart()
        {
            IPAddress ipAssress = null;
            IPEndPoint lep = null;
            
            bool bo = false;
            try
            {
                //IPHostEntry lipa = Dns.Resolve("www.163.com");
                //IPEndPoint lep = new IPEndPoint(lipa.AddressList[0], 5555);
                ipAssress = IPAddress.Parse(Address);
                lep = new IPEndPoint(ipAssress, Port);
            
                listener = new Socket(lep.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                LOGGER.Info("服务器启动成功!!");
                bo = true;
            }
            catch (Exception ex)
            {
                LOGGER.Info("服务器启动失败!错误:{0}", ex.Message);
            }
            try
            {
                listener.Bind(lep);
                listener.Listen(1000);


                Console.WriteLine("Waiting for a connection...");
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                WriteLog(ex.Message);
            }

            return bo;

        }
        public bool OnStop()
        {
            try
            {
                listener.Shutdown(SocketShutdown.Both);
            }
            catch { }
            finally
            {
                listener.Close();
            }
            return true;
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            //获取处理客户端请求的套接字。
            Socket listener = (Socket)ar.AsyncState;
            try
            {
                Socket handler = listener.EndAccept(ar);

                //为异步接收创建状态对象。
                StateObject state = new StateObject();
                state.ServerSocket = handler;
                //模式选择0:代理模式，1:转发模式
                state.ConnModel = ConnModel;

                //连接到代理服务器并组装ClientSocket到对象类
                Socks5Handler sh = new Socks5Handler();
                sh.ClientConnect(state);

                AddList(state);

                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }
            catch { }

        }

        




    }
}
