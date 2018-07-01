using Morphologue.IdentityWsClient.Extensions;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient.Tests
{
    internal class RecordedRequest
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
}
