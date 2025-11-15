using ForeverProjects.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TestLock.Data;

namespace TestLock.Controllers
{
    public class OrdersController2: ControllerBase
    {
        // 方法2：分布式锁（Redis 分布式锁）
        private readonly IRedisService _redis;
        private readonly AppDbContext _appDbContext;
        public OrdersController2(IRedisService redisService, AppDbContext appDbContext)
        {
            _redis = redisService;
            _appDbContext = appDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromQuery] string userId)
        {
            var lockKey = $"order_lock:{userId}";
            var lockValue = Guid.NewGuid().ToString(); // 防止误删他人锁
            var lockExpiry = TimeSpan.FromSeconds(10); // 锁过期时间，防止死锁
            // 尝试获取分布式锁
            var acquired = await _redis.AcquireLockAsync(lockKey, lockValue, lockExpiry);
            if (!acquired)
            {
                return BadRequest("请求太频繁，请稍后重试");
            }
            try
            {
                bool orderExists = await _appDbContext.Orders.AnyAsync(o => o.UserId.ToString() == userId );
                if (orderExists)
                {
                    return BadRequest("用户订单已经存在");
                }
                var newOrder = new ForeverProjects.models.Order
                {
                    UserId = int.Parse(userId),
                    Status = ForeverProjects.models.Status.Pending
                };
                _appDbContext.Orders.Add(newOrder);
                await _appDbContext.SaveChangesAsync();
                return Ok("订单创建成功");
            }
            finally
            {
                // 释放分布式锁
                await _redis.ReleaseLockAsync(lockKey, lockValue);
            }
        }
    }
}
