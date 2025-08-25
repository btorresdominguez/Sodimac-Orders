using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SodimacOrders.Application.DTOs;
using SodimacOrders.Application.Interfaces;
using SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities;
using SodimacOrders.Infrastructure.Repositories;
using System.Security.Claims;

namespace SodimacOrders.WebApi.Controllers
{
    /// <summary>
    /// Controlador para manejo de autenticación
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthRepository _authRepository;
        public AuthController(IAuthService authService, ILogger<AuthController> logger, IAuthRepository authRepository)
        {
            _authService = authService;
            _logger = logger;
            _authRepository = authRepository;
        }

        /// <summary>
        /// Inicia sesión de usuario
        /// </summary>
        /// <param name="loginRequest">Datos de login (email y contraseña)</param>
        /// <returns>Token JWT y información del usuario</returns>
        /// <response code="200">Login exitoso</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">Credenciales incorrectas</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login(
            [FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", loginRequest?.Email);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage)).ToList();
                    _logger.LogWarning("Login failed due to invalid model state for email: {Email}", loginRequest?.Email);
                    return BadRequest(ApiResponseDto<LoginResponseDto>.ErrorResponse("Datos inválidos", errors));
                }

                var result = await _authRepository.GetUserByEmailAsync(loginRequest.Email, loginRequest.Contraseña);

                if (result == null)
                {
                    _logger.LogWarning("Intento fallido de inicio de sesión para {Email}", loginRequest.Email);
                    return Unauthorized("Usuario o contraseña inválidos");
                }

                var token = _authService.GenerateJwtToken(result);
               //await _authRepository.SaveToken(result.IdUsuario, token);

        
                _logger.LogInformation("Usuario {Email} inició sesión correctamente", loginRequest.Email);

                return Ok(new
                {
                    token,
                    usuario = result.Email,
                    roles = result.Role                  
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", loginRequest?.Email);
                return StatusCode(500, ApiResponseDto<LoginResponseDto>.ErrorResponse("Error interno del servidor"));
            }
        }


   }
}