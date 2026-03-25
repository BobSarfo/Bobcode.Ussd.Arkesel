# Sample Project Documentation

# Sample USSD Application

A reference implementation demonstrating:

- Multiple menu nodes and navigation flows
- Strongly-typed action handlers
- Session management with type-safe keys
- Pagination for long lists
- Multi-step flows (money transfer)
- Session resumption
- Back and home navigation
- Voting functionality

**Session store:** the sample calls **`AddUssdSdk(menu, …)`**, which uses the default **`MemorySessionStore`** (in-memory). **Redis is not used** unless you switch to **`AddUssdSdk<RedisSessionStore>(…)`**; see [sdk.md](sdk.md).

---

# **1. Project Structure**

```
Bobcode.Ussd.Arkesel.Sample/
├── Handlers/
│   ├── BalanceCheckHandler.cs
│   ├── TransferHandler.cs   (TransferRecipient, TransferAmount, TransferConfirm)
│   └── VotingActionHandler.cs
├── MenuNodes.cs             (BankMenuPage enum)
├── SessionKeys.cs
├── Program.cs               (menu, DI, minimal API POST /ussd)
└── Sample.http
```

---

# **2. Menu Implementation**

The sample builds one menu in `Program.cs` with **`UssdMenuBuilder<BankMenuPage>`**. Each screen is a **`.Page`**; user-facing text uses **`.Title`** (not `.Message`).

```csharp
var menu = new UssdMenuBuilder<BankMenuPage>("demo_bank_menu")
    .Root(BankMenuPage.Main)

    .Page(BankMenuPage.Main, n => n
        .Title("Welcome to Demo Bank")
        .Option("1", "Check Balance").Action<BalanceCheckHandler>()
        .Option("2", "Transfer Money").GoTo(BankMenuPage.TransferRecipient)
        .Option("3", "Vote").GoTo(BankMenuPage.VoteMenu)
        .Option("4", "View Products").GoTo(BankMenuPage.Products)
    )

    .Page(BankMenuPage.TransferRecipient, n => n
        .Title("Enter recipient phone number:")
        .Input().Action<TransferRecipientHandler>()
    )

    .Page(BankMenuPage.TransferAmount, n => n
        .Title("Enter amount to transfer:")
        .Input().Action<TransferAmountHandler>()
    )

    .Page(BankMenuPage.TransferConfirm, n => n
        .Title("Confirm transfer:")
        .Option("1", "Confirm").Action<TransferConfirmHandler>()
        .Option("2", "Cancel").GoTo(BankMenuPage.Main)
    )

    .Page(BankMenuPage.VoteMenu, n => n
        .Title("Vote for your candidate:")
        .Option("1", "Candidate A").Action<VotingActionHandler>()
        .Option("2", "Candidate B").Action<VotingActionHandler>()
        .Option("3", "Candidate C").Action<VotingActionHandler>()
    )

    .Page(BankMenuPage.Products, n => n
        .Title("Our Products:")
        .OptionList(products, p => $"{p.Name} - GHS {p.Price}", autoPaginate: true, itemsPerPage: 3)
    )

    .Build();
```

---

# **3. Menu Pages Enum**

The menu uses an enum for type-safe navigation:

```csharp
public enum BankMenuPage
{
    Main,
    TransferRecipient,
    TransferAmount,
    TransferConfirm,
    VoteMenu,
    Products
}
```

---

# **4. Action Handlers**

### Balance Check Handler

```csharp
[UssdAction]
public class BalanceCheckHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var balance = 1500.00m; // In real app, fetch from database

        return Task.FromResult(End($"Your balance is GHS {balance:N2}\nThank you."));
    }
}
```

### Transfer Handlers (Multi-Step Flow)

The transfer flow is split into three handlers:

**Step 1: Collect Recipient**

```csharp
[UssdAction]
public class TransferRecipientHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var input = context.Request.UserData;

        if (string.IsNullOrWhiteSpace(input) || input.Length < 10)
        {
            return Task.FromResult(Continue("Invalid phone number. Please try again:"));
        }

        Set(context, SessionKeys.Recipient, input);
        return Task.FromResult(GoTo(BankMenuPage.TransferAmount));
    }
}
```

**Step 2: Collect Amount**

```csharp
[UssdAction]
public class TransferAmountHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var input = context.Request.UserData;

        if (!decimal.TryParse(input, out var amount) || amount <= 0)
        {
            return Task.FromResult(Continue("Invalid amount. Please try again:"));
        }

        Set(context, SessionKeys.Amount, amount);

        var recipient = Get(context, SessionKeys.Recipient);
        var message = $"Confirm transfer:\nTo: {recipient}\nAmount: GHS {amount:F2}";

        return Task.FromResult(Continue(message, BankMenuPage.TransferConfirm.ToPageId()));
    }
}
```

**Step 3: Confirm and Process**

```csharp
[UssdAction]
public class TransferConfirmHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var input = context.Request.UserData;

        if (input == "1")
        {
            var recipient = Get(context, SessionKeys.Recipient);
            var amount = Get(context, SessionKeys.Amount);

            // Process transfer here

            Remove(context, SessionKeys.Recipient);
            Remove(context, SessionKeys.Amount);

            return Task.FromResult(End($"Transfer of GHS {amount:F2} to {recipient} successful!"));
        }
        else
        {
            Remove(context, SessionKeys.Recipient);
            Remove(context, SessionKeys.Amount);
            return Task.FromResult(GoHome());
        }
    }
}
```

### Voting Handler

```csharp
[UssdAction]
public class VotingActionHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var choice = context.Request.UserData;

        Set(context, SessionKeys.VoteChoice, choice);

        var candidateName = choice switch
        {
            "1" => "Candidate A",
            "2" => "Candidate B",
            "3" => "Candidate C",
            _ => "Unknown"
        };

        return Task.FromResult(End($"Thank you for voting for {candidateName}!"));
    }
}
```

---

# **5. Session Keys**

Strongly-typed session keys for type-safe data storage:

```csharp
public static class SessionKeys
{
    public static SessionKey<string?> Recipient => new("recipient");
    public static SessionKey<decimal?> Amount => new("amount");
    public static SessionKey<string?> VoteChoice => new("vote_choice");
}
```

---

# **6. Pagination Example**

The sample includes a products menu with built-in pagination:

```csharp
var products = new[]
{
    new { Name = "Product A", Price = 10m },
    new { Name = "Product B", Price = 20m },
    new { Name = "Product C", Price = 30m },
    new { Name = "Product D", Price = 40m },
    new { Name = "Product E", Price = 50m },
    new { Name = "Product F", Price = 60m },
    new { Name = "Product G", Price = 70m },
};

.Page(BankMenuPage.Products, n => n
    .Title("Our Products:")
    .OptionList(products,
        p => $"{p.Name} - GHS {p.Price}",
        autoPaginate: true,
        itemsPerPage: 3)
)
```

Users can navigate with:
- 98 = Previous page
- 99 = Next page

---

# **7. Session Resumption Feature**

The SDK automatically detects interrupted sessions and asks users if they want to resume:

```
You have an active session.
1. Resume
2. Start Again
```

Enabled in configuration:

```csharp
options.EnableSessionResumption = true;
options.ResumeSessionPrompt = "You have an active session.";
options.ResumeOptionLabel = "Resume";
options.StartFreshOptionLabel = "Start Again";
```

---

# **8. Configuration**

The SDK is configured in `Program.cs` with inline options:

```csharp
builder.Services.AddUssdSdk(menu, options =>
{
    options.SessionTimeout = TimeSpan.FromMinutes(5);
    options.BackCommand = "0";
    options.HomeCommand = "#";
    options.EnablePagination = true;
    options.ItemsPerPage = 5;
    options.InvalidInputMessage = "Invalid input. Please try again.";
    options.DefaultEndMessage = "Thank you for using our service.";
    options.EnableAutoBackNavigation = true;
    options.EnableSessionResumption = true;
    options.ResumeSessionPrompt = "You have an active session.";
    options.ResumeOptionLabel = "Resume";
    options.StartFreshOptionLabel = "Start Again";
});

builder.Services.AddUssdActionsFromAssembly(typeof(Program).Assembly);
```

This registration uses the **default in-memory** session store (`MemorySessionStore`). It is fine for local runs; use **Redis** only when you need shared sessions across instances (see [sdk.md](sdk.md)).

---

# **9. Running the Sample**

```bash
cd Bobcode.Ussd.Arkesel.Sample
dotnet run
```

The dev server URL is defined in `Properties/launchSettings.json` (for example **http://localhost:5108**). Swagger is opened automatically for the `http` / `https` profiles.

---

# **10. Testing the Sample**

### Using HTTP Client

Send **POST** `{baseUrl}/ussd` (replace `baseUrl` with your run URL, e.g. `http://localhost:5108`):

```json
{
  "sessionID": "unique-session-id",
  "userID": "user123",
  "msisdn": "0244000000",
  "userData": "",
  "newSession": true,
  "network": "MTN"
}
```

### Using the Included Sample.http File

The sample includes `Sample.http`. Open it in Visual Studio Code with the REST Client extension (adjust the host if your port differs).

### Example Flow

1. **Initial request** (`newSession`: true, `userData`: "")
   - Response: "Welcome to Demo Bank\n1. Check Balance\n2. Transfer Money\n3. Vote\n4. View Products"

2. **Select transfer** (`newSession`: false, `userData`: "2")
   - Response: "Enter recipient phone number:"

3. **Enter phone** (`newSession`: false, `userData`: "0244123456")
   - Response: "Enter amount to transfer:"

4. **Enter amount** (`newSession`: false, `userData`: "100")
   - Response: "Confirm transfer:\nTo: 0244123456\nAmount: GHS 100.00\n\n1. Confirm\n2. Cancel"

5. **Confirm** (`newSession`: false, `userData`: "1")
   - Response: "Transfer of GHS 100.00 to 0244123456 successful!\nThank you."

---

# **11. Features Demonstrated**

The sample application demonstrates:

1. **Menu Navigation** - Enum-based navigation between nodes
2. **Action Handlers** - Business logic execution
3. **Session Management** - Storing and retrieving data across requests
4. **Multi-Step Flows** - Complex flows with validation
5. **Pagination** - Automatic pagination for long lists
6. **Voting** - Simple voting functionality
7. **Session Resumption** - Resume interrupted sessions
8. **Back Navigation** - Press "0" to go back
9. **Home Navigation** - Press "#" to return to main menu
10. **Input Validation** - Validate user input in handlers

---

# **12. Extending the Sample**

To add new functionality:

1. **Add new menu pages** to the `BankMenuPage` enum
2. **Create new handlers** in the `Handlers/` folder with `[UssdAction]` attribute
3. **Add new nodes** to the menu builder in `Program.cs`
4. **Define session keys** in `SessionKeys.cs` if needed
5. **Run the application** - handlers are auto-discovered

Example:

```csharp
// 1. Add to enum
public enum BankMenuPage
{
    Main,
    // ... existing pages
    BuyAirtime
}

// 2. Create handler
[UssdAction]
public class BuyAirtimeHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        return Task.FromResult(End("Airtime purchased successfully!"));
    }
}

// 3. Add to menu
.Page(BankMenuPage.Main, n => n
    .Title("Welcome to Demo Bank")
    // ... existing options
    .Option("5", "Buy Airtime").Action<BuyAirtimeHandler>()
)
```

---

# **13. Best Practices Demonstrated**

1. **Enum-Based Navigation** - Type-safe menu navigation
2. **Strongly-Typed Session Keys** - Avoid magic strings
3. **Input Validation** - Validate all user input
4. **Session Cleanup** - Remove session data when done
5. **BaseActionHandler** - Use helper methods for cleaner code
6. **Multi-Step Flows** - Break complex flows into steps
7. **Error Messages** - Provide clear feedback to users