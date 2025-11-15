using ForeverProjects.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TestLock.Data;

var builder = WebApplication.CreateBuilder(args);

// 配置 DbContext 和 MySQL 连接
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("DB:MySqlConn")
             ?? throw new InvalidOperationException("Connection string 'MySqlConn' not found.");
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr)); //自动检测数据库版本
});

// 配置 redis 缓存：使用 IDistributedCache 接口抽象缓存操作
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("Redis:ConnectionString")
//        ?? throw new InvalidOperationException("Connection string 'RedisConn' not found.");
//    options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName") ?? throw new InvalidOperationException("Redis instance name not found.");
//});
// 注册原生 StackExchange.Redis 客户端（灵活控制）
builder.Services.AddSingleton<IConnectionMultiplexer>(options =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis:ConnectionString")
        ?? throw new InvalidOperationException("Connection string 'RedisConn' not found.");
    return ConnectionMultiplexer.Connect(configuration);
});
// 注册 RedisService
builder.Services.AddSingleton<IRedisService, RedisService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
}
app.UseHttpsRedirection();
app.Run();

