using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Morphologue.IdentityWsClient.Extensions;
using Newtonsoft.Json;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient.Testability
{
    internal class HttpJsonClient : IJsonClient
    {
        private HttpClient _http;

        internal HttpJsonClient(HttpClient http)
        {
            _http = http;
        }

        public Task<JsonResponse> GetAsync(string url) => MethodAsync(url, "GET");

        public Task<JsonResponse> PostAsync(string url, JsonDict body = null) => MethodAsync(url, "POST", body);

        public Task<JsonResponse> PatchAsync(string url, JsonDict body) => MethodAsync(url, "PATCH", body);

        public Task<JsonResponse> DeleteAsync(string url) => MethodAsync(url, "DELETE");

        protected virtual async Task<JsonResponse> MethodAsync(string url, string method, JsonDict body = null)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url)
            {
                Content = body == null ? new StringContent("") : body.ToHttpContent()
            })
            using (HttpResponseMessage response = await _http.SendAsync(request))
            {
                string serial = await response.Content.ReadAsStringAsync();
                return new JsonResponse
                {
                    StatusCode = response.StatusCode,
                    Content = string.IsNullOrEmpty(serial) ? null : JsonConvert.DeserializeObject<JsonDict>(serial)
                };
            }
        }
    }
}
