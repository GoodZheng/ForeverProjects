using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT
// 配置 TrustedServices 列表(注入 DI 中，其他类可以使用 IOptions<List<TrustedService>> 对象的属性 Value 获取)
builder.Services.Configure<List<TrustedApp>>(builder.Configuration.GetSection("TrustedServices"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
// 读取 JWT 设置
JwtSettings jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not found in configuration.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.FromSeconds(10) // 可选：可以时间偏移10s防止网络延迟问题
    };
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public class JwtSettings
{
    public string SecretKey { get; set; }

    public int ExpirationInMinutes { get; set; }

    public string Issuer { get; set; }

    public string Audience { get; set; }
}

public class TrustedApp
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}