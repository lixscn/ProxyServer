using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static ProxyLibrary.StateObject;

namespace ProxyLibrary
{
    public class Socks5Handler
    {
        private static Logger logger = new Logger("NLog.config");
        private SocketService sosv = new SocketService();

        public void ClientConnect(StateObject so)
        {
            //StateObject stateObj = null;
            try
            {
                switch (so.ConnModel)
                {
                    case CONNMODEL.TRANSMIT:
                        string host = so.remoteHost;
                        Int32 port = so.remotePort;
                        ClientConnect(host, port, so);
                        break;
                    case CONNMODEL.PROXY:
                        GetServerReceive(so);
                        break;

                }
            }
            catch (Exception ex)
            {
                sosv.Dispose(so);
                //Console.WriteLine(string.Format("ClientConnect(StateObject state):{0}", ex.Message));
                sosv.WriteLogErr(string.Format("ClientConnect(StateObject state):{0}", ex.Message));
            }
            //return stateObj;
        }

        /// <summary>
        /// 连接远程服务器
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private void ClientConnect(string host, Int32 port, StateObject so)
        {
            //StateObject stateObj = null;
            try
            {
                Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAssress = IPAddress.Parse(host);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAssress, port);
                so.ClientSocket = ClientSocket;
                ClientSocket.BeginConnect(ipEndPoint, new AsyncCallback(sosv.HandlerBeginReceive), so);

                //stateObj = state;
            }
            catch (Exception ex)
            {
                sosv.Dispose(so);
                //Console.WriteLine(string.Format("ClientConnect():{0}", ex.Message));
                sosv.WriteLogErr(string.Format("ClientConnect():{0},Host = {1},Port = {2}", ex.Message,host,port));
            }
            //return stateObj;
        }

        /// <summary>
        /// 在代理模式下，接收浏览器信息，获得要连接的远程服务器地址，并连接远程服务器
        /// </summary>
        /// <returns></returns>
        private void GetServerReceive(StateObject so)
        {
            Socket ServerSocket = so.ServerSocket;
            try
            {
                //先接收浏览器发送过来的信息，解析后得到远程连接地址。
                ServerSocket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ToClientConnectCallback), so);

            }
            catch (Exception ex)
            {
                sosv.Dispose(so);
                //Console.WriteLine(string.Format("GetServerReceiveToClientConnect:{0}", ex.Message));
                sosv.WriteLogErr(string.Format("GetServerReceiveToClientConnect():{0}", ex.Message));
            }
            //return stateObj;
        }

        private void ToClientConnectCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket ServerSocket = so.ServerSocket;
            try
            {
                int read = ServerSocket.EndReceive(ar);
                //Console.WriteLine("ServerSocket.EndReceive {0}", read);

                if (read > 0)
                {
                    //Console.WriteLine(so.buffer[0]);
                    byte[] bytes = so.buffer;
                    //要先处理VMess加密封包数据

                    if (bytes[0] == 0x05)  //处理SOCKET5
                    {
                        //Console.WriteLine("进入0x05");
                        byte[] reBytes = { 0x05, 0x00 };
                        ServerSocket.BeginSend(reBytes, 0, reBytes.Length, SocketFlags.None, new AsyncCallback(ServerSocketBeginSendClientConnectCallback), so);  //客户端回应：Socket服务端不需要验证方式

                    }
                }
            }
            catch (Exception ex)
            {
                sosv.Dispose(so);
                //Console.WriteLine(string.Format(@"ToClientConnectCallback() --> ERR:{0}", ex.Message));
                sosv.WriteLogErr(string.Format("ToClientConnectCallback():{0}", ex.Message));
            }
        }

        private void ServerSocketBeginSendClientConnectCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket ServerSocket = so.ServerSocket;
            try
            {
                int read = ServerSocket.EndSend(ar);
                if (read > 0)
                {
                    //先接收浏览器发送过来的信息，解析后得到远程连接地址。
                    ServerSocket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ServerSocketBeginReceiveClientConnectCallback), so);
                }

            }
            catch (Exception ex)
            {
                sosv.Dispose(so);
                //Console.WriteLine(string.Format("GetServerReceiveToClientConnect:{0}", ex.Message));
                sosv.WriteLogErr(string.Format("GetServerReceiveToClientConnect():{0}", ex.Message));
            }
            //return stateObj;
        }
        /// <summary>
        /// 解释接收到的信息，得到远程地址后连接
        /// </summary>
        /// <param name="ar"></param>
        private void ServerSocketBeginReceiveClientConnectCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket ServerSocket = so.ServerSocket;

            try
            {
                int read = ServerSocket.EndReceive(ar);
                if (read > 0)
                {
                    byte[] bytes = so.buffer;
                    if (bytes[1] == 1)
                    {//完成了CONNECT
                        IPAddress RemoteIP = null;
                        int RemotePort = 0;
                        if (bytes[3] == 0x01)
                        {
                            RemoteIP = IPAddress.Parse(bytes[4].ToString() + "." + bytes[5].ToString() + "." + bytes[6].ToString() + "." + bytes[7].ToString());
                            RemotePort = bytes[8] * 256 + bytes[9];
                        }
                        else if (bytes[3] == 0x03)
                        {
                            RemoteIP = Dns.Resolve(Encoding.ASCII.GetString(bytes, 5, bytes[4])).AddressList[0];
                            RemotePort = bytes[4] + 5;
                            RemotePort = bytes[RemotePort] * 256 + bytes[RemotePort + 1];
                        }
                        if (RemoteIP != null)
                        {
                            so.ClientSocket = new Socket(RemoteIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                            so.ClientSocket.BeginConnect(new IPEndPoint(RemoteIP, RemotePort), new AsyncCallback(this.ReBeginConnectCallback), so);

                        }
                    }//后续再加其他

                }
            }
            catch (Exception ex)
            {
                sosv.Dispose(so);
                //Console.WriteLine(string.Format(@"ServerSocketBeginSendClientConnectCallback() --> ERR:{0}", ex.Message));
                sosv.WriteLogErr(string.Format("ServerSocketBeginSendClientConnectCallback():{0}", ex.Message));
            }
        }

        private void ReBeginConnectCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket ServerSocket = so.ServerSocket;
            Socket ClientSocket = so.ClientSocket;
            try
            {
                ClientSocket.EndConnect(ar);

                //返回连接响应
                byte[] bytes = { 0x05, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                ServerSocket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(sosv.HandlerBeginReceive), so);

            }
            catch (Exception ex)
            {
                sosv.Dispose(so);
                //Console.WriteLine(string.Format(@"ReBeginConnectCallback() --> ERR:{0}", ex.Message));
                sosv.WriteLogErr(string.Format("ReBeginConnectCallback():{0}", ex.Message));
            }
        }

    }
}
