using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Morphologue.IdentityWsClient.Testability;
using Newtonsoft.Json;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient.Tests
{
    // An IJsonClient which replays server interactions from RecordingHttpJsonClient.
    internal class ReplayingJsonClient : IJsonClient
    {
        private int _playbackIndex;
        private List<RequestAndResponse> _recording;

        internal ReplayingJsonClient(string fileName)
        {
            _recording = JsonConvert.DeserializeObject<List<RequestAndResponse>>(File.ReadAllText(fileName));
        }

        public Task<JsonResponse> GetAsync(string url) => MethodAsync(url, "GET");

        public Task<JsonResponse> PostAsync(string url, JsonDict body = null) => MethodAsync(url, "POST", body);

        public Task<JsonResponse> PatchAsync(string url, JsonDict body) => MethodAsync(url, "PATCH", body);

        public Task<JsonResponse> DeleteAsync(string url) => MethodAsync(url, "DELETE");

        protected Task<JsonResponse> MethodAsync(string url, string method, JsonDict body = null)
        {
            RequestAndResponse rar = _recording[_playbackIndex];

            RecordedRequest request = new RecordedRequest
            {
                Url = url,
                Method = method,
                Body = body
            };
            if (!request.Matches(rar.Request))
                throw new Exception($"Actual request doesn't match recorded at index {_playbackIndex}");

            _playbackIndex++;
            return Task.FromResult(rar.Response);
        }
    }
}
