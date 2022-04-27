using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;

namespace SV2.Database.Models.Factories;

public class Mine : IHasOwner, IBuilding
{
    [Key]
    [GuidID]
    public string Id { get; set; }

    [VarChar(64)]
    public string? Name { get; set; }

    [VarChar(1024)]
    public string? Description { get; set; }

    [EntityId]
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }

    [GuidID]
    public string CountyId { get; set; }

    // the name of the resource that this mine mines
    [VarChar(32)]
    public string ResourceName { get; set; }

    public int Level { get; set; }
    public bool HasAnEmployee { get; set; }

    // amount of ResourceName that this mine produces per hour
    public double Rate { get; set;}

    public double Quantity { get; set; }

    // base is 1x
    public double QuantityGrowthRate { get; set; }
    public double QuantityCap { get; set; }

    // default is 1, size directly increases output
    // max value is 10

    public int Size { get; set; }

    // every tick (1 hour), Age increases by 1
    public int Age { get; set; }

    public int HoursSinceBuilt { get; set; }

    [NotMapped]
    public BuildingType buildingType { 
        get {
            return BuildingType.Factory;
        }
    }

    [NotMapped]
    public County County {
        get {
            return DBCache.Get<County>(CountyId)!;
        }
    }

    public string GetProduction()
    {
        return ResourceName;
    }

    public async Task Tick(List<TradeItem> tradeItems)
    {
        // TODO: when we add district stats (industal stat, etc) update this
        

        double rate = Rate;

        rate *= Size;

        if (HasAnEmployee) {
            rate *= 1.5;
        };

        // ((A2^1.2/1.6)-1)/1000

        // ex:
        // 10 days : 1% lost
        // 100 days: 15.8% lost
        // 300 days: 58.8% lost

        double AgeProductionLost = ( (Math.Pow(Age, 1.2) / 1.6)-1 ) / 1000;

        rate *= 1-AgeProductionLost;

        // tick Quantity system

        // ex:
        // 3 days : 26.24%
        // 11 days: 57.28%
        // 32 days: 82.78% 

        if (Quantity < QuantityCap) {
            HoursSinceBuilt += 1;
            double days = HoursSinceBuilt/24;
            double newQuantity = Math.Max(1.5, Math.Log10( Math.Pow(days, 20) / 40));
            newQuantity = Math.Min(0.1, newQuantity);
            newQuantity *= QuantityGrowthRate;

            Quantity = newQuantity;
        }

        rate *= Quantity;

        rate *= 30;

        // find the tradeitem
        TradeItem? item = tradeItems.FirstOrDefault(x => x.Definition.Name == ResourceName && x.Definition.OwnerId == "g-vooperia" && x.OwnerId == OwnerId);
        if (item is null) {
            item = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = OwnerId,
                Definition_Id = DBCache.GetAll<TradeItemDefinition>().FirstOrDefault(x => x.Name == ResourceName && x.OwnerId == "g-vooperia")!.Id,
                Amount = 0
            };
            await DBCache.Put<TradeItem>(item.Id, item);
            await VooperDB.Instance.TradeItems.AddAsync(item);
            await VooperDB.Instance.SaveChangesAsync();
        }
        item.Amount += (int)rate;
    }

}