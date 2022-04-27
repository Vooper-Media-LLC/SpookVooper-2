using System.Threading.Tasks;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Factories;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Users;
using SV2.Database.Models.Items;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SV2.Managers;


public class Recipe
{
    public string Name { get; set; }
    public Dictionary<string, double> Inputs { get; set; }
    public Dictionary<string, double> Outputs { get; set; }
    public double ProcessingCost { get; set; }
}

public class Material_Group
{
    public string Name { get; set; }
    public List<string> Materials { get; set; }
}

public class TopLevelResources
{
    public List<Material_Group> Material_Groups { get; set; }
    public List<Recipe> Recipes { get; set; }
}

public static class ResourceManager 
{
    static public List<string> Resources = new();
    static public List<Recipe> Recipes = new();
    static public List<Material_Group> Material_Groups = new();

    public static async Task Load()
    {
        TopLevelResources toplevelresource = await JsonSerializer.DeserializeAsync<TopLevelResources>(File.OpenRead("./Managers/resources.json"));

        Material_Groups = toplevelresource.Material_Groups;

        Resources = toplevelresource.Material_Groups.SelectMany(x => x.Materials).ToList();

        Recipes = toplevelresource.Recipes;

        // need to create item definitions

        foreach(string Resource in Resources)
        {
            TradeItemDefinition? def = DBCache.GetAll<TradeItemDefinition>().FirstOrDefault(x => x.OwnerId == "g-vooperia" && x.Name == Resource);

            if (def is null) {
                // now we need to create a definition for this resource
                def = new TradeItemDefinition("g-vooperia", Resource);

                await DBCache.Put<TradeItemDefinition>(def.Id, def);
                await VooperDB.Instance.TradeItemDefinitions.AddAsync(def);
            }
        }
        await VooperDB.Instance.SaveChangesAsync();
    }
}