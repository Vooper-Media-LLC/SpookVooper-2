using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Groups;
using Microsoft.EntityFrameworkCore;

namespace SV2.Database.Models.Districts;

public class DistrictModifier
{
    public DistrictModifierType Type { get; set; }
    public decimal Amount { get; set; }  
}

public class District
{
    [Key]
    [GuidID]
    public string Id { get; set;}

    [VarChar(64)]
    public string? Name { get; set;}

    [VarChar(512)]
    public string? Description { get; set; }

    [NotMapped]
    public List<Province> Provinces {
        get {
            return DBCache.GetAll<Province>().Where(x => x.DistrictId == Id).ToList();
        }
    }

    public Group Group { 
        get {
            return DBCache.Get<Group>(GroupId)!;
        }
    }

    [EntityId]
    public string GroupId { get; set; }

    [EntityId]
    public string? SenatorId { get; set;}

    [EntityId]
    public string? GovernorId { get; set;}

    [VarChar(128)]
    public string? FlagUrl { get; set; }

    [Column(TypeName = "jsonb")]
    public List<DistrictModifier> Modifiers { get; set; }

    public static District Find(string id)
    {
        return DBCache.GetAll<District>().FirstOrDefault(x => x.Id == id)!;
    }

    public decimal GetModifier(DistrictModifierType modifierType)
    {
        DistrictModifier modifier = Modifiers.FirstOrDefault(x => x.Type == modifierType);
        if (modifier == null) {
            string modifierTypeName = modifierType.ToString();
            if (modifierTypeName.Contains("Factor")) {
                // if this modifier is factor then it applies a percent change so just return 1 (100%)
                return 1;
            }
            return 0;
        }

        return modifier.Amount;
    }
}