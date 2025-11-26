using OpenUSSD.Core;
using OpenUSSD.Builders;
using OpenUSSD.models;
using Sample;
using Sample.Handlers;

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
    .Node(BankMenuNode.Main, n => n
        .Message("Welcome to Demo Bank")
        .Option("1", "Check Balance").Action<BalanceCheckHandler>()
        .Option("2", "Transfer Money").GoTo(BankMenuNode.TransferRecipient)
        .Option("3", "Vote").GoTo(BankMenuNode.VoteMenu)
        .Option("4", "View Products").GoTo(BankMenuNode.Products)
    )

    // Transfer flow
    .Node(BankMenuNode.TransferRecipient, n => n
        .Message("Enter recipient phone number:")
        .Input().Action<TransferHandler>()
    )

    // Voting menu
    .Node(BankMenuNode.VoteMenu, n => n
        .Message("Vote for your candidate:")
        .Option("1", "Candidate A").Action<VotingActionHandler>()
        .Option("2", "Candidate B").Action<VotingActionHandler>()
        .Option("3", "Candidate C").Action<VotingActionHandler>()
    )

    // Products menu with built-in pagination
    .Node(BankMenuNode.Products, n => n
        .Message("Our Products:")
        .OptionList(products, p => $"{p.Name} - GHS {p.Price}", autoPaginate: true, itemsPerPage: 3)
    )

    .Build();

// Configure USSD SDK with custom options
var ussdOptions = new UssdOptions
{
    SessionTimeout = TimeSpan.FromMinutes(5),
    BackCommand = "0",
    HomeCommand = "#",
    EnablePagination = true,
    ItemsPerPage = 5,
    InvalidInputMessage = "Invalid input. Please try again.",
    DefaultEndMessage = "Thank you for using our service.",
    EnableAutoBackNavigation = true
};

builder.Services.AddUssdSdk(menu, ussdOptions);

// Auto-discover and register all action handlers from this assembly
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
