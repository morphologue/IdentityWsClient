using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient.Extensions
{
    internal static class DictionaryExtensions
    {
        public static HttpContent ToHttpContent(this JsonDict dict)
        {
            string body = JsonConvert.SerializeObject(dict);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        public static Dictionary<string, string> ToStringValues(this JsonDict dict)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object> kv in dict)
                result.Add(kv.Key, kv.Value?.ToString());
            return result;
        }

        public static bool ShallowEquals(this JsonDict dict, JsonDict other)
        {
            if (dict.Count != other.Count)
                return false;
            foreach (KeyValuePair<string, object> pair in dict)
            {
                if (!other.ContainsKey(pair.Key))
                    return false;
                if (pair.Value == null)
                {
                    if (other[pair.Key] != null)
                        return false;
                }
                else
                {
                    if (!pair.Value.Equals(other[pair.Key]))
                        return false;
                }
            }
            return true;
        }
    }
}
