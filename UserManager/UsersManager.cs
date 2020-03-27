using System;
using System.Collections.Generic;
using System.Text;

namespace UserManager
{
    public sealed class UsersManager
    {
        // 定义一个静态变量来保存类的实例
        private static UsersManager instance = null;

        // 定义一个标识确保线程同步
        private static readonly object padlock = new object();

        private List<UserObject> _userList = new List<UserObject>();
        private List<UserObject> _loginList = new List<UserObject>();
        private readonly string _filePath = MySerialize.filePath + "/userList.dat";
        private readonly string _key = MySerialize.key;
        private readonly string _iv = MySerialize.iv;

        public List<UserObject> UserList { get => _userList; set => _userList = value; }
        public string FilePath { get => _filePath; }
        public string Key { get => _key; }
        public string Iv { get => _iv; }
        public List<UserObject> LoginList { get => _loginList; set => _loginList = value; }

        private UsersManager()
        {
            //加载序列化文件
            try { 
                UserList = (List<UserObject>)MySerialize.DeserializeObjectFromFile(FilePath, Key, Iv);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(string.Format("加载序列化文件错误:{0}",ex.Message));
            }
        }

        
        public UserObject Login(string userName,string passWord)
        {
            UserObject User = selectUser(userName);
            

            if (User != null)
            {
                if (User.UserName == userName && User.Password == passWord)
                {
                    LoginList.Add(User);  //添加到已登陆列表
                    return User;
                }
            }
            return null;

        }

        

        public bool SetUser(string userName, string passWord)
        {
            string uuid = System.Guid.NewGuid().ToString("N");
            UserObject User = selectUser(userName);
            bool bo = false;
            if (User == null)
            {
                UserObject userObject = new UserObject();
                userObject.UserName = userName;
                userObject.Password = passWord;
                userObject.Uid = uuid;
                UserList.Add(userObject);

                MySerialize.SerializeObjectToFile(FilePath,UserList, Key, Iv);

                bo = true;
            }
            return bo;
        }

        public UserObject selectUser(string userName)
        {
            UserObject reObj = null;
            foreach (UserObject userObj in UserList)
            {
                if (userObj.UserName.Equals(userName))
                {
                    reObj = userObj;
                    break;
                }
            }
            return reObj;
        }


        public static UsersManager Instance()
        {
            // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
            // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
            // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
            if (instance == null)
            {
                lock (padlock)
                {
                    // 如果类的实例不存在则创建，否则直接返回
                    if (instance == null)
                    {
                        instance = new UsersManager();
                    }
                }
            }
            return instance;
        }
        
        

    }
}
