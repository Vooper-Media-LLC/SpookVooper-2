using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SV2.Database.Models.OAuth2;

public class OauthApp
{
    [Key]
    [GuidID]
    public string Id { get; set; }
    public string Secret { get; set; }

    [EntityId]
    public string OwnerId { get; set; }
    public int Uses { get; set; }
    public string Name { get; set; }
    public string Image_Url { get; set; }
}