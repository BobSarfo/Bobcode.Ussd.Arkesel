# OpenUSSD

A modern, strongly-typed .NET SDK for building USSD (Unstructured Supplementary Service Data) applications with ASP.NET Core.

## Overview

OpenUSSD simplifies USSD application development by providing a clean, type-safe API with built-in session management, pagination, and navigation support. Build complex USSD flows without magic strings or boilerplate code.

## Key Features

- **Strongly-Typed Navigation** - Use enums instead of magic strings for menu navigation
- **Fluent Menu Builder** - Intuitive API for building hierarchical menu structures
- **Action Handlers** - Modular, testable business logic handlers
- **Session Management** - Built-in session storage with type-safe session keys
- **Pagination Support** - Automatic pagination for long lists
- **Multi-Step Flows** - Easy implementation of complex multi-step processes
- **Session Resumption** - Allow users to resume interrupted sessions
- **Navigation Commands** - Built-in back and home navigation
- **Dependency Injection** - Full DI support throughout the SDK
- **Extensible Architecture** - Replace session stores, customize behavior

## Quick Start

### Installation

```bash
dotnet add package OpenUSSD
```

### Basic Usage

#### 1. Define Menu Nodes

```csharp
public enum BankMenuNode
{
    Main,
    CheckBalance,
    Transfer,
    TransferRecipient,
    TransferAmount,
    TransferConfirm
}
```

#### 2. Build Menu Structure

```csharp
var menu = new UssdMenuBuilder<BankMenuNode>("bank")
    .Root(BankMenuNode.Main)

    .Node(BankMenuNode.Main, n => n
        .Message("Welcome to Demo Bank")
        .Option("1", "Check Balance").Action<BalanceCheckHandler>()
        .Option("2", "Transfer Money").GoTo(BankMenuNode.TransferRecipient)
    )

    .Node(BankMenuNode.TransferRecipient, n => n
        .Message("Enter recipient phone number:")
        .Input().Action<TransferRecipientHandler>()
    )

    .Build();
```

#### 3. Configure Services

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUssdSdk(menu, options =>
{
    options.SessionTimeout = TimeSpan.FromMinutes(5);
    options.BackCommand = "0";
    options.HomeCommand = "#";
    options.EnablePagination = true;
    options.ItemsPerPage = 5;
    options.EnableSessionResumption = true;
});

builder.Services.AddUssdActionsFromAssembly(typeof(Program).Assembly);
```

#### 4. Create Action Handlers

```csharp
[UssdAction("balance_check")]
public class BalanceCheckHandler : IActionHandler
{
    public Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var balance = 1500.00m; // Fetch from database

        return Task.FromResult(new UssdStepResult
        {
            Message = $"Your balance is GHS {balance:N2}",
            ContinueSession = false
        });
    }
}
```

#### 5. Create Endpoint

```csharp
app.MapPost("/ussd", async (UssdRequestDto request, UssdApp ussdApp) =>
{
    var response = await ussdApp.HandleRequestAsync(request);
    return Results.Ok(response);
});
```

## Configuration Options

The SDK supports extensive configuration through `UssdOptions`:

```csharp
builder.Services.AddUssdSdk(menu, options =>
{
    // Session management
    options.SessionTimeout = TimeSpan.FromMinutes(5);

    // Navigation commands
    options.BackCommand = "0";
    options.HomeCommand = "#";
    options.EnableAutoBackNavigation = true;

    // Pagination
    options.EnablePagination = true;
    options.ItemsPerPage = 5;
    options.NextPageCommand = "99";
    options.PreviousPageCommand = "98";

    // Messages
    options.InvalidInputMessage = "Invalid input. Please try again.";
    options.DefaultEndMessage = "Thank you for using our service.";

    // Session resumption
    options.EnableSessionResumption = true;
    options.ResumeSessionPrompt = "You have an active session.";
    options.ResumeOptionLabel = "Resume";
    options.StartFreshOptionLabel = "Start Fresh";
});
```

## Advanced Features

### Session Management

Use strongly-typed session keys for type-safe data storage:

```csharp
public static class SessionKeys
{
    public static SessionKey<string?> Recipient => new("recipient");
    public static SessionKey<decimal?> Amount => new("amount");
}

// In your handler
Set(context, SessionKeys.Recipient, phoneNumber);
var recipient = Get(context, SessionKeys.Recipient);
```



### Pagination

Automatically paginate long lists:

```csharp
.Node(BankMenuNode.Products, n => n
    .Message("Our Products:")
    .OptionList(products,
        p => $"{p.Name} - GHS {p.Price}",
        autoPaginate: true,
        itemsPerPage: 3)
)
```

### Custom Session Store

Replace the default in-memory session store with Redis or other implementations:

```csharp
builder.Services.AddUssdSdk<RedisSessionStore>(menu, options =>
{
    options.SessionTimeout = TimeSpan.FromMinutes(10);
});
```

## Documentation

For detailed documentation, please refer to:

- **[SDK Documentation](docs/sdk.md)** - Complete SDK reference, architecture, and API details
- **[Sample Project Documentation](docs/sample.md)** - Working examples and implementation patterns
- **[Sample Application README](sample/README.md)** - Full sample project with multiple features

## Sample Project

The repository includes a comprehensive sample application demonstrating:

- Multiple menu nodes and navigation flows
- Action handlers for business logic
- Session-based multi-step processes
- Pagination of product lists
- Voting functionality
- Money transfer flows
- Session resumption

To run the sample:

```bash
cd sample
dotnet run
```

Then send a POST request to `http://localhost:5000/ussd`:

```json
{
  "sessionID": "unique-session-id",
  "userID": "user123",
  "msisdn": "0244000000",
  "newSession": true,
  "userInput": ""
}
```

## Requirements

- .NET 8.0 or later
- ASP.NET Core

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or contributions, please visit the GitHub repository.
