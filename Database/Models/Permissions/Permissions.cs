namespace SV2.Database.Models.Permissions;

/// <summary>
/// This class contains all group permissions and helper methods for working
/// with them.
/// </summary>
public static class GroupPermissions
{
    /// <summary>
    /// Contains every group permission
    /// </summary>
    public static GroupPermission[] Permissions;

    static GroupPermissions()
    {
        Permissions = new GroupPermission[]
        {
                FullControl,
                ManageRoles,
                ManageInvites,
                ManageMembership,
                Post,
                Eco
        };
    }

    // Use shared full control definition
    public static readonly GroupPermission FullControl = new GroupPermission(Permission.FULL_CONTROL, "Full Control", "Control every part of this group.");

    // Every subsequent permission has double the value (the next bit)
    // An update should NEVER change the order or value of old permissions
    // As that would be a massive security issue
    public static readonly GroupPermission ManageRoles = new GroupPermission(0x01, "Manage Roles", "Allows members to manage roles below their authority.");
    public static readonly GroupPermission ManageInvites = new GroupPermission(0x02, "Manage Invites", "Allows members to manage invites.");
    public static readonly GroupPermission ManageMembership = new GroupPermission(0x04, "Manage Membership", "Allows members to kick or ban users from the group which are below their authority.");
    public static readonly GroupPermission Post = new GroupPermission(0x8, "Post", "Allows members to post as this group. For example to make group forum posts or post new articles.");
    public static readonly GroupPermission Eco = new GroupPermission(0x10, "Eco", "Allows members to send transactions, trade stocks, etc as this group.");
    public static readonly GroupPermission Edit = new GroupPermission(0x20, "Edit", "Allows members to edit details about this group.");
    public static readonly GroupPermission News = new GroupPermission(0x20, "News", "Allows members to post news under this group.");
}

public class GroupPermission : Permission
{
    public GroupPermission(ulong value, string name, string description) : base(value, name, description)
    {
    }
}

/// <summary>
/// Permissions are basic flags used to denote if actions are allowed
/// to be taken on one's behalf
/// </summary>
public class Permission
{
    /// <summary>
    /// Permission node to have complete control
    /// </summary>
    public const ulong FULL_CONTROL = ulong.MaxValue;

    /// <summary>
    /// The name of this permission
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of this permission
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The value of this permission
    /// </summary>
    public ulong Value { get; set; }

    /// <summary>
    /// Initializes the permission
    /// </summary>
    public Permission(ulong value, string name, string description)
    {
        this.Name = name;
        this.Description = description;
        this.Value = value;
    }

    /// <summary>
    /// Returns whether the given code includes the given permission
    /// </summary>
    public static bool HasPermission(ulong code, Permission permission)
    {
        // Case if full control is granted
        if (code == FULL_CONTROL) return true;

        // Otherwise check for specific permission
        return (code & permission.Value) == permission.Value;
    }

    /// <summary>
    /// Creates and returns a permission code from given permissions 
    /// </summary>
    public static ulong CreateCode(params Permission[] permissions)
    {
        ulong code = 0x00;

        foreach (Permission permission in permissions)
        {
            if (permission != null)
            {
                code |= permission.Value;
            }
        }

        return code;
    }
}

public enum PermissionState
{
    Undefined, True, False
}

/// <summary>
/// Permission codes use two ulongs to represent
/// three possible states for every permission
/// </summary>
public struct PermissionCode
{
    // Just for reference,
    // If the mask bit is 0, then it is always undefined
    // If the mask but is 1, then if the code bit is 1 it is true. Otherwise it is false.
    // This basically compresses 64 booleans (64 bytes) into 2 ulongs (16 bytes)

    public ulong Code { get; set; }
    public ulong Mask { get; set; }

    public PermissionCode(ulong code, ulong mask)
    {
        this.Code = code;
        this.Mask = mask;
    }

    public PermissionState GetState(Permission permission)
    {
        if ((Mask & permission.Value) != permission.Value)
        {
            return PermissionState.Undefined;
        }

        if ((Code & permission.Value) != permission.Value)
        {
            return PermissionState.False;
        }

        return PermissionState.True;
    }
}