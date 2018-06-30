using System.Net;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient.Testability
{
    internal class JsonResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public JsonDict Content { get; set; }
    }
}
