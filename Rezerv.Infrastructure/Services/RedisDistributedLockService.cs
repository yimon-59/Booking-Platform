using StackExchange.Redis;
using Rezerv.Application.Interfaces;

namespace Rezerv.Infrastructure.Services;

public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedLockService(
        IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<IAsyncDisposable?> AcquireAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var database = _redis.GetDatabase();

        var lockValue = Guid.NewGuid().ToString();

        var acquired = await database.StringSetAsync(
            key,
            lockValue,
            expiry,
            When.NotExists);

        if (!acquired)
        {
            return null;
        }

        return new RedisLockHandle(
            database,
            key,
            lockValue);
    }

    private sealed class RedisLockHandle : IAsyncDisposable
    {
        private readonly IDatabase _database;
        private readonly string _key;
        private readonly string _value;

        public RedisLockHandle(
            IDatabase database,
            string key,
            string value)
        {
            _database = database;
            _key = key;
            _value = value;
        }

        public async ValueTask DisposeAsync()
        {
            const string script = """
                if redis.call("GET", KEYS[1]) == ARGV[1] then
                    return redis.call("DEL", KEYS[1])
                end
                return 0
                """;

            await _database.ScriptEvaluateAsync(
                script,
                new RedisKey[] { _key },
                new RedisValue[] { _value });
        }
    }
}