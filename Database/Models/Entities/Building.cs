
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database.Models.Buildings;

public enum BuildingType
{
    Factory = 1,
    Mine = 2
}

public interface IBuilding
{
    public string Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string OwnerId { get; set; }

    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }

    public string CountyId { get; set; } 

    [NotMapped]
    public County County {
        get {
            return DBCache.Get<County>(CountyId)!;
        }
    }

    BuildingType buildingType { get;}

    public string GetProduction();
}