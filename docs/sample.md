# Sample Project Documentation 

# Sample USSD Application

A reference implementation demonstrating:

✔ Multiple menus
✔ Strongly-typed handlers
✔ Session management
✔ Pagination
✔ Multi-step flows
✔ Continue session
✔ Back + Home navigation

---

# **1. Project Structure**

```
sample/
│── Menus/
│   ├── BankMenu.cs
│   ├── AirtimeMenu.cs
│   └── UtilitiesMenu.cs
│── Handlers/
│   ├── BalanceCheckHandler.cs
│   ├── TransferHandler.cs
│   └── BuyAirtimeHandler.cs
│── MenuNodes/
│── SessionKeys/
│── Program.cs
```

---

# **2. Menu Implementation**

### Example menu: Bank

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
            .Node(BankMenuNode.Transfer, n => n
                .Message("Enter Recipient")
                .Input().Action<RecipientHandler>()
            )
            .Build();
    }
}
```

---

# **3. Action Handlers**

```csharp
[UssdAction]
public class TransferHandler : BaseActionHandler
{
    public override Task<UssdStepResult> HandleAsync(UssdContext context)
    {
        var amount = Get(context, SessionKeys.Amount);
        var recipient = Get(context, SessionKeys.Recipient);

        return Task.FromResult(
            End($"Sent {amount} to {recipient}")
        );
    }
}
```

---

# **4. Pagination Example**

```csharp
.OptionList(accounts, a => a.DisplayName, a => a.Id)
```

Supports:

* 98 = previous
* 99 = next

Automatically configurable.

---

# **5. Session-Based Flow Example**

```csharp
.Set(context, SessionKeys.Recipient, context.Input)
.GoTo(BankMenuNode.Amount);
```

---

# **6. Continue Session / Resume Feature**

SDK automatically asks:

```
You have an active session:
1. Resume
2. Start Fresh
```

Enabled by:

```csharp
options.EnableSessionResumption = true;
```

---

# **7. End-to-End Example**

Input → Action → Next Node → Persist Session → Output

The sample solution includes:

* Check balance
* Money transfer
* Airtime purchase
* Utility payments
* Pagination menus
* Multi-step input capture

---

# **8. Testing**

Use `UssdApp.HandleRequestAsync()` with mocked request DTOs.

---

# **9. Running the Sample**

```bash
cd sample
dotnet run
```

Then POST:

```json
{
  "msisdn": "0244000000",
  "newSession": true,
  "sessionId": "xyz"
}
```

---

# **10. Extending the Sample**

Add new menu:

1. Add enum → `NewMenuNode.cs`
2. Add menu → `NewMenu.cs`
3. Add handlers in `/Handlers`
4. Done (auto-discovered)

---

# ✅ COMPLETE

Your documentation is now:

✔ Professional
✔ Structured
✔ Complete
✔ Ready for production repos

