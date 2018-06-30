using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Morphologue.IdentityWsClient.Testability;
using Morphologue.IdentityWsClient.Extensions;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient
{
    public class IdentityWs
    {
        private IJsonClient _json;

        internal IdentityWs(IJsonClient json)
        {
            _json = json;
        }

        public IdentityWs(HttpClient http)
        {
            _json = new HttpJsonClient(http);
        }

        public async Task<Alias> GetAliasAsync(string emailAddress)
        {
            string callee = Callee(emailAddress);
            JsonResponse response = await _json.GetAsync(callee);
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return null;
                case HttpStatusCode.OK:
                    return new Alias(_json, callee, response.Content["confirmToken"].ToString());
                default:
                    response.StatusCode.Throw();
                    return null;
            }
        }

        public async Task<Alias> CreateAliasAsync(string emailAddress, string password)
        {
            string callee = Callee(emailAddress);
            JsonResponse response = await _json.PostAsync(callee, new JsonDict
            {
                ["password"] = password
            });
            switch (response.StatusCode)
            {
                case HttpStatusCode.Conflict:
                    throw new IdentityException(response.StatusCode, "The email address is not available");
                case HttpStatusCode.NoContent:
                    return new Alias(_json, callee);
                default:
                    response.StatusCode.Throw();
                    return null;
            }
        }

        public async Task<Alias> CreateLinkedAliasAsync(string emailAddress, string otherEmailAddress)
        {
            string callee = Callee(emailAddress);
            JsonResponse response = await _json.PostAsync(callee, new JsonDict
            {
                ["otherEmailAddress"] = otherEmailAddress
            });
            switch (response.StatusCode)
            {
                case HttpStatusCode.Conflict:
                    throw new IdentityException(response.StatusCode, "The email address is not available");
                case HttpStatusCode.NotFound:
                    throw new IdentityException(response.StatusCode, $"Cannot find existing alias {otherEmailAddress}");
                case HttpStatusCode.NoContent:
                    return new Alias(_json, callee);
                default:
                    response.StatusCode.Throw();
                    return null;
            }
        }

        private string Callee(string emailAddress) => "aliases/" + emailAddress;
    }
}
