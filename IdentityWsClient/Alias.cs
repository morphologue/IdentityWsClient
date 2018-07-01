using System.Net;
using System.Threading.Tasks;
using Morphologue.IdentityWsClient.Testability;
using Morphologue.IdentityWsClient.Extensions;
using JsonDict = System.Collections.Generic.Dictionary<string, object>;
using System.Collections.Generic;
using System;

namespace Morphologue.IdentityWsClient
{
    public class Alias
    {
        private IJsonClient _json;
        private string _url, _confirmationToken;
        
        public bool IsConfirmationTokenLoaded { get; private set; }

        public string EmailAddress { get; private set; }

        public string ConfirmationToken
        {
            get
            {
                if (!IsConfirmationTokenLoaded)
                    throw new InvalidOperationException($"The confirmation token has not been loaded: call {nameof(FetchConfirmationTokenAsync)}()");
                return _confirmationToken;
            }
        }

        // If false, the Alias still might have been confirmed via a different Alias. Call
        // FetchConfirmationTokenAsync() to refresh.
        public bool IsConfirmed => ConfirmationToken == null;

        internal Alias(IJsonClient json, string url, string emailAddress, string confirmationToken) : this(json, url, emailAddress)
        {
            _confirmationToken = confirmationToken;
            IsConfirmationTokenLoaded = true;
        }

        internal Alias(IJsonClient json, string url, string emailAddress)
        {
            _json = json;
            _url = url;
            EmailAddress = emailAddress;
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
                    return new Client(_json, callee, clientName, response.Content.ToStringValues());
                default:
                    response.StatusCode.Throw();
                    return null;
            }
        }

        public async Task<Client> CreateClientAsync(string clientName, Dictionary<string, string> data = null)
        {
            // Convert data to JsonDict.
            if (data == null)
                data = new Dictionary<string, string>();
            JsonDict objData = new JsonDict();
            foreach (KeyValuePair<string, string> pair in data)
                objData.Add(pair.Key, pair.Value);

            string callee = Callee(clientName);
            JsonResponse response = await _json.PostAsync(callee, objData);
            switch (response.StatusCode)
            {
                case HttpStatusCode.Conflict:
                    throw new IdentityException(response.StatusCode, "The client name is not available");
                case HttpStatusCode.NoContent:
                    return new Client(_json, callee, clientName, data);
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
            switch (response.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    throw new IdentityException(response.StatusCode, "The last alias may not be deleted");
                case HttpStatusCode.NoContent:
                    return;
                default:
                    response.StatusCode.Throw();
                    return;
            }
        }

        public async Task<string> GenerateResetTokenAsync()
        {
            JsonResponse response = await _json.PostAsync(_url + "/reset");
            if (response.StatusCode != HttpStatusCode.OK)
                response.StatusCode.Throw();
            return response.Content["resetToken"].ToString();
        }

        public async Task ConfirmAsync(string confirmationToken)
        {
            JsonResponse response = await _json.PostAsync(_url + "/confirm", new JsonDict
            {
                ["confirmToken"] = confirmationToken
            });
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new IdentityException(response.StatusCode, "The confirmation token is invalid");
                case HttpStatusCode.NoContent:
                    _confirmationToken = null;
                    IsConfirmationTokenLoaded = true;
                    return;
                default:
                    response.StatusCode.Throw();
                    return;
            }
        }

        public async Task Email(string from, string subject, string bodyText, string bodyHtml = null, string replyTo = null,
            bool sendIfUnconfirmed = false)
        {
            JsonResponse response = await _json.PostAsync(_url + "/email", new JsonDict
            {
                ["from"] = from,
                ["replyTo"] = replyTo,
                ["subject"] = subject,
                ["bodyText"] = bodyText,
                ["bodyHTML"] = bodyHtml,
                ["sendIfUnconfirmed"] = sendIfUnconfirmed
            });
            if (response.StatusCode != HttpStatusCode.NoContent)
                response.StatusCode.Throw();
        }

        public async Task FetchConfirmationTokenAsync()
        {
            JsonResponse response = await _json.GetAsync(_url);
            if (response.StatusCode != HttpStatusCode.OK)
                response.StatusCode.Throw();
            _confirmationToken = response.Content["confirmToken"]?.ToString();
            IsConfirmationTokenLoaded = true;
        }

        private string Callee(string clientName) => $"{_url}/clients/{WebUtility.UrlEncode(clientName)}";
    }
}
