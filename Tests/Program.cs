using System;
using System.Net.Http;
using Morphologue.IdentityWsClient;
using Morphologue.IdentityWsClient.Testability;
using FluentAssertions;

namespace Morphologue.IdentityWsClient.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup
            HttpClient http = new HttpClient();
            http.BaseAddress = new Uri("http://localhost:5000/");
            bool record = args.Length > 0 && args[0].Trim() == "--record";
            RecordingHttpJsonClient json = new RecordingHttpJsonClient(http, "Recording.json", !record);
            IdentityWs ws = new IdentityWs(json);

            // Narrative test
            ws.GetAliasAsync("test@test.org").Result.Should().BeNull("the alias has not been created yet");
            Alias alias1 = ws.CreateAliasAsync("test@test.org", "password1").Result;
            alias1.Should().NotBeNull("the alias was just created");
            alias1.IsConfirmed.Should().BeFalse("the alias has not been confirmed yet");
            alias1.GetClientAsync("test").Result.Should().BeNull("the client has not been created yet");

            // Epilogue
            if (record)
                json.Persist();
            Console.WriteLine("Test run succeeded");
        }
    }
}
