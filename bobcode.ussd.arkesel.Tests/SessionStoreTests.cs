using Microsoft.Extensions.Caching.Memory;
using Bobcode.Ussd.Arkesel.Actions;
using Bobcode.Ussd.Arkesel.Models;

namespace bobcode.ussd.arkesel.Tests;

public class MemorySessionStoreTests
{
    private readonly IMemoryCache _cache;
    private readonly MemorySessionStore _store;

    public MemorySessionStoreTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _store = new MemorySessionStore(_cache);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenSessionDoesNotExist()
    {
        // Act
        var session = await _store.GetAsync("nonexistent");

        // Assert
        Assert.Null(session);
    }

    [Fact]
    public async Task SetAsync_StoresSession()
    {
        // Arrange
        var session = new UssdSession("session1", "233123456789", "user1", "MTN");

        // Act
        await _store.SetAsync(session, TimeSpan.FromMinutes(5));
        var retrieved = await _store.GetAsync("session1");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("session1", retrieved.SessionId);
        Assert.Equal("233123456789", retrieved.Msisdn);
        Assert.Equal("user1", retrieved.UserId);
        Assert.Equal("MTN", retrieved.Network);
    }

    [Fact]
    public async Task RemoveAsync_RemovesSession()
    {
        // Arrange
        var session = new UssdSession("session1", "233123456789", "user1", "MTN");
        await _store.SetAsync(session, TimeSpan.FromMinutes(5));

        // Act
        await _store.RemoveAsync("session1");
        var retrieved = await _store.GetAsync("session1");

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task SetAsync_UpdatesExistingSession()
    {
        // Arrange
        var session = new UssdSession("session1", "233123456789", "user1", "MTN");
        await _store.SetAsync(session, TimeSpan.FromMinutes(5));

        // Act
        session.CurrentStep = "step2";
        session.Level = 3;
        await _store.SetAsync(session, TimeSpan.FromMinutes(5));
        var retrieved = await _store.GetAsync("session1");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("step2", retrieved.CurrentStep);
        Assert.Equal(3, retrieved.Level);
    }

    [Fact]
    public async Task Session_SupportsDataDictionary()
    {
        // Arrange
        var session = new UssdSession("session1", "233123456789", "user1", "MTN");
        session.Data["userName"] = "John Doe";
        session.Data["age"] = 25;

        // Act
        await _store.SetAsync(session, TimeSpan.FromMinutes(5));
        var retrieved = await _store.GetAsync("session1");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("John Doe", retrieved.Data["userName"]);
        Assert.Equal(25, retrieved.Data["age"]);
    }
}

