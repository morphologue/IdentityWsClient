using System.Collections.Immutable;
using Morphologue.IdentityWsClient.Extensions;
using Morphologue.IdentityWsClient.Testability;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient
{
    public class Client
    {
        private IJsonClient _json;
        private string _url;

        public ImmutableDictionary<string, string> Data { get; private set; }

        internal Client(IJsonClient json, string url, JsonDict data)
        {
            _json = json;
            _url = url;
            Data = data.ToStringValues().ToImmutableDictionary();
        }
    }
}
