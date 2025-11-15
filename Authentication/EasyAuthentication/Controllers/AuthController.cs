using EasyAuthentication.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EasyAuthentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController:ControllerBase
    {
        private readonly List<TrustedApp> _trustedApps;
        private readonly JwtSettings _jwtSettings;
        public AuthController(IOptions<List<TrustedApp>> trustedAppsOptions, IOptions<JwtSettings> jwtSettings)
        {
            _trustedApps = trustedAppsOptions.Value;
            _jwtSettings = jwtSettings.Value;
        }

        // GET 请求默认不支持 RequestBody
        [HttpPost("token")]
        [AllowAnonymous]
        public IActionResult GetToken([FromBody]TokenRequest request)
        {
            if(string.IsNullOrEmpty(request.ServiceId) || string.IsNullOrEmpty(request.ServiceSecret))
            {
                return Unauthorized("API key 不能为空, 无法获取 Token");
            }

            TrustedApp app = _trustedApps.FirstOrDefault(s => s.ServiceId == request.ServiceId);
            if(app == null || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(app.Secret),
                Encoding.UTF8.GetBytes(request.ServiceSecret)))
            {
                return Unauthorized("无效的 API key, 无法获取 Token");
            }

            // 1. 构建 Token 的 Claim（负载）：存储自定义信息（如服务标识、用户ID等，可选）
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, app.ServiceName), // 示例：调用方服务名称
                new Claim("ServiceId", request.ServiceId.ToString()), // 自定义 Claim：服务ID
                new Claim("IssuedAt", DateTime.Now.ToString()) // 自定义 Claim：签发时间
            };

            // 2. 生成签名密钥(用于对token做签名，别人拿着token能知道是自己颁发的)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. 构建 Token
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes), // 过期时间（UTC 时间，避免时区问题）
                signingCredentials: credentials
            );

            // 4. 序列化 Token 为字符串
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 5. 返回 Token 给调用方
            return Ok(new
            {
                Token = tokenString,
                ExpiresIn = _jwtSettings.ExpirationInMinutes * 60, // 有效期（秒）
                TokenType = "Bearer" // 认证类型（固定为 Bearer）
            });
        }
    }
}
