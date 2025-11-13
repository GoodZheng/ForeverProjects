namespace TestLock.Data
{
    public interface IRedisService
    {
        // 字符串取值
        Task<string?> GetStringAsync(string key);

        // 字符串存值
        Task SetStringAsync(string key, string value, TimeSpan? expiry = null);


        // 分布式锁：尝试获取锁
        Task<bool> AcquireLockAsync(string key, string value, TimeSpan expiry);

        // 分布式锁：释放锁
        Task<bool> ReleaseLockAsync(string key, string value);
    }
}
