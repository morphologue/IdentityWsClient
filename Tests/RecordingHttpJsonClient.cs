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
    // An HttpJsonClient which records real server interactions in order.
    internal class RecordingHttpJsonClient : HttpJsonClient
    {
        private string _fileName;
        private List<RequestAndResponse> _recording;

        internal RecordingHttpJsonClient(HttpClient http, string fileName) : base(http)
        {
            _fileName = fileName;
            _recording = new List<RequestAndResponse>();
        }

        internal void Persist()
        {
            File.WriteAllText(_fileName, JsonConvert.SerializeObject(_recording));
        }

        protected override async Task<JsonResponse> MethodAsync(string url, string method, JsonDict body = null)
        {
            JsonResponse response = await base.MethodAsync(url, method, body);
            _recording.Add(new RequestAndResponse
            {
                Request = new RecordedRequest
                {
                    Url = url,
                    Method = method,
                    Body = body
                },
                Response = response
            });
            return response;
        }
    }
}
