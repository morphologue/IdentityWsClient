using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Threading.Tasks;
using Morphologue.IdentityWsClient.Extensions;
using Morphologue.IdentityWsClient.Testability;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient
{
    public class Client
    {
        private IJsonClient _json;
        private string _url;

        public string Name { get; private set; }

        public ImmutableDictionary<string, string> Data { get; private set; }

        internal Client(IJsonClient json, string url, string name, Dictionary<string, string> data)
        {
            _json = json;
            _url = url;
            Name = name;
            Data = data.ToImmutableDictionary();
        }

        public async Task DeleteAsync()
        {
            JsonResponse response = await _json.DeleteAsync(_url);
            if (response.StatusCode != HttpStatusCode.NoContent)
                response.StatusCode.Throw();
        }

        [Obsolete("Use LogInAsync() instead")]
        public Task LogIn(string password) => LogInAsync(password);

        public async Task LogInAsync(string password)
        {
            JsonResponse response = await _json.PostAsync(_url + "/login", new JsonDict
            {
                ["password"] = password
            });
            switch (response.StatusCode)
            {
                case HttpStatusCode.ServiceUnavailable:
                    throw new IdentityException(response.StatusCode, "The account is locked - please try again later");
                case HttpStatusCode.Unauthorized:
                    throw new IdentityException(response.StatusCode, "The password does not match");
                case HttpStatusCode.NoContent:
                    return;
                default:
                    response.StatusCode.Throw();
                    return;
            }
        }
    }
}
