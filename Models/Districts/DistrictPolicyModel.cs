using System.ComponentModel.DataAnnotations.Schema;


namespace SV2.Models.Districts;

public class DistrictPolicyModel
{
    public List<TaxPolicy> TaxPolicies { get; set; }
    public List<UBIPolicy> UBIPolicies { get; set; }

    public string DistrictId { get; set; }

    [NotMapped]
    public District District {
        get {
            return DBCache.Get<District>(DistrictId)!;
        }
    }

    public DistrictPolicyModel()
    {
        
    }

    public void AddUBIPolicy(Rank? rank, string DistrictId)
    {
        UBIPolicy pol = new();
        pol.DistrictId = DistrictId;
        if (rank is null) {
            pol.Anyone = true;
        }
        else {
            pol.ApplicableRank = rank;
        }
        pol.Id = Guid.NewGuid().ToString();

        UBIPolicy? oldpol = DBCache.GetAll<UBIPolicy>().FirstOrDefault(x => x.DistrictId == DistrictId && x.ApplicableRank == rank);
        if (oldpol is not null) {
            pol.Rate = oldpol.Rate;
        }

        UBIPolicies.Add(pol);
    }

    public void AddTaxPolicy(string DistrictId, TaxType type, decimal min = 0.0m, decimal max = 99999999.0m)
    {
        TaxPolicy pol = new();
        pol.Id = Guid.NewGuid().ToString();
        pol.DistrictId = DistrictId;
        pol.Rate = 0.0m;
        pol.taxType = type;
        pol.Minimum = min;
        pol.Maximum = max;
        pol.Collected = 0.0m;
        TaxPolicies.Add(pol);
    }

    public DistrictPolicyModel(District district)
    {
        DistrictId = district.Id;
        TaxPolicies = new();
        UBIPolicies = new();
        AddUBIPolicy(null, district.Id);
        AddUBIPolicy(Rank.Unranked, district.Id);
        AddUBIPolicy(Rank.Oof, district.Id);
        AddUBIPolicy(Rank.Corgi, district.Id);
        AddUBIPolicy(Rank.Gaty, district.Id);
        AddUBIPolicy(Rank.Crab, district.Id);
        AddUBIPolicy(Rank.Spleen, district.Id);
        UBIPolicies.Reverse();

        IEnumerable<TaxPolicy> oldpols = DBCache.GetAll<TaxPolicy>().Where(x => x.DistrictId == district.Id).OrderBy(x => x.taxType);
        if (oldpols.Count() > 0) {
            foreach(TaxPolicy pol in oldpols) {
                TaxPolicies.Add(pol);
            }
        }
        else {
            // Do other taxe
             
            AddTaxPolicy(district.Id, TaxType.Payroll);
            AddTaxPolicy(district.Id, TaxType.Sales);
            AddTaxPolicy(district.Id, TaxType.StockBought);
            AddTaxPolicy(district.Id, TaxType.StockSale);
            AddTaxPolicy(district.Id, TaxType.Transactional);
            AddTaxPolicy(district.Id, TaxType.Balance);
            AddTaxPolicy(district.Id, TaxType.Wealth);

            // do personal tax brackets

            for (int i = 0; i < 4; i++)
            {
                AddTaxPolicy(district.Id, TaxType.PersonalIncome);
            }


           

            // do corporate tax brackets

            for (int i = 0; i < 4; i++)
            {
                AddTaxPolicy(district.Id, TaxType.CorporateIncome);
            }
        }
    }
}