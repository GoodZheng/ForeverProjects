
using StackExchange.Redis;

namespace TestLock.Data
{
    public class RedisService : IRedisService
    {
        // IDatabase 依赖于 IConnectionMultiplexer ，其生命周期由 IConnectionMultiplexer 管理。不用注册
        private readonly IDatabase _redis; 
        public RedisService(IConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer.GetDatabase();
        }

        // 分布式锁：尝试获取锁
        public Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiry)
        {
            return _redis.StringSetAsync(key, value, expiry, When.NotExists);
        }
        // 分布式锁：释放锁
        public async Task<bool> ReleaseLockAsync(string key, string value)
        {
            var res = await _redis.ScriptEvaluateAsync(
                // Lua 脚本：如果 key 的值等于 value，则删除 key
                @"if redis.call('get', KEYS[1]) == ARGV[1] then 
                    return redis.call('del', KEYS[1]) 
                  else 
                    return 0  
                  end",
                new RedisKey[] { key },
                new RedisValue[] { value });
            return (int)res == 1;
        }


        // 字符串取值
        public async Task<string?> GetStringAsync(string key)
        {
            return await _redis.StringGetAsync(key);
        }
        // 字符串存值
        public Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            return _redis.StringSetAsync(key, value, expiry);
        }
    }
}
