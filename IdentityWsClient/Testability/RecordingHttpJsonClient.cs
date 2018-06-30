using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Morphologue.IdentityWsClient.Extensions;
using Newtonsoft.Json;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient.Testability
{
    // An HttpJsonClient which either records or plays back server interactions in order.
    internal class RecordingHttpJsonClient : HttpJsonClient
    {
        private class RecordedRequest
        {
            public string Url { get; set; }
            public string Method { get; set; }
            public JsonDict Body { get; set; }

            internal bool Matches(RecordedRequest other)
            {
                if (Url != other.Url || Method != other.Method)
                    return false;
                if (Body == null)
                {
                    if (other.Body != null)
                        return false;
                }
                else
                {
                    if (!Body.ShallowEquals(other.Body))
                        return false;
                }
                return true;
            }
        }

        private class RequestAndResponse
        {
            public RecordedRequest Request { get; set; }
            public JsonResponse Response { get; set; }
        }

        private string _fileName;
        private bool _playback;
        private int _playbackIndex;
        private List<RequestAndResponse> _recording;

        internal RecordingHttpJsonClient(HttpClient http, string fileName, bool playback) : base(http)
        {
            _fileName = fileName;
            _playback = playback;
            _recording = playback
                ? JsonConvert.DeserializeObject<List<RequestAndResponse>>(File.ReadAllText(_fileName))
                : new List<RequestAndResponse>();
        }

        internal void Persist()
        {
            if (_playback)
            {
                throw new InvalidOperationException("Can't persist when in playback mode");
            }
            File.WriteAllText(_fileName, JsonConvert.SerializeObject(_recording));
        }

        protected override async Task<JsonResponse> MethodAsync(string url, string method, JsonDict body = null)
        {
            RecordedRequest request = new RecordedRequest
            {
                Url = url,
                Method = method,
                Body = body
            };

            RequestAndResponse rar;
            if (_playback)
            {
                rar = _recording[_playbackIndex];
                if (!request.Matches(rar.Request))
                    throw new Exception($"Actual request doesn't match recorded at index {_playbackIndex}");
                _playbackIndex++;
                return rar.Response;
            }
            else
            {
                JsonResponse jr = await base.MethodAsync(url, method, body);
                rar = new RequestAndResponse
                {
                    Request = request,
                    Response = jr
                };
                _recording.Add(rar);
                return jr;
            }
        }
    }
}
