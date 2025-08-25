using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SodimacOrders.Application.DTOs;
using System.Security.Claims;

namespace SodimacOrders.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }

        protected string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        protected string? GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value;
        }

        protected string? GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }

        protected bool IsInRole(params string[] roles)
        {
            var userRole = GetCurrentUserRole();
            return !string.IsNullOrEmpty(userRole) && roles.Contains(userRole);
        }

        protected ActionResult<ApiResponseDto<T>> SuccessResult<T>(T data, string message = "")
        {
            return Ok(ApiResponseDto<T>.SuccessResponse(data, message));
        }

        protected ActionResult<ApiResponseDto<T>> ErrorResult<T>(string message, List<string>? errors = null)
        {
            return BadRequest(ApiResponseDto<T>.ErrorResponse(message, errors));
        }

        protected ActionResult<ApiResponseDto<T>> NotFoundResult<T>(string message = "Recurso no encontrado")
        {
            return NotFound(ApiResponseDto<T>.ErrorResponse(message));
        }

        protected ActionResult<ApiResponseDto<T>> ForbiddenResult<T>(string message = "No tiene permisos para realizar esta acción")
        {
            return StatusCode(403, ApiResponseDto<T>.ErrorResponse(message));
        }

        protected ActionResult<ApiResponseDto<T>> InternalServerErrorResult<T>(string message = "Error interno del servidor")
        {
            return StatusCode(500, ApiResponseDto<T>.ErrorResponse(message));
        }
    }
}