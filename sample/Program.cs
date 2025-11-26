using Bobcode.Ussd.Arkesel.Builders;
using Bobcode.Ussd.Arkesel.Core;
using Bobcode.Ussd.Arkesel.Models;
using Bobcode.Ussd.Arkesel.Sample;
using Bobcode.Ussd.Arkesel.Sample.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Sample product data for pagination demo
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

// Build the USSD menu using strongly-typed MenuBuilder
var menu = new UssdMenuBuilder<BankMenuNode>("demo_bank_menu")
    .Root(BankMenuNode.Main)

    // Main menu
    .Page(BankMenuNode.Main, n => n
        .Title("Welcome to Demo Bank")
        .Option("1", "Check Balance").Action<BalanceCheckHandler>()
        .Option("2", "Transfer Money").GoTo(BankMenuNode.TransferRecipient)
        .Option("3", "Vote").GoTo(BankMenuNode.VoteMenu)
        .Option("4", "View Products").GoTo(BankMenuNode.Products)
    )

    // Transfer flow - Step 1: Collect recipient
    .Page(BankMenuNode.TransferRecipient, n => n
        .Title("Enter recipient phone number:")
        .Input().Action<TransferRecipientHandler>()
    )

    // Transfer flow - Step 2: Collect amount
    .Page(BankMenuNode.TransferAmount, n => n
        .Title("Enter amount to transfer:")
        .Input().Action<TransferAmountHandler>()
    )

    // Transfer flow - Step 3: Confirm transfer
    .Page(BankMenuNode.TransferConfirm, n => n
        .Title("Confirm transfer:")
        .Option("1", "Confirm").Action<TransferConfirmHandler>()
        .Option("2", "Cancel").GoTo(BankMenuNode.Main)
    )

    // Voting menu
    .Page(BankMenuNode.VoteMenu, n => n
        .Title("Vote for your candidate:")
        .Option("1", "Candidate A").Action<VotingActionHandler>()
        .Option("2", "Candidate B").Action<VotingActionHandler>()
        .Option("3", "Candidate C").Action<VotingActionHandler>()
    )

    // Products menu with built-in pagination
    .Page(BankMenuNode.Products, n => n
        .Title("Our Products:")
        .OptionList(products, p => $"{p.Name} - GHS {p.Price}", autoPaginate: true, itemsPerPage: 3)
    )

    .Build();

// Configure USSD SDK with custom options
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

// Minimal API endpoint for USSD
app.MapPost("/ussd", async (UssdRequestDto request, UssdApp ussdApp) =>
{
    var response = await ussdApp.HandleRequestAsync(request);
    return Results.Ok(response);
})
.WithName("PostUSSD")
.WithOpenApi();

app.Run();
