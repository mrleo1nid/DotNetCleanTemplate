namespace DotNetCleanTemplate.Shared.Common
{
    public static class ErrorCodes
    {
        public const string UserNotFound = "User.NotFound";
        public const string UserAlreadyExists = "User.AlreadyExists";
        public const string RoleNotFound = "Role.NotFound";
        public const string UserRoleAlreadyExists = "UserRole.AlreadyExists";
        public const string UserRoleNotFound = "UserRole.NotFound";
        public const string CannotRemoveLastAdmin = "UserRole.CannotRemoveLastAdmin";
    }
}
