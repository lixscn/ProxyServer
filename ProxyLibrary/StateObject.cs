using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ProxyLibrary
{
    public class StateObject
    {
        public Socket ServerSocket = null;
        public Socket ClientSocket = null;
        public const int BUFFER_SIZE = 4 * 1024;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public byte[] downBuffer = new byte[BUFFER_SIZE];
        public StringBuilder sb = new StringBuilder();

        public CONNMODEL ConnModel = CONNMODEL.PROXY;
        public string remoteHost = "";
        public Int32 remotePort = 1080;


        public long Read = 0;

        public static long TotelRead = 0;

        public enum CONNMODEL : int
        {
            PROXY,
            TRANSMIT
        }
    }


}
