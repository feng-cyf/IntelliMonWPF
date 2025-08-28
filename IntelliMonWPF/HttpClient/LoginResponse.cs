using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.HttpClient
{
    public class JobSlectInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string JobName { get; set; }  // 对应job_name
        [JsonProperty("role")]
        public string JobRole { get; set; }  // 对应job_role
    }
    public class JobInfo
    {
        [JsonProperty("job_id")]
        public int JobId { get; set; }      // 对应job_id
        [JsonProperty("job_name")]
        public string JobName { get; set; }  // 对应job_name
        [JsonProperty("job_role")]
        public string JobRole { get; set; }  // 对应job_role
       
    }

    // 2. 定义用户信息类
    public class UserInfo
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }      // 对应user_id
        [JsonProperty("username")]
        public string Username { get; set; } // 对应username
        public JobInfo Job { get; set; }     // 嵌套职位信息
        [JsonProperty("create_time")]
        public DateTime CreateTime { get; set; }
    }
    public class LoginResponse<T>
    {
        public int code { get; set; }
        public string message { get; set; }
        public T data { get; set; }
    }
}
