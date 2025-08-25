using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SodimacOrders.Application.DTOs;
using SodimacOrders.Application.Interfaces;
using SodimacOrders.Domain.Entities;
using SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _config;

        public AuthService(
            IAuthRepository authRepository,
            ILogger<AuthService> logger,
            IConfiguration config)
        {
            _authRepository = authRepository;
            _logger = logger;
            _config = config;
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.Email)
            };

            // Agregar roles si existen
            if (user.Role != null)
            {
                foreach (var rol in user.Role)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Obtener duración desde configuración o usar valor por defecto
            int durationHours = _config.GetValue<int?>("Jwt:DurationInHours") ?? 2;

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(durationHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}