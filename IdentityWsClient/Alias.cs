using System.Net;
using System.Threading.Tasks;
using Morphologue.IdentityWsClient.Testability;
using Morphologue.IdentityWsClient.Extensions;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;

namespace Morphologue.IdentityWsClient
{
    public class Alias
    {
        private IJsonClient _json;
        private string _url, _confirmationToken;
        private bool _confirmationTokenLoaded;

        public string ConfirmationToken
        {
            get
            {
                if (!_confirmationTokenLoaded)
                    LoadConfirmationToken();
                return _confirmationToken;
            }
        }

        public bool IsConfirmed => ConfirmationToken == null;

        internal Alias(IJsonClient json, string url, string confirmationToken) : this(json, url)
        {
            _confirmationToken = confirmationToken;
            _confirmationTokenLoaded = true;
        }

        internal Alias(IJsonClient json, string url)
        {
            _json = json;
            _url = url;
        }

        public async Task<Client> GetClientAsync(string clientName)
        {
            string callee = Callee(clientName);
            JsonResponse response = await _json.GetAsync(callee);
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return null;
                case HttpStatusCode.OK:
                    return new Client(_json, callee, response.Content);
                default:
                    response.StatusCode.Throw();
                    return null;
            }
        }

        public async Task ChangePasswordAsync(string oldPassword, string newPassword)
        {
            JsonResponse response = await _json.PatchAsync(_url, new JsonDict
            {
                ["oldPassword"] = oldPassword,
                ["password"] = newPassword
            });
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new IdentityException(response.StatusCode, "The existing password does not match");
                case HttpStatusCode.Conflict:
                    throw new IdentityException(response.StatusCode, "The new password must differ from the existing one");
                case HttpStatusCode.NoContent:
                    return;
                default:
                    response.StatusCode.Throw();
                    break;
            }
        }

        public async Task ChangePasswordViaResetTokenAsync(string resetToken, string newPassword)
        {
            JsonResponse response = await _json.PatchAsync(_url, new JsonDict
            {
                ["resetToken"] = resetToken,
                ["password"] = newPassword
            });
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new IdentityException(response.StatusCode, "The reset token is invalid");
                case HttpStatusCode.Conflict:
                    throw new IdentityException(response.StatusCode, "The new password must differ from the existing one");
                case HttpStatusCode.NoContent:
                    return;
                default:
                    response.StatusCode.Throw();
                    break;
            }
        }

        public async Task DeleteAsync()
        {
            JsonResponse response = await _json.DeleteAsync(_url);
            if (response.StatusCode != HttpStatusCode.NoContent)
                response.StatusCode.Throw();
        }

        public async Task<string> GenerateResetTokenAsync()
        {
            JsonResponse response = await _json.PostAsync(_url);
            if (response.StatusCode != HttpStatusCode.OK)
                response.StatusCode.Throw();
            return response.Content["resetToken"].ToString();
        }

        private void LoadConfirmationToken()
        {
            JsonResponse response = _json.GetAsync(_url).Result;
            if (response.StatusCode != HttpStatusCode.OK)
                response.StatusCode.Throw();
            _confirmationToken = response.Content["confirmToken"].ToString();
            _confirmationTokenLoaded = true;
        }

        private string Callee(string clientName, string action = null) =>
            $"{_url}/clients/{clientName}" + (action == null ? "" : $"/{action}");
    }
}
