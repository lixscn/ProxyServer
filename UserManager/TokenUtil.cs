using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace UserManager
{
    public class TokenUtil
    {
        public static string SecretKey = "bqsid123k12s0h1d3uhf493fh02hdd102h9s3h38ff";//这个服务端加密秘钥 属于私钥

        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static string GenToken(UserObject M)
        {
            Dictionary<string, dynamic> payload = new Dictionary<string, dynamic>
            {
                {"UserName", M.UserName},//用于存放当前登录人账户信息
                {"UserPwd", M.Password}//用于存放当前登录人登录密码信息
            };
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
            return encoder.Encode(payload, SecretKey);
        }

        /// <summary>
        /// 验证Token
        /// </summary>
        /// <returns></returns>
        public static string DecodeToken(string token)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm jwtAlgorithm = new HMACSHA256Algorithm();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, jwtAlgorithm);
                var json = decoder.Decode(token, SecretKey, verify: true);
                return json;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
