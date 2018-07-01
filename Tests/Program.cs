using System;
using System.Net.Http;
using Morphologue.IdentityWsClient;
using Morphologue.IdentityWsClient.Testability;
using FluentAssertions;
using System.Net;
using System.Collections.Generic;

namespace Morphologue.IdentityWsClient.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            // Prologue
            IJsonClient json = Setup(args);
            IdentityWs ws = new IdentityWs(json);

            // Alias creation/linking
            ws.GetAliasAsync("test@test.org").Result.Should().BeNull("the alias has not been created yet");
            Alias alias1 = ws.CreateAliasAsync("test@test.org", "password1").Result;
            Action linkWrong = () => ws.CreateLinkedAliasAsync("test.linked@test.org", "wrong@test.org").Wait();
            linkWrong.Should().Throw<IdentityException>().Which.StatusCode.Should().Be(HttpStatusCode.NotFound, "the link target doesn't exist");
            Action linkDup = () => ws.CreateLinkedAliasAsync("test@test.org", "test@test.org").Wait();
            linkDup.Should().Throw<IdentityException>().Which.StatusCode.Should().Be(HttpStatusCode.Conflict, "the alias already exists");
            Action createDup = () => ws.CreateAliasAsync("test@test.org", "password2").Wait();
            createDup.Should().Throw<IdentityException>().Which.StatusCode.Should().Be(HttpStatusCode.Conflict, "the alias already exists");
            Alias alias2 = ws.CreateLinkedAliasAsync("test.linked@test.org", "test@test.org").Result;
            Action changeWrong = () => alias2.ChangePasswordAsync("wrong", "strong.password").Wait();
            changeWrong.Should().Throw<IdentityException>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "the password is wrong");
            alias2.ChangePasswordAsync("password1", "strong.password").Wait();
            alias1.ChangePasswordAsync("strong.password", "stronger.password").Wait();

            // Password reset
            string resetTok = alias2.GenerateResetTokenAsync().Result;
            string resetTokWrong = (resetTok[0] == '0' ? "1" : "0") + resetTok.Substring(1);
            Action resetWrong = () => alias2.ChangePasswordViaResetTokenAsync(resetTokWrong, "impossible").Wait();
            resetWrong.Should().Throw<IdentityException>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "the reset token is wrong");
            alias2.ChangePasswordViaResetTokenAsync(resetTok, "strongest.password").Wait();

            // Confirmation and email
            alias1.IsConfirmationTokenLoaded.Should().BeFalse("the token is automatically loaded only on alias get, not create");
            Action unfetched = () => _ = alias1.IsConfirmed;
            unfetched.Should().Throw<InvalidOperationException>("the confirmation token has not been loaded");
            alias1.FetchConfirmationTokenAsync().Wait();
            alias1.IsConfirmed.Should().BeFalse("the alias has not been confirmed yet");
            string confTok = alias1.ConfirmationToken;
            string confTokWrong = (confTok[0] == '0' ? "1" : "0") + confTok.Substring(1);
            Action confWrong = () => alias1.ConfirmAsync(confTokWrong).Wait();
            confWrong.Should().Throw<IdentityException>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "the confirmation token is wrong");
            alias1.ConfirmAsync(confTok).Wait();
            alias1.IsConfirmed.Should().BeTrue("the alias was just confirmed");
            ws.GetAliasAsync("test@test.org").Result.IsConfirmed.Should().BeTrue("it's a copy of the confirmed alias");
            alias2.FetchConfirmationTokenAsync().Wait();
            alias2.IsConfirmed.Should().BeFalse("confirmation is at the alias level");
            alias2.Email("noreply@test.org", "Hello", "World!", "<html><body><strong>World!</strong></body></html>").Wait();

            // Client creation
            alias1.GetClientAsync("test1").Result.Should().BeNull("the client has not been created yet");
            Client client1 = alias1.CreateClientAsync("test1").Result;
            Action clientDup = () => alias2.CreateClientAsync("test1").Wait();
            clientDup.Should().Throw<IdentityException>().Which.StatusCode.Should().Be(HttpStatusCode.Conflict, "the client already exists for the being");
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["flubble"] = "1",
                ["Bup"] = "False"
            };
            Client client2 = alias2.CreateClientAsync("test2", data).Result;
            alias1.GetClientAsync("test2").Result.Data.Should().BeEquivalentTo(data, "it's a copy of the client just created with such data");

            // Login and alias deletion
            Action oldPassword = () => client1.LogIn("password1").Wait();
            oldPassword.Should().Throw<IdentityException>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "the password was changed");
            client1.LogIn("strongest.password").Wait();
            alias1.DeleteAsync().Wait();
            Action tooEnthusiastic = () => alias2.DeleteAsync().Wait();
            tooEnthusiastic.Should().Throw<IdentityException>().Which.StatusCode.Should().Be(HttpStatusCode.Forbidden, "one alias must remain");
            client2.LogIn("strongest.password").Wait();
            Action tooLate = () => client1.LogIn("strongest.password").Wait();
            tooLate.Should().Throw<HttpRequestException>("the server will 404 because the alias has been deleted");

            // Client deletion
            client2.DeleteAsync().Wait();
            Alias alias2Copy = ws.GetAliasAsync("test.linked@test.org").Result;
            Client client1ViaAlias2Copy = alias2Copy.GetClientAsync("test1").Result;
            client1ViaAlias2Copy.LogIn("strongest.password").Wait();
            client1ViaAlias2Copy.DeleteAsync().Wait();
            ws.GetAliasAsync("test.linked@test.org").Result.Should().BeNull("everything should be deleted once the last service is deleted");

            // Epilogue
            (json as RecordingHttpJsonClient)?.Persist();
            Console.WriteLine("Test run succeeded");
        }

        private static IJsonClient Setup(string[] args)
        {
            const string FILE_NAME = "Recording.json";

            if (args.Length > 0 && args[0].Trim() == "--record")
            {
                HttpClient http = new HttpClient();
                http.BaseAddress = new Uri("http://localhost:5000/");
                return new RecordingHttpJsonClient(http, FILE_NAME);
            }
            else
            {
                return new ReplayingJsonClient(FILE_NAME);
            }
        }
    }
}
