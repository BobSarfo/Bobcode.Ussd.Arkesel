using Bobcode.Ussd.Arkesel.Actions;

namespace Bobcode.Ussd.Arkesel.Tests;

public class MenuBuilderTests
{
    [Fact]
    public void Build_WithValidMenu_ReturnsMenu()
    {
        // Arrange
        var builder = new MenuBuilder("test-menu");

        // Act
        var menu = builder
            .AddPage("main", "Welcome")
            .AddOption("main", "1", "Option 1", "step1")
            .AddPage("step1", "Step 1")
            .Build();

        // Assert
        Assert.NotNull(menu);
        Assert.Equal("test-menu", menu.Id);
        Assert.Equal(2, menu.Pages.Count);
    }

    [Fact]
    public void AddPage_CreatesPageWithCorrectProperties()
    {
        // Arrange
        var builder = new MenuBuilder("test-menu");

        // Act
        var menu = builder
            .AddPage("main", "Welcome Message", isTerminal: false)
            .Build();

        // Assert
        var page = menu.GetPage("main");
        Assert.Equal("main", page.Id);
        Assert.Equal("Welcome Message", page.Title);
        Assert.False(page.IsTerminal);
    }

    [Fact]
    public void AddOption_AddsOptionToPage()
    {
        // Arrange
        var builder = new MenuBuilder("test-menu");

        // Act
        var menu = builder
            .AddPage("main", "Main Menu")
            .AddOption("main", "1", "First Option", "step1")
            .AddOption("main", "2", "Second Option", actionKey: "action1")
            .AddPage("step1", "Step 1")
            .Build();

        // Assert
        var page = menu.GetPage("main");
        Assert.Equal(2, page.Options.Count);
        Assert.Equal("1", page.Options[0].Input);
        Assert.Equal("First Option", page.Options[0].Label);
        Assert.Equal("step1", page.Options[0].TargetStep);
        Assert.Equal("action1", page.Options[1].ActionKey);
    }

    [Fact]
    public void AddOption_ThrowsException_WhenPageDoesNotExist()
    {
        // Arrange
        var builder = new MenuBuilder("test-menu");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            builder.AddOption("nonexistent", "1", "Option"));
    }

    [Fact]
    public void SetRoot_SetsRootId()
    {
        // Arrange
        var builder = new MenuBuilder("test-menu");

        // Act
        var menu = builder
            .AddPage("custom-root", "Custom Root")
            .SetRoot("custom-root")
            .Build();

        // Assert
        Assert.Equal("custom-root", menu.RootId);
    }

    [Fact]
    public void Build_ThrowsException_WhenRootPageDoesNotExist()
    {
        // Arrange
        var builder = new MenuBuilder("test-menu")
            .SetRoot("nonexistent");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void AddOptions_AddsMultipleOptions()
    {
        // Arrange
        var builder = new MenuBuilder("test-menu");

        // Act
        var menu = builder
            .AddPage("main", "Main Menu")
            .AddOptions("main",
                ("1", "Option 1", "step1", null),
                ("2", "Option 2", "step2", null),
                ("3", "Option 3", null, "action1"))
            .AddPage("step1", "Step 1")
            .AddPage("step2", "Step 2")
            .Build();

        // Assert
        var page = menu.GetPage("main");
        Assert.Equal(3, page.Options.Count);
    }

    [Fact]
    public void Build_CreatesComplexMenuStructure()
    {
        // Arrange & Act
        var menu = new MenuBuilder("voting-app")
            .AddPage("main", "Welcome to Voting App")
            .AddOption("main", "1", "Vote for Product A", actionKey: "vote_a")
            .AddOption("main", "2", "Vote for Product B", actionKey: "vote_b")
            .AddOption("main", "3", "View Results", "results")
            .AddPage("results", "Results will be displayed here", isTerminal: true)
            .Build();

        // Assert
        Assert.Equal(2, menu.Pages.Count);
        Assert.True(menu.GetPage("results").IsTerminal);
        Assert.False(menu.GetPage("main").IsTerminal);
    }
}

