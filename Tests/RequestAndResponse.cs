using Morphologue.IdentityWsClient.Testability;

namespace Morphologue.IdentityWsClient.Tests
{
    internal class RequestAndResponse
    {
        public RecordedRequest Request { get; set; }
        public JsonResponse Response { get; set; }
    }
}
