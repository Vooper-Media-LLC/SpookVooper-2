using System.ComponentModel.DataAnnotations.Schema;

using SV2.Helpers;

namespace SV2.Database.Models.Government;

public enum ElectionType
{
    Senate = 1,
    PM = 2,
    President = 3,
    DistrictGovernor = 4
}

public class ResultData
{
    public User Candidate { get; set; }
    public int Votes { get; set; }

    public ResultData(User cand, int votes)
    {
        this.Candidate = cand;
        this.Votes = votes;
    }
}

public class Election
{
    [GuidID]
    public string Id { get; set; }

    // District the election is for
    [VarChar(64)]
    public string? DistrictId { get; set; }

    [NotMapped]
    public District District {
        get {
            return DBCache.Get<District>(DistrictId);
        }
    }

    // Time the election began
    public DateTime Start_Date { get; set; }

    // Time the election ended
    public DateTime End_Date { get; set; }

    // The resulting winner of the election
    [EntityId]
    public string? WinnerId { get; set; }

    [NotMapped]
    public User Winner { 
        get {
            return DBCache.Get<User>(WinnerId)!;
        }
    }

    [Column(TypeName = "CHAR(38)[]")]
    public List<string> ChoiceIds { get; set; }

    // False if the election has been ended
    [NotMapped]
    public bool Active { 
        get {
            return DateTime.UtcNow < End_Date;
        }
    }

    // The kind of election this is
    public ElectionType Type { get; set; }

    public string GetElectionTitle()
    {
        if (District is not null) {
            return $"The {District.Name} Senate Election";
        }

        else if (Type == ElectionType.PM) {
            return $"The Prime Minister Election";
        }

        else if (Type == ElectionType.President) {
           return $"The Presidential Election";
        }
        return "";
    }

    public Election()
    {

    }

    public Election(DateTime start_date, DateTime end_date, List<string> choiceids, string? districtid, ElectionType type)
    {
        Id = Guid.NewGuid().ToString();
        DistrictId = districtid;
        Start_Date = start_date;
        End_Date = end_date;
        ChoiceIds = choiceids;
        Type = type;
    }
}