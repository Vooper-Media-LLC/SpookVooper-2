using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Economy;

public enum TaxType
{
    // PersonalIncome and CorporateIncome are paid daily
    Transactional = 1,
    Sales = 2,
    StockSale = 3,
    StockBought = 4,
    Payroll = 5,
    Balance = 6,
    Wealth = 7,
    // only the imperial government can use this one
    Inactivity = 8,
    None = 9,
    PersonalIncome = 10,
    CorporateIncome = 11,
    GroupIncome = 12,
}

public class TaxPolicy
{
    [Key]
    [GuidID]
    public string Id { get; set; }

    [VarChar(64)]
    public string? Name { get; set; }
    public decimal Rate { get; set; }
    
    // should be null if this tax policy is by Vooperia
    [EntityId]
    public string? DistrictId { get; set; }
    public TaxType taxType { get; set; }

    // the min amount after which the tax has effect
    // example for Minimum and Maximum
    // if a sales tax has a min of $1 and a max of $3 then
    // If I sell a apple for $2, then $1 will be subjected to the Rate
    // If I sell a apple for $4, then $2 will be subjected to the Rate
    public decimal Minimum { get; set; }
    // the max amount after which the tax no longer has effect
    public decimal Maximum { get; set; }
    // amount this tax has collected in the current month
    public decimal Collected { get; set; }

    public decimal GetTaxAmount(decimal amount) {
        if (amount < Minimum) {
            return 0.0m;
        }
        if (Maximum != 0.0m) {
            amount = Math.Min(Maximum, amount);
        }
        return amount * (Rate / 100.0m);
    }
}