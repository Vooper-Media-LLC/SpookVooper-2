using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Permissions;

namespace SV2.Database.Models.Users;

public enum Rank
{
    Spleen = 1,
    Crab = 2,
    Gaty = 3,
    Corgi = 4,
    Oof = 5,
    Unranked = 6
}

public class User : IEntity
{
    [Key]
    [EntityId]
    public string Id { get; set; }

    [Column(TypeName = "bigint")]

    public ulong ValourId { get; set; }

    [VarChar(64)]
    public string Name { get; set; }

    [VarChar(1024)]
    public string? Description { get; set; }
    public float Xp { get; set;}
    public int ForumXp { get; set;}
    public float MessageXp { get; set;}
    public int CommentLikes { get; set;}
    public int PostLikes { get; set;}
    public int Messages { get; set;}
    public DateTime LastSentMessage { get; set;}

    [JsonIgnore]
    [VarChar(36)]
    public string Api_Key { get; set; }
    public decimal Credits { get; set;}
    public decimal TaxAbleCredits { get; set; }
    public List<decimal>? CreditSnapshots { get; set;}
    public Rank Rank { get; set;}
    // the datetime that this user created their account
    public DateTime Created { get; set; }

    [VarChar(128)]
    public string? Image_Url { get; set; }

    [EntityId]
    public string? DistrictId { get; set;}

    public bool IsMinister(Ministers minister)
    {
        return false;
    }

    public static User? FindByName(string name)
    {
        return DBCache.GetAll<User>().FirstOrDefault(x => x.Name == name);
    }
    
    public static User? Find(string Id)
    {
        return DBCache.Get<User>(Id);
    }

    public bool HasPermissionWithKey(string apikey, GroupPermission permission)
    {
        if (apikey == Api_Key) {
            return true;
        }
        return false;
    }

    public bool HasPermission(IEntity entity, GroupPermission permission)
    {
        if (entity.Id == Id) {
            return true;
        }
        return false;
    }

    public User(string name, ulong valourId)
    {
        Id = "u-"+Guid.NewGuid().ToString();
        ValourId = valourId;
        Name = name;
        Xp = 0;
        ForumXp = 0;
        MessageXp = 0;
        Messages = 0;
        PostLikes = 0;
        CommentLikes = 0;
        Api_Key = Guid.NewGuid().ToString();
        Credits = 0.0m;
        CreditSnapshots = new();
        Rank = Rank.Unranked;
        Created = DateTime.UtcNow;
    }
}