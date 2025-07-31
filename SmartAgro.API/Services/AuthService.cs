using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SmartAgro.Models.DTOs.Auth;
using SmartAgro.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartAgro.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Buscar usuario por email
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    _logger.LogWarning($"Intento de login fallido - Usuario no encontrado: {loginDto.Email}");
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // Verificar que el usuario esté activo
                if (!user.Activo)
                {
                    _logger.LogWarning($"Intento de login fallido - Usuario inactivo: {loginDto.Email}");
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Su cuenta está desactivada. Contacte al administrador."
                    };
                }

                // Verificar contraseña
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning($"Intento de login fallido - Contraseña incorrecta: {loginDto.Email}");
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Credenciales inválidas"
                    };
                }

                // Generar token JWT
                var token = await GenerateJwtTokenAsync(user);
                var userRoles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation($"✅ Login exitoso: {loginDto.Email}");

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Login exitoso",
                    Token = token,
                    Expiration = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:ExpirationInDays"]!)),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Nombre = user.Nombre,
                        Apellidos = user.Apellidos,
                        Email = user.Email!,
                        Telefono = user.Telefono,
                        Direccion = user.Direccion,
                        Roles = userRoles.ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error durante el login: {loginDto.Email}");
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error interno del servidor"
                };
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

                // Validar token sin verificar expiración
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = false, // No validar expiración para refresh
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var userId = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Token inválido"
                    };
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !user.Activo)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Usuario no válido o inactivo"
                    };
                }

                // Generar nuevo token
                var newToken = await GenerateJwtTokenAsync(user);
                var userRoles = await _userManager.GetRolesAsync(user);

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Token refrescado exitosamente",
                    Token = newToken,
                    Expiration = DateTime.UtcNow.AddDays(int.Parse(jwtSettings["ExpirationInDays"]!)),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Nombre = user.Nombre,
                        Apellidos = user.Apellidos,
                        Email = user.Email!,
                        Telefono = user.Telefono,
                        Direccion = user.Direccion,
                        Roles = userRoles.ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al refrescar token");
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error al refrescar token"
                };
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation($"✅ Logout exitoso para usuario: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error durante logout para usuario: {userId}");
                return false;
            }
        }

        private async Task<string> GenerateJwtTokenAsync(Usuario user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("nombre", user.Nombre),
                new Claim("apellidos", user.Apellidos),
                new Claim("telefono", user.Telefono ?? ""),
                new Claim("direccion", user.Direccion ?? "")
            };

            // Agregar roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(int.Parse(jwtSettings["ExpirationInDays"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // ❌ REMOVIDO: RegisterAsync ya no se implementa aquí
        // El registro de clientes se maneja exclusivamente a través del UserService
        // cuando un administrador crea un usuario desde el panel de administración
    }
}