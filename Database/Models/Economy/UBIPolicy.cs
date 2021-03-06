using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Users;

namespace SV2.Database.Models.Economy;

public class UBIPolicy
{
    [Key]
    [GuidID]
    public string Id { get; set; }
    public decimal Rate { get; set;}

    // if true, then pay Rate to everyone, and ApplicableRank should be set to Unranked
    public bool Anyone { get; set;}

    // users with this rank will get paid Rate hourly
    public Rank? ApplicableRank { get; set;}

    // should be Null if this is the Vooperian UBI
    [EntityId]
    public string? DistrictId { get; set;}
}