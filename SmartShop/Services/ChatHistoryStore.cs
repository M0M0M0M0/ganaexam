using Microsoft.Extensions.Caching.Memory;
using SmartShop.Services.Gemini;

namespace SmartShop.Services;

public class ChatHistoryStore(IMemoryCache cache)
{
    public const int MaxMessages = 20;

    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(30);

    private sealed class Session
    {
        public readonly List<ChatMessage> Messages = [];
        public readonly SemaphoreSlim Lock = new(1, 1);
    }

    private Session GetSession(string sessionId) =>
        cache.GetOrCreate(Key(sessionId), entry =>
        {
            entry.SlidingExpiration = Expiration;
            return new Session();
        })!;

    public async Task<T> WithSessionAsync<T>(string sessionId, Func<List<ChatMessage>, Task<T>> action, CancellationToken ct)
    {
        var session = GetSession(sessionId);
        await session.Lock.WaitAsync(ct);
        try
        {
            var result = await action(session.Messages);
            if (session.Messages.Count > MaxMessages)
                session.Messages.RemoveRange(0, session.Messages.Count - MaxMessages);
            return result;
        }
        finally
        {
            session.Lock.Release();
        }
    }

    public IReadOnlyList<ChatMessage> GetHistory(string sessionId) =>
        cache.TryGetValue(Key(sessionId), out Session? session) ? session!.Messages.ToList() : [];

    public void Clear(string sessionId) => cache.Remove(Key(sessionId));

    private static string Key(string sessionId) => $"chat:{sessionId.Trim()}";
}
