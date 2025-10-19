using System.ComponentModel.DataAnnotations;
using SavePlus_API.Constants;

namespace SavePlus_API.Attributes
{
    /// <summary>
    /// Validation attribute để kiểm tra role hợp lệ
    /// </summary>
    public class ValidRoleAttribute : ValidationAttribute
    {
        private readonly bool _allowAdvancedRoles;

        public ValidRoleAttribute(bool allowAdvancedRoles = false)
        {
            _allowAdvancedRoles = allowAdvancedRoles;
        }

        public override bool IsValid(object? value)
        {
            if (value == null || value is not string role)
                return false;

            if (UserRoles.CoreRoles.Contains(role))
                return true;

            if (_allowAdvancedRoles && UserRoles.AdvancedRoles.Contains(role))
                return true;

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            var allowedRoles = _allowAdvancedRoles ? UserRoles.AllRoles : UserRoles.CoreRoles;
            return $"{name} phải là một trong các role hợp lệ: {string.Join(", ", allowedRoles)}";
        }
    }
}
