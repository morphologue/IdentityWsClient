using System;
using System.Net;

namespace Morphologue.IdentityWsClient
{
    public class IdentityException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public IdentityException(HttpStatusCode statusCode, string explanation) : base(explanation)
        {
            StatusCode = statusCode;
        }
    }
}
