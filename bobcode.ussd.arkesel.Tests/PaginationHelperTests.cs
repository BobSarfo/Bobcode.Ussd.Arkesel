using Bobcode.Ussd.Arkesel.Models;
using Bobcode.Ussd.Arkesel.Utilities;

namespace Bobcode.Ussd.Arkesel.Tests;

public class PaginationHelperTests
{
    [Fact]
    public void Paginate_ReturnsCorrectFirstPage()
    {
        // Arrange
        var items = Enumerable.Range(1, 20).ToList();

        // Act
        var result = PaginationHelper.Paginate(items, currentPage: 1, pageSize: 5);

        // Assert
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(1, result.Items[0]);
        Assert.Equal(5, result.Items[4]);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(4, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void Paginate_ReturnsCorrectMiddlePage()
    {
        // Arrange
        var items = Enumerable.Range(1, 20).ToList();

        // Act
        var result = PaginationHelper.Paginate(items, currentPage: 2, pageSize: 5);

        // Assert
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(6, result.Items[0]);
        Assert.Equal(10, result.Items[4]);
        Assert.Equal(2, result.CurrentPage);
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void Paginate_ReturnsCorrectLastPage()
    {
        // Arrange
        var items = Enumerable.Range(1, 20).ToList();

        // Act
        var result = PaginationHelper.Paginate(items, currentPage: 4, pageSize: 5);

        // Assert
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(16, result.Items[0]);
        Assert.Equal(20, result.Items[4]);
        Assert.Equal(4, result.CurrentPage);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void Paginate_HandlesPartialLastPage()
    {
        // Arrange
        var items = Enumerable.Range(1, 18).ToList();

        // Act
        var result = PaginationHelper.Paginate(items, currentPage: 4, pageSize: 5);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(16, result.Items[0]);
        Assert.Equal(18, result.Items[2]);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void Paginate_HandlesEmptyList()
    {
        // Arrange
        var items = new List<int>();

        // Act
        var result = PaginationHelper.Paginate(items, currentPage: 1, pageSize: 5);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
        Assert.Equal(0, result.TotalItems);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void Paginate_ClampsPageNumberToValidRange()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).ToList();

        // Act - page number too high
        var result1 = PaginationHelper.Paginate(items, currentPage: 100, pageSize: 5);
        
        // Act - page number too low
        var result2 = PaginationHelper.Paginate(items, currentPage: -5, pageSize: 5);

        // Assert
        Assert.Equal(2, result1.CurrentPage); // Clamped to last page
        Assert.Equal(1, result2.CurrentPage); // Clamped to first page
    }

    [Fact]
    public void CreatePaginatedMenu_AddsNavigationControls()
    {
        // Arrange
        var options = Enumerable.Range(1, 10)
            .Select(i => new MenuOption { Input = i.ToString(), Label = $"Option {i}" })
            .ToList();

        // Act
        var result = PaginationHelper.CreatePaginatedMenu(options, currentPage: 1, pageSize: 5);

        // Assert
        Assert.Equal(6, result.Count); // 5 items + next button
        Assert.Equal("#", result.Last().Input);
        Assert.Equal("Next Page", result.Last().Label);
    }

    [Fact]
    public void CreatePaginatedMenu_AddsBothNavigationControls()
    {
        // Arrange
        var options = Enumerable.Range(1, 15)
            .Select(i => new MenuOption { Input = i.ToString(), Label = $"Option {i}" })
            .ToList();

        // Act
        var result = PaginationHelper.CreatePaginatedMenu(options, currentPage: 2, pageSize: 5);

        // Assert
        Assert.Equal(7, result.Count); // 5 items + next + previous
        Assert.Contains(result, o => o.Input == "#" && o.Label == "Next Page");
        Assert.Contains(result, o => o.Input == "*" && o.Label == "Previous Page");
    }

    [Fact]
    public void CreatePaginatedMenu_NoNavigationOnSinglePage()
    {
        // Arrange
        var options = Enumerable.Range(1, 3)
            .Select(i => new MenuOption { Input = i.ToString(), Label = $"Option {i}" })
            .ToList();

        // Act
        var result = PaginationHelper.CreatePaginatedMenu(options, currentPage: 1, pageSize: 5);

        // Assert
        Assert.Equal(3, result.Count); // Only the 3 items, no navigation
    }

    [Fact]
    public void CreatePaginatedMenu_CustomNavigationCommands()
    {
        // Arrange
        var options = Enumerable.Range(1, 10)
            .Select(i => new MenuOption { Input = i.ToString(), Label = $"Option {i}" })
            .ToList();

        // Act
        var result = PaginationHelper.CreatePaginatedMenu(
            options, 
            currentPage: 1, 
            pageSize: 5,
            nextPageCommand: "99",
            previousPageCommand: "98");

        // Assert
        Assert.Contains(result, o => o.Input == "99");
    }
}

