using System.Net;
using System.Net.Http;

namespace Morphologue.IdentityWsClient.Extensions
{
    internal static class HttpStatusCodeExtensions
    {
        public static void Throw(this HttpStatusCode statusCode)
        {
            throw new HttpRequestException($"Unexpected status code {statusCode}");
        }
    }
}
