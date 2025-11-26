# 📘 SDK Documentation

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

# OpenUSSD SDK Documentation

A strongly-typed, extensible SDK for building USSD Applications in .NET.

---

# **1. Introduction**

OpenUSSD is a modern .NET SDK designed to simplify USSD application development by providing:

### ✔ Strong typing (no magic strings)

### ✔ Attribute-based menu & action auto-discovery

### ✔ Enum-based menu navigation

### ✔ Built-in session management

### ✔ Pagination support

### ✔ Multi-menu support

### ✔ Modular architecture (Menus, Handlers, Session, Registry)

### ✔ Zero-boilerplate Program.cs

This SDK abstracts the complexity of building USSD apps so engineers can focus on business logic.

---

# **2. Core Concepts**

## **2.1 Menu Definitions**

Each menu is a class that inherits from `UssdMenu` and is automatically discovered using `[UssdMenu]`.

```csharp
[UssdMenu]
public class BankMenu : UssdMenu
{
    public override string MenuId => "bank";
    public override bool IsDefault => true;

    public override Menu BuildMenu()
    {
        return new UssdMenuBuilder<BankMenuNode>("bank")
            .Root(BankMenuNode.Main)
            .Node(BankMenuNode.Main, n => n
                .Message("Welcome to Bank")
                .Option("1", "Balance").Action<BalanceCheckHandler>()
                .Option("2", "Transfer").GoTo(BankMenuNode.Transfer)
            )
            .Build();
    }
}
```

## **2.2 Node Enums**

Menus use enums to prevent string-based navigation errors.

```csharp
public enum BankMenuNode
{
    Main,
    Transfer,
    CheckBalance
}
```

## **2.3 Action Handlers**

Handlers implement business logic and are auto-discovered via `[UssdAction]`.

```csharp
[UssdAction]
public class BalanceCheckHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        return Task.FromResult(End($"Your balance: GHS 150.50")));
    }
}
```

## **2.4 Session Keys**

Strongly-typed session storage:

```csharp
public static SessionKey<string?> Recipient => new("recipient");
```

---

# **3. Auto-Discovery Architecture**

### Auto-discovered components:

| Type                | How?                             |
| ------------------- | -------------------------------- |
| **Menus**           | `[UssdMenu]` attribute           |
| **Action handlers** | `[UssdAction]` attribute         |
| **Menu registry**   | Automatic assembly scanning      |
| **Default menu**    | First menu with `IsDefault=true` |

This removes all manual registration code.

---

# **4. Program.cs Setup**

```csharp
builder.Services.AddUssdSdk(options =>
{
    options.SessionTimeout = TimeSpan.FromMinutes(5);
    options.BackCommand = "0";
    options.HomeCommand = "#";
    options.EnablePagination = true;
    options.EnableSessionResumption = true;
});
```

### Endpoint:

```csharp
app.MapPost("/ussd", async (UssdRequestDto request, UssdApp ussdApp) =>
{
    return Results.Ok(await ussdApp.HandleRequestAsync(request));
});
```

---

# **5. SDK Options**

```csharp
options.SessionTimeout = TimeSpan.FromMinutes(5);

options.BackCommand = "0";
options.HomeCommand = "#";

options.EnablePagination = true;
options.ItemsPerPage = 5;

options.NextPageCommand = "99";
options.PreviousPageCommand = "98";

options.EnableSessionResumption = true;
options.ResumeSessionPrompt = "You have a pending session.";
```

---

# **6. Multiple Menus**

The SDK supports running multiple services in a single USSD application.

```csharp
[UssdMenu]
public class AirtimeMenu : UssdMenu
{
    public override string MenuId => "airtime";
}
```

To route:

```json
{
  "MenuId": "airtime"
}
```

---

# **7. Building Menus with UssdMenuBuilder**

Fluent builder API:

```csharp
.Node(BankMenuNode.Transfer, n => n
    .Message("Enter amount")
    .Input().Action<TransferAmountHandler>()
)
```

Supports:

* `.Message(text)`
* `.Option(key, text)`
* `.Input()`
* `.Action<THandler>()`
* `.GoTo(Node)`
* Pagination

---

# **8. Session Management**

Access session:

```csharp
var amount = Get(context, SessionKeys.Amount);
Set(context, SessionKeys.Amount, 400m);
```

Session is persisted automatically.

---

# **9. Pagination Example**

```csharp
.OptionList(items, x => x.Name, x => x.Id);
```

SDK handles:

* page splits
* next/prev commands
* item indexing

---

# **10. Error Handling**

Built-in:

* Invalid input
* Missing node
* Missing action handler
* Multiple default menus

---

# **11. Extensibility**

You can replace:

* Session store
* Menu discovery
* Handlers
* Menu registry
* Options provider

Example:

```csharp
builder.Services.AddUssdSdk<RedisSessionStore>();
```
