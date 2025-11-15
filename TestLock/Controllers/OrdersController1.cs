using ForeverProjects.Data;
using ForeverProjects.models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace TestLock.Controllers
{

    // 实现每个用户只能下一单（不限制商品数量）
    

    // 方法1：单机锁（内存锁）
    // 分析：
    //      多实例/负载均衡/容器化部署：内存锁完全失效。因为多实例有各自的内存空间，其内存锁各自独立
    [Route("api/[controller]")]
    public class OrdersController1: ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        // 为了实现一个用户只能下一单，我们使用一个静态的并发字典来存储每个用户的锁
        // 使用字典存储每个用户的锁；
        // 使用静态是因为：每次请求都会创建一个 OrdersController 实例，每个实例如果有自己的字典，就无法实现跨实例的锁定效果。
        // 使用 ConcurrentDictionary 是为了普通字典是线程不安全的，即使多个实例使用同一个字典，但是并发执行时并不安全，而并发字典是线程安全的。
        // 使用 SemaphoreSlim 是为了实现信号量机制，限制一个用户的多次点击。每个用户对应一个信号量，信号量的初始计数为 1，表示同一时间只能有 "一个用户的一次操作" 可以访问该用户的订单。
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks = new();
        public OrdersController1(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost("Order")]
        public async Task<IActionResult> CreateOrder([FromQuery] string userId)
        {
            // 获取或创建用户的锁
            // GetOrAdd 是原子的，这解决了“先查再插”可能引发的竞态问题（两个线程同时发现 key 不存在，都去创建，导致重复或覆盖）。
            SemaphoreSlim userLock = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
            await userLock.WaitAsync(); // 等待获取锁
            try
            {
                // 模拟订单创建逻辑
                // 检查用户是否已有未完成的订单
                var existingOrder = _appDbContext.Orders.FirstOrDefault(o => o.UserId.ToString() == userId && o.Status != Status.Completed);
                if (existingOrder != null)
                {
                    return BadRequest("User already has an active order.");
                }
                // 创建新订单
                var newOrder = new Order
                {
                    UserId = int.Parse(userId),
                    Status = Status.Pending
                };
                _appDbContext.Orders.Add(newOrder);
                await _appDbContext.SaveChangesAsync();
                return Ok(new { OrderId = newOrder.Id });
            }
            catch (Exception ex)
            {
                return BadRequest("请求失败" );
            }
            finally
            {
                userLock.Release(); // 释放锁
            }
        }
    }


}
