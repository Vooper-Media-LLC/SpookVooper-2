using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SV2.Database.Models.Users;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Economy;
using System.Threading.Tasks;

namespace SV2.Database.Models.Entities;

public enum EntityType
{
    User,
    Group,
    CreditAccount
}

public interface IHasOwner
{
    public string OwnerId { get; set; }
    public IEntity Owner { get;}
}

public interface IEntity
{
    // the id will be in the following format:
    // x-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    // ex: u-c60c6bd8-0409-4cbd-8bb8-3c87e24c55f8
    [Key]
    [GuidID]
    public string Id { get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(512)]
    public string Description { get; set; }
    decimal Credits { get; set;}
    decimal CreditsYesterday { get; set;}
    
    [JsonIgnore]
    [VarChar(36)]
    public string Api_Key { get; set; }
    public string Image_Url { get; set; }

    [EntityId]
    public string? DistrictId { get; set; }
    public static IEntity? Find(string Id)
    {
        return DBCache.FindEntity(Id);
    }

    public async Task DoIncomeTax()
    {
        decimal amount = Credits-CreditsYesterday;
        if (amount <= 0.0m) {
            return;
        }
        decimal totaldue = 0.0m;

        // do district level taxes
        foreach(TaxPolicy policy in DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == DistrictId).OrderBy(x => x.Minimum))
        {
            totaldue += policy.GetTaxAmount(amount);
            amount -= policy.Maximum;
            if (amount <= 0.0m) {
                break;
            }
        }
        Transaction taxtrans = new()
        {
            Id = Guid.NewGuid().ToString(),
            Credits = totaldue,
            Time = DateTime.UtcNow,
            FromId = Id,
            ToId = DistrictId,
            transactionType = TransactionType.TaxPayment,
            Details = $"Income Tax Payment"
        };
        taxtrans.Execute(true);

        amount = Credits-CreditsYesterday;
        totaldue = 0.0m;

        // do imperial level taxes
        foreach(TaxPolicy policy in DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == null).OrderBy(x => x.Minimum))
        {
            totaldue += policy.GetTaxAmount(amount);
            amount -= policy.Maximum;
            if (amount <= 0.0m) {
                break;
            }
        }
        taxtrans = new()
        {
            Id = Guid.NewGuid().ToString(),
            Credits = totaldue,
            Time = DateTime.UtcNow,
            FromId = Id,
            ToId = "g-vooperia",
            transactionType = TransactionType.TaxPayment,
            Details = $"Income Tax Payment"
        };
        taxtrans.Execute(true);
    }

    public bool HasPermission(IEntity entity, GroupPermission permission);
    public static IEntity? FindByApiKey(string apikey)
    {
        IEntity? entity = DBCache.GetAll<Group>().FirstOrDefault(x => x.Api_Key == apikey);
        if (entity is null) {
            entity = DBCache.GetAll<User>().FirstOrDefault(x => x.Api_Key == apikey);
        }
        return entity;
    }
}