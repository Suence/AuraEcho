using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PowerLab.Core.Tools
{
    public class HttpHelper
    {
        private readonly HttpClient _client;
        private string? _token;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public HttpHelper(HttpClient? client = null)
        {
            _client = client ?? new HttpClient();
        }

        /// <summary>
        /// 设置 Bearer Token
        /// </summary>
        public void SetToken(string token)
        {
            _token = token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// 清除 Token
        /// </summary>
        public void ClearToken()
        {
            _token = null;
            _client.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// 统一 GET 请求
        /// </summary>
        public async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return default;
            }
        }

        /// <summary>
        /// 统一 POST JSON 请求
        /// </summary>
        public async Task<T?> PostAsync<T>(string url, object data)
        {
            try
            {
                var response = await _client.PostAsJsonAsync(url, data, _jsonOptions);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return default;
            }
        }

        /// <summary>
        /// POST 表单请求
        /// </summary>
        public async Task<string?> PostFormAsync(string url, Dictionary<string, string> formData)
        {
            try
            {
                var content = new FormUrlEncodedContent(formData);
                var response = await _client.PostAsync(url, content);
                return await HandleResponse(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return null;
            }
        }

        // -------------------- 内部统一处理逻辑 --------------------

        private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                // HTTP 200-299
                return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            }
            else
            {
                // 统一处理错误码
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ HTTP {response.StatusCode}: {error}");
                return default;
            }
        }

        private async Task<string?> HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ HTTP {response.StatusCode}: {error}");
                return null;
            }
        }

        private void HandleException(Exception ex)
        {
            if (ex is TaskCanceledException)
            {
                Console.WriteLine("⚠️ 请求超时");
            }
            else if (ex is HttpRequestException)
            {
                Console.WriteLine("⚠️ 网络异常，请检查连接");
            }
            else
            {
                Console.WriteLine($"⚠️ 未知异常：{ex.Message}");
            }
        }
    }
}
