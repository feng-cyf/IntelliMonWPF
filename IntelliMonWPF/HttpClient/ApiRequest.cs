using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.HttpClient
{
    public class ApiRequest<T> where T : class
    {
        public string Route { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        public Method Method { get; set; }

        public T Parsmeters { get; set; }

        public string ContentType { get; set; } = "application/json";
    }
}
