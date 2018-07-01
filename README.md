# IdentityWs Client
A client which makes it easy to use the
[Identity Web Service](https://github.com/morphologue/IdentityWs) in a .NET project.

## Basic operation
The `IdentityWs` class can be used as a typed HttpClient in .NET Core 2.1, e.g.:

```c#
using Morphologue.IdentityWsClient;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<IdentityWs>(http => http.BaseUrl = "http://localhost:5000/");
    }
}
```

Or if you're not using .NET Core 2.1 you can just construct an IdentityWs and pass it a suitably
configured HttpClient.

Once you have your IdentityWs instance, you can use it to get an Alias or create a new one (linked
or freestanding), e.g.:

```c#
Alias created = await identity.CreateAliasAsync("test@test.org", "password1");
Alias linked = await identity.CreateLinkedAliasAsync("test.linked@test.org", "test@test.org");
Alias gotten = await identity.GetAliasAsync("test@test.org");
```

Once you have an Alias, you can use it to change the being's password (via the existing password
or reset token), confirm the Alias's email address, asynchronously send an email or delete the
Alias.

From an Alias you can also get or create a Client. The Client is associated with the Alias's being
and may contain arbitrary data or be used to log in. Once the last Client has been deleted from a
being, the entire being is deleted.

For detailed usage examples, see Program.cs in the Tests folder. Concepts are explained in depth in
the [Identity Web Service](https://github.com/morphologue/IdentityWs) README.

## Running the tests
The following will run the tests against pre-recorded server requests and responses:

```
cd Tests
dotnet run
```

## Licence
[GPL3](https://www.gnu.org/licenses/gpl-3.0.en.html)
