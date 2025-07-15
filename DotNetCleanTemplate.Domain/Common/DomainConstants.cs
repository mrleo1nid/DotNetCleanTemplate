namespace DotNetCleanTemplate.Domain.Common
{
    public static class DomainConstants
    {
        public static readonly int MaxPasswordHashLength = 20;
        public static readonly int MinPasswordHashLength = 6;
        public static readonly int MinRoleNameLength = 3;
        public static readonly int MaxRoleNameLength = 255;
        public static readonly int MinUserNameLength = 3;
        public static readonly int MaxUserNameLength = 255;
        public static readonly int MinEmailLength = 3;
        public static readonly int MaxEmailLength = 255;
    }
}
