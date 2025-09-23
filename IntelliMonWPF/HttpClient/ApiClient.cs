using IntelliMonWPF.DTOs;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.HttpClient
{
    internal class ApiClient
    {
        private string _baseUrl = "http://127.0.0.1:8000";
        public ApiResponse<TResult> Excute<TResult, TRequest>(ApiRequest<TRequest> apiRequest) where TRequest : class
        {
            try
            {
                var client = new RestRequest(apiRequest.Route, apiRequest.Method);
                client.AddHeader("Content-Type", apiRequest.ContentType);
                if (apiRequest.Parsmeters != null)
                {
                    var parameters = apiRequest.Parsmeters;
                    client.AddJsonBody(parameters);
                }
                var options = new RestClientOptions(_baseUrl)
                {
                    ThrowOnAnyError = false,
                    Timeout = TimeSpan.FromSeconds(10)
                };
                var _client = new RestClient(options);
                var result = _client.Execute(client);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = JsonConvert.DeserializeObject<ApiResponse<TResult>>(result.Content);
                    return response;
                }
                else
                {
                    return new ApiResponse<TResult> { code = -1, message = "出现异常" };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResult> { code = -1, message = ex.Message };
            }
        }

        public ApiResponse<List<JobSlectInfo>> SelectJob(ApiRequest<object> apiRequest)
        {
            var request = new RestRequest(apiRequest.Route, apiRequest.Method);
            var options = new RestClientOptions(_baseUrl)
            {
                ThrowOnAnyError = false,
                Timeout = TimeSpan.FromSeconds(10)
            };
            var _client = new RestClient(options);
            var result = _client.Execute(request);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ApiResponse<List<JobSlectInfo>>>(result.Content);
            }
            else
            {
                return new ApiResponse<List<JobSlectInfo>> { message = "获取失败", code = -1, data = null };
            }
        }
        public async Task<ApiResponse<DataSendDTO>> SendData(ApiRequest<DataSendDTO> data)
        {
            try
            {
                var request = new RestRequest(data.Route, data.Method);
                if (data.Parsmeters != null)
                {
                    var json = JsonConvert.SerializeObject(data.Parsmeters);
                    request.AddStringBody(json, data.ContentType);
                }

                var options = new RestClientOptions(_baseUrl)
                {
                    ThrowOnAnyError = false,
                    Timeout = TimeSpan.FromSeconds(0.5)
                };
                var _client = new RestClient(options);

                var result = await _client.ExecuteAsync(request);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<ApiResponse<DataSendDTO>>(result.Content)!;
                }
                else
                {
                    return new ApiResponse<DataSendDTO>
                    {
                        code = -99,
                        message = $"API未响应或状态异常: {result.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException)
            {
                return new ApiResponse<DataSendDTO>
                {
                    code = -99,
                    message = "API未启动，发送已跳过"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DataSendDTO>
                {
                    code = -99,
                    message = $"发送异常: {ex.Message}"
                };
            }
        }
    }
}
