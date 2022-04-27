using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Users;

namespace SV2.Database.Models.Groups;

public enum GroupTypes
{
    Company,
    // a corporation is a company that is listed on SVSE or a company on a private stock exchange that the CFV has determined is a corporation
    Corporation,
    NonProfit,
    PoliticalParty, 
    District
}

public enum GroupFlag
{
    // is only given by the CFV
    Charity,
    // is only given by the MOJ
    News
}

public class Group : IHasOwner, IEntity
{
    [Key]
    [EntityId]
    public string Id { get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(2048)]
    public string? Description { get; set; }

    [VarChar(512)]
    public string? Image_Url { get; set; }
    
    [EntityId]
    public string? DistrictId { get; set;}
    public decimal Credits { get; set;}
    public decimal TaxAbleCredits { get; set; }
    public List<decimal>? CreditSnapshots { get; set;}

    public List<string> MembersIds { get; set; }
    
    [JsonIgnore]
    [VarChar(36)]
    public string Api_Key { get; set; }
    public GroupTypes GroupType { get; set; }
    // will be use the PostgreSQL Array datatype
    public List<GroupFlag> Flags { get; set; }
    // if the group is open to the public
    public bool Open { get; set; }

    public bool IsInGroup(User user)
    {
        return MembersIds.Contains(user.Id);
    }

    public IEnumerable<User> GetMembers()
    {
        return MembersIds.Select(x => User.Find(x));
    }
    
    [EntityId]
    public string OwnerId { get; set; }

    [NotMapped]

    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }

    public Group()
    {
        
    }

    public Group(string name, string ownerId)
    {
        Id = "g-"+Guid.NewGuid().ToString();
        Name = name;
        Api_Key = Guid.NewGuid().ToString();
        Credits = 0.0m;
        CreditSnapshots = new();
        OwnerId = ownerId;
        Open = false;
        Flags = new();
        GroupType = GroupTypes.Company;
        MembersIds = new() {OwnerId};
    }

    public GroupRole? GetHighestRole(IEntity user) 
    {
        GroupRole? role = DBCache.GetAll<GroupRole>().Where(x => x.GroupId == Id && x.Members.Contains(user.Id)).OrderByDescending(x => x.Authority).FirstOrDefault();
        if (role is null)
        {
            return GroupRole.Default;
        }
        return role;
    }

    public GroupRole GetHighestRoleWithPermission(IEntity user, GroupPermission permission) 
    {
        GroupRole role = DBCache.GetAll<GroupRole>().Where(x => x.GroupId == Id && x.Members.Contains(user.Id) && HasPermission(user, permission)).OrderByDescending(x => x.Authority).First();
        return role;
    }

    public bool HasPermissionWithKey(string apikey, GroupPermission permission)
    {
        if (apikey == Api_Key) {
            return true;
        }
        
        // add oauth key handling
        return false;
        
    }

    public bool HasPermission(IEntity entity, GroupPermission permission)
    {
        if (entity.Id == OwnerId) {
            return true;
        }

        foreach(GroupRole role in DBCache.GetAll<GroupRole>().Where(x => x.GroupId == Id && x.Members.Contains(entity.Id)).OrderByDescending(x => x.Authority))
        {
            PermissionCode code = new PermissionCode(role.PermissionValue, permission.Value);
            PermissionState state = code.GetState(permission);

            if (state == PermissionState.Undefined)
            {
                continue;
            }
            else if (state == PermissionState.True)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;

    }

    public static Group? Find(string Id)
    {
        return DBCache.Get<Group>(Id);
    }
}