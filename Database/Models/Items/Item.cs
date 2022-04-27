using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Economy;

namespace SV2.Database.Models.Items;

public class TradeItem : IHasOwner
{
    [Key]
    [GuidID]
    public string Id { get; set; }

    [EntityId]
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }
    
    [GuidID]
    public string Definition_Id { get; set; }
    
    public TradeItemDefinition Definition { 
        get {
            return DBCache.Get<TradeItemDefinition>(Definition_Id);
        }
    }
    public int Amount { get; set;}
    public async Task Give(IEntity entity, int amount) 
    {
        // check if the entity we are sending already has this TradeItem
        TradeItem? item = DBCache.GetAll<TradeItem>().FirstOrDefault(x => x.OwnerId == entity.Id && x.Definition_Id == Definition_Id);
        
        // if null then create one

        if (item is null) 
        {
            item = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = entity.Id,
                Definition_Id = Definition_Id,
                Amount = 0
            };
            await DBCache.Put<TradeItem>(item.Id, item);
            await VooperDB.Instance.TradeItems.AddAsync(item);
            await VooperDB.Instance.SaveChangesAsync();
        }

        item.Amount += amount;
    }
}

public class TradeItemDefinition : IHasOwner
{
    [Key]
    [GuidID]
    public string Id { get; set; }

    [EntityId]
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }
    // for example SV would have a "Tank" definition owned by SV, in which case "Tank" would be the name
    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(1024)]
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    // json list of modifiers
    public string? Modifiers { get; set; }

    public TradeItemDefinition()
    {

    }

    public TradeItemDefinition(string ownerid, string name)
    {
        Id = Guid.NewGuid().ToString();
        OwnerId = ownerid;
        Name = name;
        Created = DateTime.UtcNow;
    }
}