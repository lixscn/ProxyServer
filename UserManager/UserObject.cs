using System;
using System.Collections.Generic;
using System.Text;

namespace UserManager
{
    [Serializable()]
    public class UserObject
    {
        private string uid;
        private string userName;
        private string password;
        private string remoteHostIP;
        private string token;

        public string Uid { get => uid; set => uid = value; }
        public string UserName { get => userName; set => userName = value; }
        public string Password { get => password; set => password = value; }
        public string RemoteHostIP { get => remoteHostIP; set => remoteHostIP = value; }
        public string Token { get => token; set => token = value; }
    }
}
