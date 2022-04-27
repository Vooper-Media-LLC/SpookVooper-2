using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Users;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Permissions;

namespace SV2.Database.Models.Economy;


public class CreditAccount : IHasOwner, IEntity
{
    [Key]
    [GuidID]
    public string Id { get; set;}

    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(512)]
    public string Description { get; set; }

    [NotMapped]
    public string Image_Url { get; set; }

    [EntityId]
    public string OwnerId { get; set;}
    
    [ForeignKey("OwnerId")]
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }

    [JsonIgnore]
    public string Api_Key { get; set; }
    public decimal Credits { get; set; }
    public decimal TaxAbleCredits { get; set; }

    [EntityId]
    public string? DistrictId { get; set; }
    // used for tax purposes
    public List<decimal> CreditSnapshots { get; set;}

    public bool HasPermissionWithKey(string apikey, GroupPermission permission)
    {
        return false;
    }

    public bool HasPermission(IEntity entity, GroupPermission permission)
    {
        return false;
    }
}