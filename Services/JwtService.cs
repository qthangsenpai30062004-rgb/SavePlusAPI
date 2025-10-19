using Microsoft.IdentityModel.Tokens;
using SavePlus_API.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SavePlus_API.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GeneratePatientToken(PatientDto patient)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, patient.PatientId.ToString()),
                new Claim("UserId", patient.PatientId.ToString()), // Add explicit UserId claim for patients
                new Claim(ClaimTypes.Name, patient.FullName),
                new Claim(ClaimTypes.MobilePhone, patient.PrimaryPhoneE164),
                new Claim("UserType", "Patient"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            return GenerateToken(claims);
        }

        public string GeneratePatientToken(UserInfoDto userInfo)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString()),
                new Claim("UserId", userInfo.UserId.ToString()),
                new Claim(ClaimTypes.Name, userInfo.FullName),
                new Claim("UserType", "Patient"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (!string.IsNullOrEmpty(userInfo.Email))
                claims.Add(new Claim(ClaimTypes.Email, userInfo.Email));
            
            if (!string.IsNullOrEmpty(userInfo.PhoneE164))
                claims.Add(new Claim(ClaimTypes.MobilePhone, userInfo.PhoneE164));

            return GenerateToken(claims);
        }

        public string GenerateUserToken(UserInfoDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("UserId", user.UserId.ToString()), // Add explicit UserId claim
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("UserType", "Staff"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            
            if (!string.IsNullOrEmpty(user.PhoneE164))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneE164));
            
            if (!string.IsNullOrEmpty(user.Role))
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            
            if (user.TenantId.HasValue)
            {
                claims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
                claims.Add(new Claim("TenantName", user.TenantName ?? ""));
            }

            return GenerateToken(claims);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetTokenValidationParameters();

                await tokenHandler.ValidateTokenAsync(token, validationParameters);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }

        public string? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);
                return jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract user ID from token");
                return null;
            }
        }

        public string? GetPatientIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(token);
                var userType = jwt.Claims.FirstOrDefault(x => x.Type == "UserType")?.Value;
                
                if (userType == "Patient")
                {
                    return jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract patient ID from token");
                return null;
            }
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var key = GetSecretKey();
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "SavePlus-API",
                audience: _configuration["Jwt:Audience"] ?? "SavePlus-Users",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30), // Token valid for 30 days
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private SymmetricSecurityKey GetSecretKey()
        {
            var secret = _configuration["Jwt:SecretKey"] ?? "SavePlus-Super-Secret-Key-For-JWT-Token-Generation-2024";
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "SavePlus-API",
                ValidAudience = _configuration["Jwt:Audience"] ?? "SavePlus-Users",
                IssuerSigningKey = GetSecretKey(),
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
