using ForeverProjects.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TestLock.Data;

namespace TestLock.Controllers
{
    public class OrdersController3: ControllerBase
    {
        // 方法3：在数据库层面实现唯一约束
        // 分析：
        //      最简单可靠的方法，直接在数据库层面实现唯一约束，避免重复订单的产生。
        //      CREATE UNIQUE INDEX IX_Orders_UserId

    }
}
