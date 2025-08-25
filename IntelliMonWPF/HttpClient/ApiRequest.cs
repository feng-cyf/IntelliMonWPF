using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.HttpClient
{
    internal class ApiRequest
    {
        public string Route { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        public Method Method { get; set; }

        public object Parsmeters { get; set; }

        public string ContentType { get; set; } = "application/json";
    }
}
