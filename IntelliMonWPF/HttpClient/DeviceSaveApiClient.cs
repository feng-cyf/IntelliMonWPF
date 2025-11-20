using IntelliMonWPF.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.HttpClient
{
    public class DeviceSaveApiClient
    {
        string BaseUri = "http://127.0.0.1:8000";
        public async Task<ApiResponse<JToken>> DeviceSave(ApiRequest<List<DeviceDTO>> api)
        {
            try
            {
                var request = new RestRequest(api.Route, api.Method);
                if (api.Parsmeters != null)
                {
                    var device = new
                    {
                        devices = api.Parsmeters,
                    };
                    var json= JsonConvert.SerializeObject(device);
                    request.AddStringBody(json,api.ContentType);
                }
                var option = new RestClientOptions(BaseUri);
                var client = new RestClient(option);
                var result = await client.ExecuteAsync(request);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<ApiResponse<JToken>>(result.Content);
                }
                else
                {
                    return new ApiResponse<JToken> { code = -99, message = "网络出现错误", data = null };
                }
            }
            catch (Exception ex) 
            {
                return new ApiResponse<JToken> { code= -99, message = "保存失败", data = null };
            }
        }
        public async Task<ApiResponse<JToken>> PointSave(ApiRequest<List<PointDTO>> api)
        {
            try
            {
                var request = new RestRequest(api.Route, api.Method);
                var points = new
                {
                    points = api.Parsmeters
                };
                var json = JsonConvert.SerializeObject(points);
                request.AddStringBody(json, api.ContentType);
                var option = new RestClientOptions(BaseUri);
                var client = new RestClient(option);
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<ApiResponse<JToken>>(response.Content);
                }
                else
                {
                    return new ApiResponse<JToken> { code = -99, message = "保存失败请排查", data = null };
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<ApiResponse<JToken>> EditPoint(ApiRequest<List<EditPointDTO>> api)
        {
            try
            {
                var request = new RestRequest(api.Route, api.Method);
                var EditPoinList = new
                {
                    EditPointList = api.Parsmeters
                };
                var json = JsonConvert.SerializeObject(EditPoinList);
                request.AddStringBody(json, api.ContentType);
                var option = new RestClientOptions(BaseUri) { Timeout=TimeSpan.FromSeconds(2)};
                var client = new RestClient(option);
                var result = await client.ExecuteAsync(request);
                if (result.StatusCode == System.Net.HttpStatusCode.OK) 
                {
                    return JsonConvert.DeserializeObject<ApiResponse<JToken>>(result.Content);
                }
                else
                {
                    return new ApiResponse<JToken> { code = -99, message = "更新失败请检查服务器", data = null };
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        internal async Task<ApiResponse<JToken>> GetTOCE(ApiRequest<TOCEDTO> api)
        {
            try
            {
                
                var request = new RestRequest(api.Route, api.Method);
                var Json= JsonConvert.SerializeObject(api.Parsmeters);
                request.AddStringBody(Json, api.ContentType);
                var option = new RestClientOptions(BaseUri) { Timeout = TimeSpan.FromSeconds(2) };
                var client = new RestClient(option);
                var result = await client.ExecuteAsync(request);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<ApiResponse<JToken>>(result.Content);
                }
                else
                {
                    return new ApiResponse<JToken> { code = -99, message = "获取失败请检查服务器", data = null };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}