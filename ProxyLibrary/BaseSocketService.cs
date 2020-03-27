using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using static ProxyLibrary.StateObject;

namespace ProxyLibrary
{
    public abstract class BaseSocketService
    {
        private static List<StateObject> _list = new List<StateObject>();
        private static Logger _logger = new Logger("NLog.config");
        private string _address = "127.0.0.1";
        private Int32 _port = 5556;
        private CONNMODEL _connModel = CONNMODEL.PROXY;

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }
        public Int32 Port
        {
            get { return _port; }
            set { _port = value; }
        }
        public CONNMODEL ConnModel
        {
            get { return _connModel; }
            set { _connModel = value; }
        }
        public Logger LOGGER
        {
            get => _logger;
        }
        public List<StateObject> LIST
        {
            get => _list;
            set { _list = value; }
        }

        public BaseSocketService()
        {

        }


        /// <summary>
        /// 进行数据转发
        /// </summary>
        /// <param name="ar"></param>
        public void HandlerBeginReceive(IAsyncResult ar)
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
                WriteLogErr(string.Format("ServerReceiveCallback():{0}", ex.Message));
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
                WriteLogErr(string.Format("ClientSocketSendCallback():{0}", ex.Message));
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
                WriteLogErr(string.Format("ClientReceiveCallback():{0}", ex.Message));
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
                WriteLogErr(string.Format("ServerSocketSendCallback():{0}", ex.Message));
            }
        }

        public void Dispose(StateObject so)
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
            if (LIST != null)
                RemoveList(so);
        }

        public void AddList(StateObject so)
        {
            ///
            lock (LIST)
            {
                LIST.Add(so);
                //ShowList();
            }
        }
        public void RemoveList(StateObject so)
        {
            ///
            lock (LIST)
            {
                LIST.Remove(so);
                //ShowList();
            }
        }

        public void WriteLogErr(string msg, params object[] args)
        {
            LOGGER.Error(msg, args);

        }

        public void WriteLogErr(string msg, Exception err)
        {
            LOGGER.Error(msg, err);
        }
        public void WriteLogInfo(string msg, params object[] args)
        {
            LOGGER.Info(msg, args);

        }

        public void WriteLogInfo(string msg, Exception err)
        {
            LOGGER.Info(msg, err);
        }
        public void WriteLogDebug(string msg, params object[] args)
        {
            LOGGER.Debug(msg, args);

        }

        public void WriteLogDebug(string msg, Exception err)
        {
            LOGGER.Debug(msg, err);
        }
    }
}
