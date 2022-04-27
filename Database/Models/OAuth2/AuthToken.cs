namespace SV2.Database.Models.OAuth2;

public class AuthToken
{
    /// <summary>
    /// The ID of the authentification key is also the secret key. Really no need for another random gen.
    /// (is sha256)
    /// </summary>
    public string Id { get; set; }

    [GuidID]
    public string AppId { get; set; }

    [EntityId]
    public string UserId { get; set; }
    public string Scope { get; set; }
    public DateTime Time { get; set; }
}