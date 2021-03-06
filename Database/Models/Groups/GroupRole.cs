using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Permissions;

namespace SV2.Database.Models.Groups;

public class GroupRole
{
    [Key]
    [GuidID]
    public string Id { get; set; }

    [VarChar(64)]
    public string Name { get; set; }
    // this role's permission value
    public ulong PermissionValue { get; set; }

    public List<String> Members { get; set; }

    // Hexcode for role color (ex: #ffffff)
    public string Color { get; set; }

    // The group this role belongs to
    [EntityId]
    public string GroupId { get; set; }

    [NotMapped]
    public Group Group { 
        get {
            return DBCache.Get<Group>(GroupId)!;
        }
    }
    
    // Salary for role, paid every hour
    public decimal Salary { get; set; }
    public int Authority { get; set; }

    public bool HasPermission(GroupPermission permission)
    {
        return Permission.HasPermission(PermissionValue, permission);
    }
}