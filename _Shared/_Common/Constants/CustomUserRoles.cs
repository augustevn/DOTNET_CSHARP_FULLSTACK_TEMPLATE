namespace _Common.Constants;

public class CustomUserRoles
{
    public static IEnumerable<string> RoleNamesAsArray => new[]
        {Admin, Moderator, Author, Editor , AppUser};

    public const string Admin = "ADMINISTRATOR";
    public const string Moderator = "MODERATOR";
    public const string Author = "AUTHOR";
    public const string Editor = "EDITOR";
    public const string AppUser = "APP_USER";

    public const string RolesWithAdminAccess = Admin;
    public const string RolesWithModeratorAccess = $"{Admin}, {Moderator}";
    public const string RolesWithAppUserAccess = $"{RolesWithModeratorAccess}, {AppUser}";
}