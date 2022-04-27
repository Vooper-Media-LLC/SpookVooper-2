using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Groups;
using Microsoft.EntityFrameworkCore;

namespace SV2.Database.Models.Districts;


public class District
{
    [Key]
    [GuidID]
    public string Id { get; set;}

    [VarChar(64)]
    public string? Name { get; set;}

    [VarChar(512)]
    public string? Description { get; set; }

    [InverseProperty("District")]
    public ICollection<County> Counties { get; set;}

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

    public static District Find(string id)
    {
        return DBCache.GetAll<District>().FirstOrDefault(x => x.Id == id)!;
    }
}