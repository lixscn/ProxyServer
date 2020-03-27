using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace UserManager
{
    public class EncryptUtil
    {
        public const string DESKey = "6ce3d3a9de10c126498d897b2107d38f";//秘钥
        public const string DESIV = "ebe92df98b6c1444dc1aab77f41c1194";//向量

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="ToEncrypt">要加密的文本</param>
        /// <param name="DESKey"></param>
        /// <param name="DESIV"></param>
        /// <returns></returns>
        public string EncryptDES(string ToEncrypt, string DESKey, string DESIV)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            { //创建des实例
                byte[] inputByteArray = Encoding.UTF8.GetBytes(ToEncrypt);//将需要加密的内容转为字节
                des.Key = ASCIIEncoding.ASCII.GetBytes(DESKey);//秘钥
                des.IV = ASCIIEncoding.ASCII.GetBytes(DESIV);//向量
                System.IO.MemoryStream ms = new System.IO.MemoryStream();//创建流实例
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {//把输出的内容通过第二个参数转换(加密)馈送到第一个参数ms
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                //转为Base64后输出
                string str = Convert.ToBase64String(ms.ToArray());
                ms.Close();
                return str;
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Paras"></param>
        /// <param name="DESKey"></param>
        /// <param name="DESIV"></param>
        /// <returns></returns>
        public string[] DecodeDES(string Paras, string DESKey, string DESIV)
        {
            string Str = "";
            string[] Ret = new string[2] { "", "" };
            try
            {
                byte[] inputByteArray = Convert.FromBase64String(Paras);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    des.Key = ASCIIEncoding.ASCII.GetBytes(DESKey);
                    des.IV = ASCIIEncoding.ASCII.GetBytes(DESIV);
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    Str = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    Ret[0] = "1";
                    Ret[1] = Str;
                }
            }
            catch (Exception ex)
            {
                Ret[0] = "-1";
                Ret[1] = ex.ToString();
            }
            finally
            {
            }
            return Ret;
        }
    }
}
