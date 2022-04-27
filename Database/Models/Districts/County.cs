using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Districts;


public class County
{
    [Key]
    [GuidID]
    public string Id { get; set;}

    [VarChar(64)]
    public string? Name { get; set;}

    [VarChar(512)]
    public string? Description { get; set; }
    public int Population { get; set;}

    [EntityId]
    public string DistrictId { get; set;}

    [ForeignKey("DistrictId")]
    public District District { get; set;}

    public IEnumerable<IBuilding> GetBuildings()
    {
        List<IBuilding> buildings = new();
        buildings.AddRange(DBCache.GetAll<Factory>().Where(x => x.CountyId == Id));
        buildings.AddRange(DBCache.GetAll<Mine>().Where(x => x.CountyId == Id));
        return buildings;
    }
}