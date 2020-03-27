using HttpServices;
using ProxyLibrary;
using System;
using System.Collections.Generic;
using UserManager;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            //SocketService sks = new SocketService();
            //sks.OnStart();

            //UsersManager usersManager = UsersManager.Instance();
            //usersManager.SetUser("admin","admin2");
            //List<UserObject> userList = usersManager.UserList;
            //foreach (UserObject uo in userList)
            //{
            //    Console.WriteLine("User : {0}  PassWord : {1}  UID : {2}",uo.UserName,uo.Password,uo.Uid);
            //}

            //UserObject uo = new UserObject();
            //uo.UserName = "admin";
            //uo.Password = "123456";
            //string token = TokenUtil.GenToken(uo);

            //string decodeToken = TokenUtil.DecodeToken(token);

            //Console.WriteLine("token : {0}    decodeToken : {1}", token, decodeToken);

            ServerHelper sh = new ServerHelper();
            sh.Setup();

            Console.ReadLine();
        }
    }
}
