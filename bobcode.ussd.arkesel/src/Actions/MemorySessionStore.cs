using Bobcode.Ussd.Arkesel.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Bobcode.Ussd.Arkesel.Actions;



public class MemorySessionStore : IUssdSessionStore
{
    private readonly IMemoryCache _cache;
    public MemorySessionStore(IMemoryCache cache) => _cache = cache;


    public Task<UssdSession?> GetAsync(string sessionId, CancellationToken ct = default)
    {
        _cache.TryGetValue(sessionId, out UssdSession? session);
        return Task.FromResult(session);
    }


    public Task SetAsync(UssdSession session, TimeSpan ttl, CancellationToken ct = default)
    {
        _cache.Set(session.SessionId, session, ttl);
        return Task.CompletedTask;
    }


    public Task RemoveAsync(string sessionId, CancellationToken ct = default)
    {
        _cache.Remove(sessionId);
        return Task.CompletedTask;
    }
}