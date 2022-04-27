using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;

namespace SV2.Database.Models.Military;

public enum RegimentType
{
    Infantry = 1,
    Artillery = 2,
    Tank = 3,
    Mech = 4,
}

public enum DivisionEquipmentType
{
    Gun = 1,
    // 1k units will use 1 ammo per hour of fighting
    Ammo = 2
}

// Represents the current equipment of the division
public class DivisionEquipment
{
    [Key]
    [GuidID]
    public string Id { get; set;}
    public DivisionEquipmentType Type { get; set;}
    
    [GuidID]
    public string tradeItemId { get; set; }

    [ForeignKey("tradeItemId")]
    // the item that is currently selected to be used
    public TradeItem tradeItem { get; set;}

    [GuidID]
    public string DivisionId { get; set; }
    
    [ForeignKey("DivisionId")]
    public Division Division { get; set; }
}

// NOTE: there can only be 1 Regiment of a type per Division
// an Infantry Regiment might have 5k or 500k troops in it
public class Regiment
{
    [Key]
    [GuidID]
    public string Id { get; set;}
    public RegimentType Type { get; set;}
    
    // number of things in this regiment
    // for example in an Infantry Regiment, Count will be the number of soldiers
    // only allowed values are in 1k increments
    public int Count { get; set;}

    [GuidID]
    public string DivisionId { get; set; }
    
    [ForeignKey("DivisionId")]
    public Division Division { get; set; }

    public List<List<int>> GetEquipmentNeeds()
    {
        // NOTE: 1 of any equipment is enough for 1k troops that uses that equipment
        // for example 100k Infantry needs 100 Guns & 100 Ammo.
        switch (Type)
        {
            case RegimentType.Infantry:
                return new List<List<int>> {
                    new List<int> {
                        (int)DivisionEquipmentType.Ammo,
                        Count / 1000
                    },
                    new List<int> {
                        (int)DivisionEquipmentType.Gun,
                        Count / 1000
                    }
                };
        }
        return new List<List<int>> {};
    }
}

public class Division : IHasOwner
{
    [Key]
    [GuidID]
    public string Id { get; set; }

    [InverseProperty("Division")]
    public ICollection<DivisionEquipment> Equipment { get; set; }

    [InverseProperty("Division")]
    public ICollection<Regiment> Regiments { get; set; }
    // current manpower in this division
    public int ManPower { get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    [EntityId]
    public string OwnerId { get; set; }

    [NotMapped]
    public IEntity Owner { 
        get {
            return IEntity.Find(OwnerId)!;
        }
    }

    // current strength of the division
    public decimal Strength { get; set; }

    public decimal Xp { get; set; }

    [NotMapped]
    public int Level {
        get {
            return GetLevel(Xp);
        }
    }
    
    public int GetLevel(decimal xp)
    {
        if (xp > 10_000) {
            return 4;
        }
        if (xp > 5_000) {
            return 3;
        }
        if ( xp > 2_000) {
            return 2;
        }
        return 1;
    }

    // the id of the Province that this unit is currently in
    public int Province { get; set; }

    public decimal GetAttack()
    {
        decimal attack = 0;
        foreach(Regiment regiment in Regiments) 
        {
            switch (regiment.Type) {
                case RegimentType.Infantry:
                    attack += regiment.Count / 1000.0m;
                    break;
                case RegimentType.Artillery:
                    attack += regiment.Count / 1000.0m * 9.0m;
                    break;
                case RegimentType.Tank:
                    attack += regiment.Count / 1000.0m * 15.0m;
                    break;
                case RegimentType.Mech:
                    attack += regiment.Count / 1000.0m * 60.0m;
                    break;
            }
        }
        attack *= Strength;
        return attack;
    }

    public decimal GetStrength()
    {
        // Strength is computed as which of the following has the lowest ratio:
        // 1. ManPower / ManPowerNeeded
        // 2. Equipment in storage / EquipmentNeeded
        
        int totalManPowerNeeded = Regiments.Sum(x => x.Count);
        decimal manPowerStrength = ManPower/totalManPowerNeeded;
        decimal totalEquipmentNeed = 0;
        decimal currentequipment = 0;
        foreach (Regiment regiment in Regiments) {
            foreach(List<int> equipmentNeed in regiment.GetEquipmentNeeds()) {
                totalEquipmentNeed += (decimal)equipmentNeed[1];
                currentequipment += Math.Min(equipmentNeed[1], Equipment.First(x => (int)x.Type == equipmentNeed[0]).tradeItem.Amount);
            }
        }
        decimal equipmentStrength = currentequipment/totalEquipmentNeed;
        return Math.Min(manPowerStrength, equipmentStrength);
    }
}