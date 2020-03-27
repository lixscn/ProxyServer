using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServices
{
    public class ResponseReturnObj
    {
        public int StatusCode;
        public string ContentType;
        public Dictionary<string,string> Headers;
        public Encoding ContentEncoding;

    }
}
