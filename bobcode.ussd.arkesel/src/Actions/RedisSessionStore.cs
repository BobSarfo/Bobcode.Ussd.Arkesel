using System.Text.Json;
using Bobcode.Ussd.Arkesel.Models;
using StackExchange.Redis;

namespace Bobcode.Ussd.Arkesel.Actions;

public class RedisSessionStore : IUssdSessionStore
{
    private readonly IDatabase _db;
    private readonly string _prefix = "ussd:sess:";


    public RedisSessionStore(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }


    public async Task<UssdSession?> GetAsync(string sessionId, CancellationToken ct = default)
    {
        var bytes = await _db.StringGetAsync(_prefix + sessionId);
        if (bytes.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<UssdSession>(bytes!);
    }


    public Task SetAsync(UssdSession session, TimeSpan ttl, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.Serialize(session);
        return _db.StringSetAsync(_prefix + session.SessionId, bytes, ttl);
    }


    public Task RemoveAsync(string sessionId, CancellationToken ct = default)
    {
        return _db.KeyDeleteAsync(_prefix + sessionId);
    }
}