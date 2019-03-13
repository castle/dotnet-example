# Castle .NET Example
This is an example of integrating Castle with a standard ASP&#46;NET Core Razor Pages application.

## Foundation
The example is almost fully from the default Visual Studio template for a Razor Pages app with *Invididual user accounts* for authentication.
### Framework
ASP&#46;NET Core 2.2

### Template modifications
* The database runs in-memory
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("CastleDemo")
);
```
* Visual Studio scaffolding has been used to create a Login page we can alter, as described by Microsoft [here](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity).
* We use [Microsoft.VisualStudio.Threading](https://www.nuget.org/packages/Microsoft.VisualStudio.Threading/) to get access to the `Forget()` extension method, which is useful for fire-and-forget calls to async methods, like **Track** or **Authenticate** in Monitor mode.

## The integration
The example application applies the steps described for the [Castle Baseline Integration](https://castle.io/docs/baseline), with the addition of [secure requests](https://castle.io/docs/securing_requests). All Castle-related changes are marked with comments containing the word *Castle* for easy searching, and affect the following files:

* `Startup.cs` Ioc
* `Areas/Identity/Pages/Account/Login.cshtml.cs` Castle SDK calls
* `Pages/Shared/_Layout.cshtml` Client-side Castle
* `appsettings.json` Your Castle API secret