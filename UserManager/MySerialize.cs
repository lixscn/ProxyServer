using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace UserManager
{
    public class MySerialize
    {
        public static string key = "hjyf57468jhmuist";
        public static string iv = "d547io0d98eddl2d";
        public static string filePath = string.Format(@"{0}/data", Environment.CurrentDirectory);
        /// <summary>
        /// 把对象序列化为字节数组
        /// </summary>
        public static byte[] SerializeObjectToBytes(object obj)
        {
            if (obj == null)
                return null;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            byte[] bytes = ms.ToArray();
            return bytes;
        }

        /// <summary>
        /// 把字节数组反序列化成对象
        /// </summary>
        public static object DeserializeObjectFromBytes(byte[] bytes)
        {
            object obj = null;
            if (bytes == null)
                return obj;
            MemoryStream ms = new MemoryStream(bytes)
            {
                Position = 0
            };
            BinaryFormatter formatter = new BinaryFormatter();
            obj = formatter.Deserialize(ms);
            ms.Close();
            return obj;
        }

        /// <summary>
        /// 序列化对象到文件：
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="obj"></param>
        public static void SerializeObjectToFile(string fileName, object obj)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, obj);
            }
        }

        /// <summary>
        /// 把文件反序列化成对象
        /// </summary>
        public static object DeserializeObjectFromFile(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                object obj = formatter.Deserialize(fs);
                return obj;
            }
        }

        /// <summary>
        /// 把对象序列化到文件(AES加密)
        /// </summary>
        /// <param name="keyString">密钥(16位)</param>
        public static void SerializeObjectToFile(string fileName, object obj, string keyString,string ivString)
        {
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            using (AesCryptoServiceProvider crypt = new AesCryptoServiceProvider())
            {
                crypt.Key = Encoding.ASCII.GetBytes(keyString);
                crypt.IV = Encoding.ASCII.GetBytes(ivString);
                using (ICryptoTransform transform = crypt.CreateEncryptor())
                {
                    FileStream fs = new FileStream(fileName, FileMode.Create);
                    using (CryptoStream cs = new CryptoStream(fs, transform, CryptoStreamMode.Write))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(cs, obj);
                    }
                }
            }
        }

        /// <summary>
        /// 把文件反序列化成对象(AES加密)
        /// </summary>
        /// <param name="keyString">密钥(16位)</param>
        public static object DeserializeObjectFromFile(string fileName, string keyString,string ivString)
        {
            using (AesCryptoServiceProvider crypt = new AesCryptoServiceProvider())
            {
                crypt.Key = Encoding.ASCII.GetBytes(keyString);
                crypt.IV = Encoding.ASCII.GetBytes(ivString);
                using (ICryptoTransform transform = crypt.CreateDecryptor())
                {
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using (CryptoStream cs = new CryptoStream(fs, transform, CryptoStreamMode.Read))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        object obj = formatter.Deserialize(cs);
                        return obj;
                    }
                }
            }
        }
    }
}
