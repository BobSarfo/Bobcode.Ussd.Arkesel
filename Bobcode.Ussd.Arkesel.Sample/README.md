# OpenUSSD Sample Application

This sample application demonstrates how to use the OpenUSSD SDK to build a USSD application with ASP.NET Core.

## Features Demonstrated

1. **Menu Building** - Using `MenuBuilder` to create hierarchical menu structures
2. **Action Handlers** - Custom action handlers for business logic
3. **Session Management** - Storing and retrieving session data
4. **Navigation** - Back and home navigation commands
5. **Multi-step Flows** - Complex flows like money transfers
6. **Configuration** - Customizing SDK behavior with `UssdOptions`

## Project Structure

```
Bobcode.Ussd.Arkesel.Sample/
├── Handlers/
│   ├── VotingActionHandler.cs
│   ├── BalanceCheckHandler.cs
│   └── TransferHandler.cs          # TransferRecipient, TransferAmount, TransferConfirm
├── MenuNodes.cs
├── SessionKeys.cs
├── Program.cs                      # Menu, DI, Swagger, POST /ussd (minimal API)
└── README.md
```

**Session storage:** `Program.cs` uses **`AddUssdSdk(menu, …)`**, so the default is **`MemorySessionStore`** (in-memory). Redis is optional for production; see the repository **`docs/sdk.md`**.

## Getting Started

### 1. Run the Application

From the repository root:

```bash
cd Bobcode.Ussd.Arkesel.Sample
dotnet run
```

URLs and ports come from **`Properties/launchSettings.json`** (for example **http://localhost:5108** and **https://localhost:7123** for the `https` profile).

### 2. Test with Swagger

Open **`/swagger`** on the same base URL (for example `https://localhost:7123/swagger` or `http://localhost:5108/swagger`) and try **POST `/ussd`**.

### 3. Sample Request

Send a POST request to `/ussd` with the following JSON:

**New Session (Root Menu):**
```json
{
  "sessionID": "test-session-123",
  "userID": "USSD_DEMO",
  "msisdn": "233271234567",
  "userData": "",
  "newSession": true,
  "network": "MTN"
}
```

**Response:**
```json
{
  "sessionID": "test-session-123",
  "userID": "USSD_DEMO",
  "msisdn": "233271234567",
  "message": "Welcome to Demo Bank\n1. Check Balance\n2. Transfer Money\n3. Vote\n4. View Products",
  "continueSession": true
}
```

**User Selection (Check Balance):**
```json
{
  "sessionID": "test-session-123",
  "userID": "USSD_DEMO",
  "msisdn": "233271234567",
  "userData": "1",
  "newSession": false,
  "network": "MTN"
}
```

**Response:**
```json
{
  "sessionID": "test-session-123",
  "userID": "USSD_DEMO",
  "msisdn": "233271234567",
  "message": "Your current balance is: GHS 150.50\nThank you for using our service.",
  "continueSession": false
}
```

## Menu Structure

```
Main Menu
├── 1. Check Balance → BalanceCheckHandler
├── 2. Transfer Money → Multi-step flow
│   ├── Enter recipient
│   ├── Enter amount
│   └── Confirm transfer
├── 3. Vote → Voting menu
│   ├── 1. Candidate A
│   ├── 2. Candidate B
│   └── 3. Candidate C
└── 4. View Products → Products list
```

## Navigation Commands

- **Back**: Enter `0` to go back to the previous menu
- **Home**: Enter `#` to return to the main menu

## Action Handlers

### VotingActionHandler

Simple action handler that processes a vote and stores it in the session.

```csharp
public class VotingActionHandler : BaseActionHandler
{
    public override string Key => "vote";

    public override async Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var candidate = context.Request.UserData;
        SetSessionData(context, "vote", candidate);
        return End($"Thank you for voting for candidate {candidate}!");
    }
}
```

### BalanceCheckHandler

Demonstrates fetching data and ending the session.

```csharp
public class BalanceCheckHandler : BaseActionHandler
{
    public override string Key => "check_balance";

    public override async Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var balance = 150.50m; // In real app, fetch from database
        return End($"Your current balance is: GHS {balance:F2}");
    }
}
```

### TransferHandler

Complex multi-step flow demonstrating session state management.

```csharp
[UssdAction]
public class TransferRecipientHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var input = context.Request.UserData;

        if (string.IsNullOrWhiteSpace(input) || input.Length < 10)
        {
            return Task.FromResult(Continue("Invalid phone number. Please enter a valid phone number:"));
        }

        Set(context, SessionKeys.Recipient, input);

        return Task.FromResult(GoTo(BankMenuPage.TransferAmount));
    }
}

```

## Configuration

The SDK is configured in `Program.cs`:

```csharp
builder.Services.AddUssdSdk(menu, options =>
{
    options.SessionTimeout = TimeSpan.FromMinutes(5);
    options.BackCommand = "0";
    options.HomeCommand = "#";
    options.EnablePagination = true;
    options.ItemsPerPage = 5;
    options.InvalidInputMessage = "Invalid input. Please try again.";
});
```


## Registering Action Handlers

Register your action handlers in `Program.cs`:

```csharp
builder.Services.AddActionHandler<VotingActionHandler>();
builder.Services.AddActionHandler<BalanceCheckHandler>();
builder.Services.AddActionHandler<TransferHandler>();
```

## Session Management

The SDK automatically manages sessions. You can store and retrieve data:

```csharp
// Store data
SetSessionData(context, "key", value);

// Retrieve data
var value = GetSessionData<T>(context, "key");
```

## Next Steps

1. Customize the menu structure for your use case
2. Implement your own action handlers
3. Integrate with your database or external APIs
4. Configure session storage (Redis for production)
5. Add logging and error handling
6. Deploy to your USSD gateway

## Production Considerations

For production use:

1. **Use Redis for session storage:**
   ```csharp
   builder.Services.AddUssdSdk<RedisSessionStore>(menu, options => { ... });
   ```

2. **Add proper error handling and logging**
3. **Implement authentication/authorization if needed**
4. **Configure HTTPS and security headers**
5. **Set up monitoring and alerting**
6. **Test thoroughly with your USSD gateway**

## Learn More

- [OpenUSSD Documentation](../README.md)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)

