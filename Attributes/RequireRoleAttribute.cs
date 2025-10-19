using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SavePlus_API.Constants;
using System.Security.Claims;

namespace SavePlus_API.Attributes
{
    /// <summary>
    /// Authorization attribute để kiểm tra role của user
    /// </summary>
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _requiredRoles;

        public RequireRoleAttribute(params string[] requiredRoles)
        {
            _requiredRoles = requiredRoles ?? throw new ArgumentNullException(nameof(requiredRoles));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Kiểm tra user đã authenticated chưa
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy role từ claims
            var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userRole))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Kiểm tra role có trong danh sách được phép không
            if (!_requiredRoles.Contains(userRole))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
