# OpenUSSD

**NuGet:** [OpenUSSD](https://www.nuget.org/packages/OpenUSSD/)

A .NET 8 SDK for building **USSD** (phone interactive menus) on **ASP.NET Core**: typed menus, sessions, pagination, and action handlers—without scattering magic strings through your code.

---

## Start here if you are new

| Step | What to do |
|------|------------|
| 1 | [Install](#1-install-the-package) the NuGet package in an ASP.NET Core web project. |
| 2 | [Define](#2-define-a-menu-enum) an `enum` for your screens (each value is a menu “page”). |
| 3 | [Build](#3-build-the-menu) the menu with `UssdMenuBuilder` (titles, options, `.Action<THandler>()`). |
| 4 | [Register](#4-wire-up-dependency-injection) the SDK and your handlers in `Program.cs`. |
| 5 | [Expose](#5-add-an-http-endpoint) a POST endpoint that forwards `UssdRequestDto` to `UssdApp`. |

After that, run the app and POST JSON to your `/ussd` route (see [Try the sample](#try-the-sample)). For full API detail, use **[docs/sdk.md](docs/sdk.md)**.

---

## Prerequisites

- **.NET 8** or later  
- An **ASP.NET Core** app (minimal APIs or controllers)

---

## 1. Install the package

```bash
dotnet add package OpenUSSD
```

---

## 2. Define a menu enum

Each enum member represents a step or screen in your flow (you can add more pages for transfers, PIN entry, etc.):

```csharp
public enum BankMenu
{
    Main,
}
```

---

## 3. Build the menu

For each screen, use **`.Page(...)`**, set the text with **`.Title(...)`**, and add **`.Option(...)`** for numbered choices. **`.Action<MyHandler>()`** wires a handler to an option. For free-text steps (for example a phone number), use **`.Input().Action<...>()`** on another page—the sample project shows a full flow.

```csharp
using Bobcode.Ussd.Arkesel.Builders;

var menu = new UssdMenuBuilder<BankMenu>("demo_bank")
    .Root(BankMenu.Main)
    .Page(BankMenu.Main, n => n
        .Title("Welcome to Demo Bank")
        .Option("1", "Check balance").Action<BalanceCheckHandler>())
    .Build();
```

---

## 4. Wire up dependency injection

In `Program.cs`, register the menu, optional `UssdOptions`, and discover handlers marked with `[UssdAction]`:

```csharp
using Bobcode.Ussd.Arkesel.Core;

builder.Services.AddUssdSdk(menu, options =>
{
    options.SessionTimeout = TimeSpan.FromMinutes(5);
    options.BackCommand = "0";   // go back
    options.HomeCommand = "#";   // main menu
});

builder.Services.AddUssdActionsFromAssembly(typeof(Program).Assembly);
```

**Defaults:** sessions are stored **in memory** (fine for local development). For production behind multiple instances, use **Redis** (see [below](#redis-session-store-production)).

---

## 5. Add an HTTP endpoint

The gateway sends a JSON body; you pass it to `UssdApp` and return the response:

```csharp
using Bobcode.Ussd.Arkesel.Core;
using Bobcode.Ussd.Arkesel.Models;

app.MapPost("/ussd", async (UssdRequestDto request, UssdApp ussdApp) =>
{
    var response = await ussdApp.HandleRequestAsync(request);
    return Results.Ok(response);
});
```

---

## 6. Implement an action handler

Handlers run when the user hits an option wired with `.Action<...>()`. Inheriting **`BaseActionHandler`** gives helpers like **`End(...)`** and **`Continue(...)`**. **`[UssdAction]`** (no argument) derives the action key from the class name (e.g. `BalanceCheckHandler` → `BalanceCheck`).

```csharp
using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Attributes;

[UssdAction]
public class BalanceCheckHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var balance = 1500.00m; // load from your database
        return Task.FromResult(End($"Your balance is GHS {balance:N2}"));
    }
}
```

You can also implement **`IActionHandler`** directly and set **`Key`** yourself; see **[docs/sdk.md](docs/sdk.md)**.

---

## Request JSON (what the gateway sends)

Typical shape (property names are usually **camelCase** in JSON, e.g. `sessionID`, `newSession`):

**First dial (new session):**

```json
{
  "sessionID": "unique-session-id",
  "userID": "user123",
  "msisdn": "233271234567",
  "userData": "",
  "newSession": true,
  "network": "MTN"
}
```

**Later inputs:** same `sessionID`, set `"newSession": false`, and put the user’s keypress in **`userData`** (e.g. `"1"`).

---

## Try the sample

The repo includes a full demo app:

```bash
cd Bobcode.Ussd.Arkesel.Sample
dotnet run
```

Open **Swagger** at the URL shown in the console (see `launchSettings.json` for ports—often `https://localhost:5xxx/swagger`). Use **POST `/ussd`** with the JSON examples in **[Bobcode.Ussd.Arkesel.Sample/README.md](Bobcode.Ussd.Arkesel.Sample/README.md)**.

---

## What you get out of the box

- Typed navigation via enums and **`UssdMenuBuilder`**
- **`UssdApp`** orchestration, session handling, back/home, pagination
- **`SessionKey<T>`** for typed session data (see **[docs/sdk.md](docs/sdk.md)**)
- Optional **session resumption** via `UssdOptions.EnableSessionResumption`
- Pluggable **`IUssdSessionStore`** (memory, Redis, or your own)

---

## Redis session store (production)

Register **`RedisSessionStore`** and **`IConnectionMultiplexer`** as usual for StackExchange.Redis, then:

```csharp
builder.Services.AddUssdSdk<RedisSessionStore>(menu, options =>
{
    options.SessionTimeout = TimeSpan.FromMinutes(10);
});
```

---

## More documentation

| Doc | Contents |
|-----|----------|
| **[docs/sdk.md](docs/sdk.md)** | Architecture, `UssdOptions`, pagination, custom session stores |
| **[docs/sample.md](docs/sample.md)** | Sample patterns and walkthroughs |
| **[Bobcode.Ussd.Arkesel.Sample/README.md](Bobcode.Ussd.Arkesel.Sample/README.md)** | Sample menus, handlers, and curl-style requests |

---

## License

MIT — see the [LICENSE](LICENSE) file.

## Contributing

Pull requests are welcome.

## Support

Use the GitHub issue tracker for bugs and questions.
