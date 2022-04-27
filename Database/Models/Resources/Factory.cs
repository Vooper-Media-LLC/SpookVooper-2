using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;
using SV2.Managers;

namespace SV2.Database.Models.Factories;

public class Factory : IHasOwner, IBuilding
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

    [VarChar(256)]
    public string? RecipeName { get; set; }
    
    [NotMapped]
    public Recipe recipe {
        get {
            return ResourceManager.Recipes.FirstOrDefault(x => x.Name == RecipeName);
        }
    }
    public string? EmployeeId { get; set; }

    // effects production speed, grows over time, min value is 10%
    public double Quantity { get; set; }

    // base is 1x
    public double QuantityGrowthRate { get; set; }


    public double QuantityCap { get; set; }

    public double Efficiency { get; set; }

    // default is 1, size directly increases output, but harms efficiency
    // max value is 10, and at 10, it costs 4x more input to produce the same output

    public int Size { get; set; }

    public int HoursSinceChangedProductionRecipe { get; set; }

    // every tick (1 hour), Age increases by 1
    public int Age { get; set; }

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
        if (recipe is null)
        {
            return "";
        }
        string output = "";
        foreach(KeyValuePair<string, double> item in recipe.Outputs) {
            output += $"{item.Key}, ";
        }
        if (output != "") {
            output = output.Substring(0, output.Length-2);
        }
        return output;
    }

    public Factory()
    {
        
    }

    public Factory(string ownerid, string countyid)
    {
        // why so many variables
        Id = Guid.NewGuid().ToString();
        OwnerId = ownerid;
        CountyId = countyid;
        Quantity = 0.1;
        QuantityCap = 1.5;
        QuantityGrowthRate = 1;
        Efficiency = 1;
        Size = 1;
        HoursSinceChangedProductionRecipe = 1;
        Age = 1;
    }

    /// <summary>
    /// This function is called every IRL hour
    /// </summary>

    public async Task Tick(List<TradeItem> tradeItems)
    {

        if (RecipeName is null) {
            return;
        }

        // TODO: when we add district stats (industal stat, etc) update this
        double rate = Size;

        if (EmployeeId != null) {
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
            HoursSinceChangedProductionRecipe += 1;
            double days = HoursSinceChangedProductionRecipe/24;
            double newQuantity = Math.Max(QuantityCap, Math.Log10( Math.Pow(days, 20) / 40));
            newQuantity = Math.Min(0.1, newQuantity);
            newQuantity *= QuantityGrowthRate;

            Quantity = newQuantity;
        }

        rate *= Quantity;

        rate *= 30;


        foreach(string Resource in recipe.Inputs.Keys) 
        {
            // find the tradeitem
            TradeItem? item = tradeItems.FirstOrDefault(x => x.Definition.Name == Resource && x.Definition.OwnerId == "g-vooperia" && x.OwnerId == OwnerId);
            if (item is null) {
                break;
            }
            int amountNeeded = (int)(recipe.Inputs[Resource]*rate/Efficiency);
            if (item.Amount < amountNeeded) {
                break;
            }
            item.Amount -= amountNeeded;
        }

        foreach(string Resource in recipe.Outputs.Keys) 
        {
            // find the tradeitem
            TradeItem? item = tradeItems.FirstOrDefault(x => x.Definition.Name == Resource && x.Definition.OwnerId == "g-vooperia" && x.OwnerId == OwnerId);
            if (item is null) {
                item = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    OwnerId = OwnerId,
                    Definition_Id = DBCache.GetAll<TradeItemDefinition>().FirstOrDefault(x => x.Name == Resource && x.OwnerId == "g-vooperia")!.Id,
                    Amount = 0
                };
                await DBCache.Put<TradeItem>(item.Id, item);
                await VooperDB.Instance.TradeItems.AddAsync(item);
                await VooperDB.Instance.SaveChangesAsync();
            }
            item.Amount += (int)(recipe.Outputs[Resource]*rate);
        }
    }

}