using System.Net;
using System.Threading.Tasks;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient.Testability
{
    // The facets of HttpClient which this library uses
    internal interface IJsonClient
    {
        Task<JsonResponse> GetAsync(string url);
        Task<JsonResponse> PostAsync(string url, JsonDict body = null);
        Task<JsonResponse> PatchAsync(string url, JsonDict body);
        Task<JsonResponse> DeleteAsync(string url);
    }
}
