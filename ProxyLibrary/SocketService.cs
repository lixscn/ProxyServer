using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Transmit
{
    public class SocketService
    {
        //public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static List<StateObject> list = new List<StateObject>();
        public static Logger logger = new Logger("NLog.config");

        public SocketService()
        {

        }

        public void OnStart()
        {
            IPAddress ipAssress = null;
            IPEndPoint lep = null;
            try
            {
                //IPHostEntry lipa = Dns.Resolve("www.163.com");
                //IPEndPoint lep = new IPEndPoint(lipa.AddressList[0], 5555);
                ipAssress = IPAddress.Parse("127.0.0.1");
                lep = new IPEndPoint(ipAssress, 5556);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Error(ex.Message);
            }

            Socket listener = new Socket(lep.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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
                logger.Error(ex.Message);
            }



        }

        private void AcceptCallback(IAsyncResult ar)
        {
            //获取处理客户端请求的套接字。
            Socket listener = (Socket)ar.AsyncState;

            Socket handler = listener.EndAccept(ar);

            //为异步接收创建状态对象。
            StateObject state = new StateObject();
            state.ServerSocket = handler;
            //模式选择0:代理模式，1:转发模式
            state.ConnModel = StateObject.CONNMODEL.PROXY;
            //连接到代理服务器并组装ClientSocket到对象类
            ClientConnect(state);

            AddList(state);

            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

        }

        private void ClientConnect(StateObject state)
        {
            //StateObject stateObj = null;
            try
            {
                switch (state.ConnModel)
                    {
                    case StateObject.CONNMODEL.TRANSMIT:
                        string host = "185.116.93.67";
                        Int32 port = 1081;
                        ClientConnect(host, port, state);
                        break;
                    case StateObject.CONNMODEL.PROXY:
                        GetServerReceive(state);
                        break;

                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(string.Format("ClientConnect(StateObject state):{0}", ex.Message));
                logger.Error(string.Format("ClientConnect(StateObject state):{0}", ex.Message));
            }
            //return stateObj;
        }

        /// <summary>
        /// 连接远程服务器
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private void ClientConnect(string host, Int32 port, StateObject state)
        {
            //StateObject stateObj = null;
            try
            {
                Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAssress = IPAddress.Parse(host);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAssress, port);
                state.ClientSocket = ClientSocket;
                ClientSocket.BeginConnect(ipEndPoint,new AsyncCallback(HandlerBeginReceive), state);
                
                //stateObj = state;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(string.Format("ClientConnect():{0}", ex.Message));
                logger.Error(string.Format("ClientConnect():{0}", ex.Message));
            }
            //return stateObj;
        }

        /// <summary>
        /// 在代理模式下，接收浏览器信息，获得要连接的远程服务器地址，并连接远程服务器
        /// </summary>
        /// <returns></returns>
        private void GetServerReceive(StateObject state)
        {
            Socket ServerSocket = state.ServerSocket;
            try
            {
                //先接收浏览器发送过来的信息，解析后得到远程连接地址。
                ServerSocket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ToClientConnectCallback), state);

            }
            catch (Exception ex)
            {
                //Console.WriteLine(string.Format("GetServerReceiveToClientConnect:{0}", ex.Message));
                logger.Error(string.Format("GetServerReceiveToClientConnect():{0}", ex.Message));
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
                    if (bytes[0] == 0x05)  //处理SOCKET5
                    {
                        //Console.WriteLine("进入0x05");
                        byte[] reBytes = { 0x05, 0x00 };
                        ServerSocket.BeginSend(reBytes,0,reBytes.Length, SocketFlags.None, new AsyncCallback(ServerSocketBeginSendClientConnectCallback), so);  //客户端回应：Socket服务端不需要验证方式

                    }
                }
            }
            catch (Exception ex)
            {
                Dispose(so);
                //Console.WriteLine(string.Format(@"ToClientConnectCallback() --> ERR:{0}", ex.Message));
                logger.Error(string.Format("ToClientConnectCallback():{0}", ex.Message));
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
                //Console.WriteLine(string.Format("GetServerReceiveToClientConnect:{0}", ex.Message));
                logger.Error(string.Format("GetServerReceiveToClientConnect():{0}", ex.Message));
            }
            //return stateObj;
        }

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
                    if (bytes[1] == 1) {//完成了CONNECT
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
                        if (RemoteIP != null) {
                            so.ClientSocket = new Socket(RemoteIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                            so.ClientSocket.BeginConnect(new IPEndPoint(RemoteIP, RemotePort), new AsyncCallback(this.ReBeginConnectCallback), so);

                        }
                    }//后续再加其他

                }
            }
            catch (Exception ex)
            {
                Dispose(so);
                //Console.WriteLine(string.Format(@"ServerSocketBeginSendClientConnectCallback() --> ERR:{0}", ex.Message));
                logger.Error(string.Format("ServerSocketBeginSendClientConnectCallback():{0}", ex.Message));
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

                ServerSocket.BeginSend(bytes,0, bytes.Length, SocketFlags.None, new AsyncCallback(HandlerBeginReceive),so);

            }
            catch (Exception ex)
            {
                Dispose(so);
                //Console.WriteLine(string.Format(@"ReBeginConnectCallback() --> ERR:{0}", ex.Message));
                logger.Error(string.Format("ReBeginConnectCallback():{0}", ex.Message));
            }
        }

        /// <summary>
        /// 进行数据转发
        /// </summary>
        /// <param name="ar"></param>
        private void HandlerBeginReceive(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket ServerSocket = state.ServerSocket;
            Socket ClientSocket = state.ClientSocket;

            ServerSocket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ServerReceiveCallback), state);
            ClientSocket.BeginReceive(state.downBuffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ClientReceiveCallback), state);
        }


        /// <summary>
        /// 发送到代理服务器
        /// </summary>
        /// <param name="ar"></param>
        private void ServerReceiveCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket ServerSocket = so.ServerSocket;
            Socket ClientSocket = so.ClientSocket;
            try
            {
                int read = ServerSocket.EndReceive(ar);

                if (read > 0)
                {
                    //把接收到的信息发送到代理服务器
                    ClientSocket.BeginSend(so.buffer, 0, read, SocketFlags.None, new AsyncCallback(ClientSocketSendCallback), so);
                }
            }
            catch (Exception ex)
            {
                Dispose(so);
                //Console.WriteLine(string.Format(@"ServerReceiveCallback() --> ERR:{0}", ex.Message));
                logger.Error(string.Format("ServerReceiveCallback():{0}", ex.Message));
            }


        }
        /// <summary>
        /// 接收浏览器信息
        /// </summary>
        /// <param name="ar"></param>
        private void ClientSocketSendCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket ServerSocket = so.ServerSocket;
            Socket ClientSocket = so.ClientSocket;

            //Console.WriteLine("ServerSocket.Handle:{0}", ServerSocket.Handle);
            //Console.WriteLine("ClientSocket.Handle:{0}", ClientSocket.Handle);
            try
            {
                int read = ClientSocket.EndSend(ar);
                if (read <= 0)
                {

                }
                //Console.WriteLine(String.Format(@"ReceiveCallback() --> BeginReceive：\n{0}", Encoding.ASCII.GetString(so.buffer, 0, read)));
                ServerSocket.BeginReceive(so.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ServerReceiveCallback), so);
            }
            catch (Exception ex)
            {
                Dispose(so);
                //Console.WriteLine(string.Format(@"ClientSocketSendCallback() --> ERR:{0}", ex.Message));
                logger.Error(string.Format("ClientSocketSendCallback():{0}", ex.Message));
            }
        }
        /// <summary>
        /// 发送到浏览器
        /// </summary>
        /// <param name="ar"></param>
        private void ClientReceiveCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket ServerSocket = so.ServerSocket;
            Socket ClientSocket = so.ClientSocket;

            //Console.WriteLine("ServerSocket.Handle:{0}", ServerSocket.Handle);
            //Console.WriteLine("ClientSocket.Handle:{0}", ClientSocket.Handle);
            try
            {
                int read = ClientSocket.EndReceive(ar);

                if (read > 0)
                {
                    //向浏览器发送数据
                    //Console.WriteLine(string.Format(@"代理服务器发回数据:{0}", Encoding.ASCII.GetString(so.downBuffer, 0, read)));
                    //Console.WriteLine(string.Format(@"排序:{0}",  StateObject.i));
                    ServerSocket.BeginSend(so.downBuffer, 0, read, SocketFlags.None, new AsyncCallback(ServerSocketSendCallback), so);
                }

            }
            catch (Exception ex)
            {
                Dispose(so);
                //Console.WriteLine(string.Format(@"ClientReceiveCallback():{0}", ex.Message));
                logger.Error(string.Format("ClientReceiveCallback():{0}", ex.Message));
            }
        }
        /// <summary>
        /// 接收代理服务器信息
        /// </summary>
        /// <param name="ar"></param>
        private void ServerSocketSendCallback(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket ServerSocket = so.ServerSocket;
            Socket ClientSocket = so.ClientSocket;
            try
            {
                int read = ServerSocket.EndSend(ar);
                if (read > 0)
                {
                    //Console.WriteLine(string.Format("成功发送{0}字节", read));
                    so.Read = so.Read + read;
                    StateObject.TotelRead = StateObject.TotelRead + read;
                }
                //接收代理服务器发回数据
                ClientSocket.BeginReceive(so.downBuffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ClientReceiveCallback), so);

            }
            catch (Exception ex)
            {
                Dispose(so);
                //Console.WriteLine(string.Format(@"ServerSocketSendCallback():{0}", ex.Message));
                logger.Error(string.Format("ServerSocketSendCallback():{0}", ex.Message));
            }
        }

        private void Dispose(StateObject so)
        {
            Socket ServerSocket = so.ServerSocket;
            Socket ClientSocket = so.ClientSocket;

            try
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            try
            {
                ServerSocket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            //Close the sockets
            if (ClientSocket != null)
                ClientSocket.Close();
            if (ServerSocket != null)
                ServerSocket.Close();
            //Clean up
            ClientSocket = null;
            ServerSocket = null;
            if (list != null)
                RemoveList(so);
        }

        private void AddList(StateObject so)
        {
            ///
            lock (list)
            {
                list.Add(so);
                //ShowList();
            }
        }
        private void RemoveList(StateObject so)
        {
            ///
            lock (list)
            {
                list.Remove(so);
                //ShowList();
            }
        }
        
    }
}
